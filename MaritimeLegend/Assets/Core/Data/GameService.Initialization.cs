using System;
using System.Linq;
namespace MaritimeLegend.Domain { public partial class GameService {
        public static GameState New(Catalog c,int slot,int home,string mode,Appearance look,string name,uint seed){var s=StateFactory.Game(c);s.slot=slot;s.home=s.city=home;s.mode=mode;s.appearance=look.accessoryIds.Length==0?StateFactory.Appearance(c):look;s.name=name;s.seed=seed==0?c.initial.game.seed:seed;s.ownerId=Guid.NewGuid().ToString("N");foreach(var p in c.ports){var city=new CityState{id=p.id,license=p.id==home,warehouse=StateFactory.Warehouse(c),factories=new int[c.industries.Length]};foreach(var product in c.products){
var entry=new MarketEntry{product=product.id,stock=0,qualityStock=new int[c.market.qualityFactors.Length]};
if(product.category=="R"&&p.raw.Contains(product.id)){
int level5=c.market.initialRawLevel5Stock;
int min=(int)Math.Ceiling(level5*c.market.initialRawLevel4MinPercent/100.0);
int max=(int)Math.Floor(level5*c.market.initialRawLevel4MaxPercent/100.0);
int level4=min+Math.Min(max-min,(int)(QualityRoller.Random(ref s.seed)*(max-min+1)));
entry.ChangeStock(c.initial.creation.starterMaterialQuality,level5);entry.ChangeStock(c.balance.economy.initialSecondaryQuality,level4);
}city.market.Add(entry);}s.cities.Add(city);}while(s.startingIndustries.Count<c.initial.creation.startingIndustryCount){int i=(int)(QualityRoller.Random(ref s.seed)*c.industries.Length)%c.industries.Length;if(!s.startingIndustries.Contains(i))s.startingIndustries.Add(i);}foreach(int skill in c.initial.creation.startingSkillIndices)s.skillLevels[skill]=c.initial.creation.startingSkillLevel;s.cities[home].factories[s.startingIndustries[0]]=c.initial.creation.startingFactoryLevel;s.ships.Add(StateFactory.Ship(c,s.ownerId));s.visited.Add(home);s.discovered.Add(home);var g=new GameService(c,s);foreach(var r in c.recipes.Where(x=>x.industry==s.startingIndustries[0]&&x.minLevel==c.initial.creation.starterRecipeLevel).Take(c.initial.creation.starterRecipeCount))foreach(var input in r.inputs.Where(x=>c.Product(x.product).category=="R"))g.Inventory.Add(s.cities[home].warehouse,input.product,c.initial.creation.starterMaterialQuality,input.quantity*c.initial.creation.starterMaterialBatches,home);g.Emit("home");return s;}
}}
