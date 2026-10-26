using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace MaritimeLegend.Domain {
public partial class GameService {
 public void EnsureTradeInventories(){
  foreach(var ship in S.ships){if(ship.cargo==null)ship.cargo=new Inventory();ship.cargo.capacity=C.ships[ship.type].capacity;}
  // Legacy cargo always belongs to the current flagship, never reinterpret it as another ship.
  if(S.ships.Count>0)S.ships[0].cargo=S.cargo;
  foreach(var city in S.cities)foreach(var p in C.products)if(!city.market.Any(m=>m.product==p.id))city.market.Add(new MarketEntry{product=p.id,qualityStock=new int[SaveContract.QualityCount]});
 }
 public bool SeedTradeTestCargo(){
  const string flag="trade-ui-test-cargo-v1";
  if(S.eventFlags.Contains(flag))return false;
  string[] ids={"p0","trade_olive_oil","p8","trade_pottery"};
  var missing=ids.Where(id=>Inventory.Count(S.cargo,id)==0).ToArray();
  if(Inventory.Weight(S.cargo)+missing.Sum(id=>C.Product(id).weight)>S.cargo.capacity)return false;
  foreach(var id in missing)Inventory.Add(S.cargo,id,4,1,S.city);
  S.eventFlags.Add(flag);return true;
 }
}
// Draft slots never mutate the live save. All quotes and final transactions use the existing economy.
public sealed class TradeSession {
 public sealed class Item {public string product;public int quality,origin,id;public bool bought;}
 public sealed class Hold {public string name;public int ship,slotLimit;public bool warehouse,locked;public List<Item> cells=new List<Item>();}
 public sealed class Sale {public Item item;public int hold,slot;}
 public readonly List<Hold> holds=new List<Hold>();public readonly List<Sale> sales=new List<Sale>();
 public int lastHold,buyTotal,sellTotal,balance;public string error="";public GameState preview;
 readonly GameService live;readonly string baseline;int serial;GameService quoteService;
 public TradeSession(GameService game){live=game;game.EnsureTradeInventories();baseline=Signature(game.S);for(int i=0;i<game.S.ships.Count;i++){var sh=game.S.ships[i];var route=game.S.autoRoutes.FirstOrDefault(x=>x.shipId==sh.id);AddHold(new Hold{ship=i,slotLimit=Math.Max(3,Math.Min(6,sh.cargoSlotCount)),name=(i==0?"旗舰 · ":"")+game.C.ships[sh.type].name,locked=sh.busy},sh.busy&&route!=null?route.cargo:Inv(game.S,i,false));}if(game.S.cities[game.S.city].license)AddHold(new Hold{warehouse=true,slotLimit=12,name="产业园仓库"},Inv(game.S,0,true));Evaluate();}
 Inventory Inv(GameState s,int ship,bool warehouse){return warehouse?s.cities[s.city].warehouse:ship==0?s.cargo:s.ships[ship].cargo;}
 string Signature(GameState s){return s.city+"|"+s.gold+"|"+s.atSea+"|"+JsonUtility.ToJson(s.cargo)+"|"+JsonUtility.ToJson(s.cities[s.city])+"|"+string.Join("|",s.ships.Select(x=>x.id+":"+x.busy+":"+JsonUtility.ToJson(x.cargo)));}
 Item Make(Batch b){return new Item{product=b.product,quality=b.quality,origin=b.origin,id=++serial};}
 string Key(Batch b){return b.product+":"+b.quality+":"+b.origin;}
 void AddHold(Hold h,Inventory inv){
  var remaining=inv.batches.GroupBy(Key).ToDictionary(x=>x.Key,x=>x.Sum(b=>b.quantity));
  if(inv.cargoCells!=null)foreach(var b in inv.cargoCells){if(b!=null&&remaining.TryGetValue(Key(b),out int n)&&n>0){h.cells.Add(Make(b));remaining[Key(b)]--;}else h.cells.Add(null);}
  foreach(var b in inv.batches){string key=Key(b);while(remaining[key]>0){int empty=h.cells.IndexOf(null);if(empty<0)h.cells.Add(Make(b));else h.cells[empty]=Make(b);remaining[key]--;}}
  while(h.cells.Count>h.slotLimit&&h.cells[h.cells.Count-1]==null)h.cells.RemoveAt(h.cells.Count-1);
  int cells=Math.Max(h.cells.Count,h.slotLimit);
  while(h.cells.Count<cells)h.cells.Add(null);holds.Add(h);
 }
 public int Count=>sales.Count+holds.Sum(h=>h.cells.Count(x=>x!=null&&x.bought));
 public int Stock(string id,int q){return Math.Max(0,live.Market(live.S.city,id).Stock(q)-holds.Sum(h=>h.cells.Count(x=>x!=null&&x.bought&&x.product==id&&x.quality==q)));}
 public int Quote(string id,int q,bool buy){return (quoteService??live).Quote(live.S.city,id,q,buy);}
 public bool Buy(string id,int q,out int slot){slot=-1;if(lastHold<0||lastHold>=holds.Count)return false;var h=holds[lastHold];if(h.locked){error="运输中的船只无法装货";return false;}slot=h.cells.FindIndex(0,Math.Min(h.slotLimit,h.cells.Count),x=>x==null);if(slot<0){error="没有空货仓";return false;}if(Stock(id,q)<1){error="城市库存不足";return false;}var item=new Item{product=id,quality=q,origin=live.S.city,bought=true,id=++serial};h.cells[slot]=item;if(!Evaluate()){h.cells[slot]=null;string reason=error;Evaluate();error=reason;return false;}return true;}
 public bool Select(int hold,int slot){lastHold=hold;var h=holds[hold];if(h.locked){error="运输中的船只无法交易";return false;}var item=h.cells[slot];if(item==null)return false;h.cells[slot]=null;if(!item.bought)sales.Add(new Sale{item=item,hold=hold,slot=slot});Evaluate();return true;}
 public bool Return(Sale sale,out int slot){slot=-1;var h=holds[sale.hold];for(int i=sale.slot;i<h.cells.Count;i++)if(h.cells[i]==null){slot=i;break;}if(slot<0){error="原货仓及右侧没有空位，无法放回";return false;}h.cells[slot]=sale.item;sales.Remove(sale);Evaluate();return true;}
 public bool Evaluate(){
  error="";buyTotal=sellTotal=0;var copy=CatalogLoader.Copy(live.S);var game=new GameService(live.C,copy);
  // Sell first so released capacity and sale proceeds are available to the purchase side.
  foreach(var sale in sales){var item=sale.item;var inv=Inv(copy,holds[sale.hold].ship,holds[sale.hold].warehouse);var b=inv.batches.FirstOrDefault(x=>x.product==item.product&&x.quality==item.quality&&x.origin==item.origin&&x.quantity>0);if(b==null){error="待售库存已变化";return false;}var temp=new Inventory{capacity=inv.capacity};temp.batches.Add(new Batch(item.product,item.quality,1,item.origin));int amount=game.Quote(copy.city,item.product,item.quality,false);string result=game.Trade(copy.city,item.product,item.quality,1,false,temp);if(temp.batches.Count!=0){error=result;return false;}b.quantity--;inv.batches.RemoveAll(x=>x.quantity<=0);sellTotal+=amount;}
  foreach(var h in holds.Where(x=>!x.locked))foreach(var item in h.cells.Where(x=>x!=null&&x.bought)){var temp=new Inventory{capacity=Inv(copy,h.ship,h.warehouse).capacity};int amount=game.Quote(copy.city,item.product,item.quality,true);string result=game.Trade(copy.city,item.product,item.quality,1,true,temp);if(temp.batches.Count==0){error=result;return false;}buyTotal+=amount;}
  foreach(var h in holds.Where(x=>!x.locked)){var inv=Inv(copy,h.ship,h.warehouse);inv.batches.Clear();inv.cargoCells=new List<Batch>();foreach(var item in h.cells){inv.cargoCells.Add(item==null?null:new Batch(item.product,item.quality,1,item.origin));if(item!=null)game.Inventory.Add(inv,item.product,item.quality,1,item.origin);}if(game.Inventory.Weight(inv)>inv.capacity){error=h.name+"载重不足";return false;}}
  preview=copy;quoteService=game;balance=copy.gold;return true;
 }
 public bool Commit(out GameState state){state=null;if(Signature(live.S)!=baseline){error="库存或报价已变化，请关闭后重新打开交易所";return false;}if(Count==0){error="尚未选择商品";return false;}if(!Evaluate())return false;state=preview;return true;}
}
}
