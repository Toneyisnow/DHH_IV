using System;
using System.Linq;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Tests {
// Runs without Unity Scene, GameObject, MonoBehaviour or wall-clock waits.
public static class WorkflowSimulation {
 public static void Run(Catalog catalog){var state=GameService.New(catalog,6,0,"休闲",new Appearance(),"WorkflowTest",41);var service=new GameService(catalog,state);var workflow=service.Workflow;
  state.cities[0].factories[3]=1;var recipe=catalog.recipes.First(x=>x.output==catalog.Named("木板").id);foreach(var item in recipe.inputs)service.Inventory.Add(state.cities[0].warehouse,item.product,3,item.quantity*2,0);service.Start(recipe.id,0);if(state.orders.Count!=1)throw new Exception("Workflow order setup");var order=state.orders[0];
  int funds=state.gold;state.gold=0;int capacity=state.cities[0].warehouse.capacity;service.ExpandWarehouse();service.UpgradeShip(0);if(state.gold!=0||state.cities[0].warehouse.capacity!=capacity||state.ships[0].level!=1)throw new Exception("Upgrade affordability atomicity");state.gold=funds;service.ExpandWarehouse();service.UpgradeShip(0);if(state.gold!=funds-500||state.cities[0].warehouse.capacity!=capacity+100||state.ships[0].level!=2)throw new Exception("Core upgrade transaction");
  workflow.Advance(1,false);double p=order.progress;double time=state.worldHours;workflow.Advance(1,true);if(order.progress<=p||state.worldHours!=time)throw new Exception("Pause policy");
  p=order.progress;state.city=1;workflow.Advance(5,false);if(order.progress<=p)throw new Exception("Away production progress");
  state.settings.productionPaused=true;p=order.progress;workflow.Advance(1,true);if(order.progress!=p)throw new Exception("Explicit production pause");state.settings.productionPaused=false;
  int gold=state.gold;workflow.Enqueue(g=>g.S.gold-=1);workflow.Advance(0,true);if(state.gold!=gold-1)throw new Exception("Command queue exactly once");
  for(int h=0;h<720;h++)workflow.Advance(30,false);foreach(var city in state.cities)foreach(var market in city.market)if(market.stock<0||market.stock>catalog.market.maxStock)throw new Exception("Workflow NPC bounds");
  if(!order.completed)throw new Exception("Workflow production incomplete");workflow.Stop();time=state.worldHours;workflow.Advance(999,false);if(time!=state.worldHours)throw new Exception("Stopped workflow progressed");
 }
}
}
