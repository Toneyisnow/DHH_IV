using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
public static class InitialStockChecks {
static void Check(bool value,string label){if(!value)throw new Exception(label);}
[MenuItem("Maritime Legend/Run initial stock checks")]
public static void Run(){
var c=CatalogLoader.Load();c.Validate();
for(uint seed=1;seed<=MaritimeLegend.Tests.TestData.Int("InitialStockChecks","repeatCount");seed++){
var s=GameService.New(c,0,0,"休闲",new Appearance(),"Stock",seed);
var twin=GameService.New(c,0,0,"休闲",new Appearance(),"Stock",seed);
foreach(var city in s.cities)foreach(var m in city.market){
bool supplied=c.Product(m.product).category=="R"&&c.ports[city.id].raw.Contains(m.product);
Check(m.stock==m.qualityStock.Sum(),"total stock");
Check(m.qualityStock.Take(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02")).All(n=>n==0),"initial grades 0-3 empty");
Check(supplied?m.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"))==MaritimeLegend.Tests.TestData.Int("InitialStockChecks","repeatCount")&&m.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02"))>=1&&m.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02"))<=MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter04"):m.stock==0,"raw supply and ratio");
Check(m.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02"))==twin.cities[city.id].market.First(x=>x.product==m.product).Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02")),"seed repeatability");
}
Check(s.cities.SelectMany(x=>x.warehouse.batches).All(x=>x.quality==MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality")&&c.Product(x.product).category=="R"),"starter warehouse");
}
var state=GameService.New(c,0,0,"休闲",new Appearance(),"Trade",MaritimeLegend.Tests.TestData.UInt("InitialStockChecks","stockSeed"));var g=new GameService(c,state);var id=c.ports[0].raw[0];var entry=g.Market(0,id);
int four=entry.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02")),five=entry.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"));g.Trade(0,id,MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02"),1,true,state.cargo);
Check(entry.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02"))==four-1&&entry.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"))==five&&g.Inventory.Count(state.cargo,id,MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02"))==1,"grade 4 purchase");
g.Trade(0,id,MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02"),1,false,state.cargo);Check(entry.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter02"))==four,"grade 4 sale retains grade");
string before=JsonUtility.ToJson(state);g.Trade(0,id,MaritimeLegend.Tests.TestData.Int("InitialStockChecks","standardQuality"),1,true,state.cargo);Check(before==JsonUtility.ToJson(state),"absent grade transaction atomicity");
g.Trade(0,id,MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"),1,true,state.cargo);Check(entry.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"))==five-1,"grade 5 purchase");
var legacy=JsonUtility.FromJson<MarketEntry>("{\"product\":\"old\",\"stock\":37}");Check(legacy.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","standardQuality"))==MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter07")&&legacy.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"))==0,"legacy read");legacy.ChangeStock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"),MaritimeLegend.Tests.TestData.Int("InitialStockChecks","highQuality"));Check(legacy.stock==MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter09")&&legacy.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","standardQuality"))==MaritimeLegend.Tests.TestData.Int("InitialStockChecks","parameter07")&&legacy.Stock(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"))==MaritimeLegend.Tests.TestData.Int("InitialStockChecks","highQuality"),"legacy mutation");
SaveStore.TestDirectory=Path.GetFullPath("Temp/InitialStockChecks/Saves");try{SaveStore.Save(state);var loaded=SaveStore.Load(0);Check(JsonUtility.ToJson(loaded.cities[0])==JsonUtility.ToJson(state.cities[0]),"disk round trip");}finally{SaveStore.TestDirectory=null;}
state.routes.Add("0>3");g.BuyShip(1);g.AddAuto(MaritimeLegend.Tests.TestData.Int("InitialStockChecks","standardQuality"),id,1,MaritimeLegend.Tests.TestData.Int("InitialStockChecks","oversizedTrade"),1);Check(state.autoRoutes.Last().quality==MaritimeLegend.Tests.TestData.Int("InitialStockChecks","lowQuality"),"new route quality");
for(int h=0;h<MaritimeLegend.Tests.TestData.Int("InitialStockChecks","simulationHours");h++){g.MarketHour();foreach(var city in state.cities)foreach(var m in city.market)Check(m.stock==m.qualityStock.Sum()&&m.stock<=c.market.maxStock&&m.qualityStock.All(n=>n>=0),"NPC bounds");}
MaritimeLegend.Tests.WorkflowSimulation.Run(c);
Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/InitialStockTestResults.txt","PASS: 100 seeds x 8 cities; raw-only initial grades 4/5 and 1-20% ratio; seed repeatability; starter materials; grade-specific transactions; rejected purchase atomicity; legacy migration; disk save/load; automatic route quality; 720 NPC hours; workflow simulation.\n");Debug.Log("INITIAL_STOCK_CHECKS_PASS");
}
}}
