using System;
using System.Linq;
using MaritimeLegend.Domain;
using UnityEngine;
namespace MaritimeLegend.Editor {
public static class TradeSessionChecks {
 static void Check(bool condition,string message){if(!condition)throw new Exception("TRADE_CHECK_FAILED "+message);}
 [UnityEditor.MenuItem("Maritime Legend/Validate Trade UI Domain")]
 public static void Run(){
  var c=CatalogLoader.Load();var s=GameService.New(c,0,0,"休闲",new Appearance(),"Trade test",123);var g=new GameService(c,s);s.gold=100000;
  Check(g.SeedTradeTestCargo(),"seed applied");string seeded=JsonUtility.ToJson(s);Check(!g.SeedTradeTestCargo()&&seeded==JsonUtility.ToJson(s),"seed once");
  foreach(var id in new[]{"p0","trade_olive_oil","p8","trade_pottery"})Check(g.Inventory.Count(s.cargo,id)>0,"requested test cargo "+id);
  var cart=new TradeSession(g);int wheat=cart.holds[0].cells.FindIndex(x=>x!=null&&x.product=="p0");var original=cart.holds[0].cells[wheat];Check(cart.Select(0,wheat),"sell staging");Check(cart.holds[0].cells[wheat]==null,"sale frees exact slot");
  string idLocal=c.ports[0].raw[0];Check(cart.Buy(idLocal,4,out int target)&&target==wheat,"buy reuses freed slot");Check(cart.Return(cart.sales[0],out int returned)&&returned>wheat,"undo sale moves right");
  Check(seeded==JsonUtility.ToJson(s),"draft does not modify live save");Check(cart.Commit(out var next),"commit works");var expected=CatalogLoader.Copy(s);new GameService(c,expected).Trade(0,idLocal,4,1,true,expected.cargo);Check(next.gold==expected.gold&&new InventoryService(c).Count(next.cargo,idLocal,4)>g.Inventory.Count(s.cargo,idLocal,4),"real inventory and money: "+next.gold+" / "+s.gold+" inventory "+new InventoryService(c).Count(next.cargo,idLocal,4)+" / "+g.Inventory.Count(s.cargo,idLocal,4));Check(new InventoryService(c).Count(next.cargo,"p0",4)==g.Inventory.Count(s.cargo,"p0",4),"returned cargo preserved");
  var poor=CatalogLoader.Copy(s);poor.gold=0;var poorCart=new TradeSession(new GameService(c,poor));Check(!poorCart.Buy(idLocal,4,out _)&&poorCart.Count==0,"unaffordable atomic rejection");
  var stale=new TradeSession(g);stale.Select(0,0);s.gold++;Check(!stale.Commit(out _),"concurrent state change rejected");s.gold--;
  var clone=CatalogLoader.Copy(next);var reopened=new TradeSession(new GameService(c,clone));Check(reopened.holds[0].cells[returned].product==original.product,"positions survive serialization");
  var noLicense=CatalogLoader.Copy(s);noLicense.cities[0].license=false;Check(new TradeSession(new GameService(c,noLicense)).holds.All(h=>!h.warehouse),"warehouse requires local license");
  var warehouse=new TradeSession(g);warehouse.lastHold=warehouse.holds.Count-1;Check(warehouse.holds.Last().warehouse&&warehouse.Buy(idLocal,4,out _),"direct warehouse buy");Check(warehouse.Commit(out var stored)&&new InventoryService(c).Count(stored.cities[0].warehouse,idLocal,4)>g.Inventory.Count(s.cities[0].warehouse,idLocal,4),"warehouse commit");
  var fleet=CatalogLoader.Copy(s);var fleetGame=new GameService(c,fleet);fleetGame.BuyShip(0);fleetGame.EnsureTradeInventories();
  var second=new TradeSession(fleetGame);second.lastHold=1;Check(second.Buy(idLocal,4,out _),"second ship purchase");Check(second.Commit(out var fleetNext),"second ship commit");
  Check(new InventoryService(c).Count(fleetNext.ships[1].cargo,idLocal,4)==1,"second ship owns cargo");Check(new InventoryService(c).Count(fleetNext.cargo,idLocal,4)==fleetGame.Inventory.Count(fleet.cargo,idLocal,4),"flagship unchanged");
  var switched=new GameService(c,fleetNext);switched.Flagship(1);Check(switched.Inventory.Count(fleetNext.cargo,idLocal,4)==1&&switched.Inventory.Count(fleetNext.ships[1].cargo,"p0",4)>0,"flagship switch preserves both holds");
  Debug.Log("TRADE_SESSION_CHECKS_PASS");
 }
}
}
