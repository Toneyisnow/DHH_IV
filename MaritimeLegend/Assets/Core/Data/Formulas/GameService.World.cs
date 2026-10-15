using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
public partial class GameService : IMarketService,IProductionService {
        public readonly GameWorkflow Workflow; public readonly Catalog C; public readonly GameState S; public readonly InventoryService Inventory; public readonly SkillResolver Skills; readonly QualityRoller quality;
        public GameService(Catalog c,GameState s){C=c;S=s;Skills=new SkillResolver(c);quality=new QualityRoller(c);Inventory=new InventoryService(c);Workflow=new GameWorkflow(this);}
        public void Tick(double real,bool paused){Workflow.Advance(real,paused);}
        internal void AdvanceWorld(double real){S.playSeconds+=real;S.worldHours+=real/C.balance.world.secondsPerHour;while(S.npcHours+1<=S.worldHours){S.npcHours++;MarketHour();}if(S.atSea){double vision=C.balance.world.visionRadius*(1+S.skillLevels[C.balance.skills.visionSkill]*C.balance.world.visionPerLevel+(HasPartner(C.balance.partnerRoles.scout)?C.balance.world.scoutVisionBonus:0)+(HasPartner(C.balance.partnerRoles.navigator)?C.balance.world.navigatorVisionBonus:0));foreach(var port in C.ports){if(!S.discovered.Contains(port.id)&&Math.Sqrt(Math.Pow(port.x-S.seaX,2)+Math.Pow(port.y-S.seaY,2))<vision){S.discovered.Add(port.id);Gain(C.balance.skills.visionSkill,C.balance.world.discoveryExperience);Log("探明港口："+port.name);}}var ship=S.ships[0];ship.food=Math.Max(0,ship.food-real*C.balance.world.foodPerSecond*(HasPartner(C.balance.partnerRoles.cook)?C.balance.world.cookFoodFactor:1));ship.hull=Math.Max(1,ship.hull-real*(ship.food>0?C.balance.world.fedHullDecay:C.balance.world.starvingHullDecay));S.windTimer-=real;if(S.windTimer<=0){S.windTimer=C.balance.world.windIntervalMin+QualityRoller.Random(ref S.seed)*C.balance.world.windIntervalRange;S.wind=QualityRoller.Random(ref S.seed)*Math.PI*2;}}}
        public static string QualityName(int q){return q==0?"S":q.ToString();}
        public static double Clamp(double v,double min,double max){return Math.Max(min,Math.Min(max,v));}
}}
