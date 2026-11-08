using System;
using System.Linq;
namespace MaritimeLegend.Domain {
 public partial class GameService {
  // Add new ports without replacing any existing warehouse, factory or progress.
  void EnsurePortStates(){
   while(S.cities.Count<C.ports.Length){
    var p=C.ports[S.cities.Count];
    var city=new CityState{id=p.id,warehouse=StateFactory.Warehouse(C),factories=new int[C.industries.Length]};
    foreach(var product in C.products){
     var entry=new MarketEntry{product=product.id,qualityStock=new int[C.market.qualityFactors.Length]};
     if(product.category=="R"&&p.raw.Contains(product.id))entry.ChangeStock(C.initial.creation.starterMaterialQuality,C.market.initialRawLevel5Stock);
     city.market.Add(entry);
    }
    S.cities.Add(city);
   }
  }
  public static int RandomSoutheastAsiaPort(Catalog c,uint seed){
   if(seed==0)seed=c.initial.game.seed;
   var ports=c.ports.Where(p=>c.sailingDefaults.portRegions[p.id]=="southeast_asia").ToArray();
   if(ports.Length==0)throw new InvalidOperationException("No Southeast Asia starting cities");
   return ports[Math.Min(ports.Length-1,(int)(QualityRoller.Random(ref seed)*ports.Length))].id;
  }
  public void ApplySoutheastAsiaStart(bool repairDeparture=false){
   if(S.southeastAsiaStartVersion>=1&&!repairDeparture)return;
   int current=S.atSea?S.seaFrom:S.city;
   bool regional=current>=0&&current<C.ports.Length&&C.sailingDefaults.portRegions[current]=="southeast_asia";
   int city=regional?current:RandomSoutheastAsiaPort(C,S.seed);
   S.southeastAsiaStartVersion=1;
   if(!regional||repairDeparture){
    S.city=city;S.seaFrom=city;S.sailing.manualMapId="";S.sailing.manualFollowers=null;
    if(S.seaTo==city)S.seaTo=C.ports.First(p=>p.id!=city&&C.sailingDefaults.portRegions[p.id]=="southeast_asia").id;
    S.seaX=C.ports[city].x;S.seaY=C.ports[city].y;
   }
   S.sailing.regionId="southeast_asia";
   if(!S.visited.Contains(city))S.visited.Add(city);
   if(!S.discovered.Contains(city))S.discovered.Add(city);
  }
 }
}
