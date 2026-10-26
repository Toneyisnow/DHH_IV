using System;
using System.Linq;
using MaritimeLegend.Domain;
using UnityEngine;
namespace MaritimeLegend.Editor {
public static class TradeSessionChecks {
 static void Check(bool condition,string message){if(!condition)throw new Exception("TRADE_CHECK_FAILED "+message);}
 [UnityEditor.MenuItem("Maritime Legend/Validate Trade UI Domain")]
 public static void Run(){
  var c=CatalogLoader.Load();var s=GameService.New(c,0,0,"休闲",new Appearance(),"Trade test",123);var g=new GameService(c,s);s.gold=100000;s.ships[0].cargoSlotCount=6;s.cities[0].warehouse.batches.Clear();
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
  var limited=GameService.New(c,0,0,"休闲",new Appearance(),"Slot limits",123);limited.cargo.batches.Clear();limited.cities[0].warehouse.batches.Clear();limited.gold=100000;
  var limits=new TradeSession(new GameService(c,limited));Check(limits.holds[0].cells.Count==3&&limits.holds[0].slotLimit==3,"three default ship slots");Check(limits.holds.Last().cells.Count==12,"twelve warehouse slots");
  for(int i=0;i<3;i++)Check(limits.Buy(idLocal,4,out _),"fill default ship");Check(!limits.Buy(idLocal,4,out _),"fourth ship slot not available");
  limited.ships[0].cargoSlotCount=99;Check(new TradeSession(new GameService(c,limited)).holds[0].slotLimit==6,"six maximum ship slots");
  limited.ships[0].cargoSlotCount=3;new InventoryService(c).Add(limited.cargo,idLocal,4,8,0);var overflow=new TradeSession(new GameService(c,limited));Check(overflow.holds[0].cells.Count==8,"legacy overflow retained");overflow.Select(0,7);Check(!overflow.Buy(idLocal,4,out _),"overflow cannot receive purchase");Check(overflow.Return(overflow.sales[0],out _),"legacy overflow can undo sale");
  Debug.Log("TRADE_SESSION_CHECKS_PASS");
 }
}
}
