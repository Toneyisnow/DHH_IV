using System;
using System.Linq;
namespace MaritimeLegend.Domain {
    public partial class GameService {
        void ScheduleEncounter() {
            S.sailing.spawnCountdown=SailingFormulas.NextEncounterSeconds(C.SeaRegion(S.sailing.regionId).eventsPerGameDay,C.balance.sailing.realSecondsPerGameDay,ref S.sailing.encounterSeed);
        }
        internal void AdvanceSeaEncounters(double seconds) {
            var sailing=S.sailing;sailing.foregroundSeconds+=seconds;
            sailing.encounters.RemoveAll(e=>e.status!="Prompting"&&e.expiresAt<=sailing.foregroundSeconds);
            // No default permissive water mask: art coordinates are not collision data.
            // Injection enables automatic spawning when a real region geometry adapter is supplied.
            if(SeaPlacement==null)return;
            sailing.spawnCountdown-=seconds;
            if(sailing.spawnCountdown>0)return;
            ScheduleEncounter();
            var region=C.SeaRegion(sailing.regionId);
            if(sailing.encounters.Count(e=>e.regionId==region.id&&!e.claimed)>=region.maxIcons)return;
            double x=C.balance.navigation.mapMinX+QualityRoller.Random(ref sailing.encounterSeed)*(C.balance.navigation.mapMaxX-C.balance.navigation.mapMinX);
            double y=C.balance.navigation.mapMinY+QualityRoller.Random(ref sailing.encounterSeed)*(C.balance.navigation.mapMaxY-C.balance.navigation.mapMinY);
            TrySpawnSeaEncounter(x,y);
        }
        public SeaEncounterState TrySpawnSeaEncounter(double x,double y) {
            if(!NormalSailing||DialogueOpen||SeaPlacement==null||double.IsNaN(x)||double.IsNaN(y)||
                x<C.balance.navigation.mapMinX||x>C.balance.navigation.mapMaxX||y<C.balance.navigation.mapMinY||y>C.balance.navigation.mapMaxY||!SeaPlacement.IsNavigable(S.sailing.regionId,x,y))return null;
            var region=C.SeaRegion(S.sailing.regionId);
            if(S.sailing.encounters.Count(e=>!e.claimed&&e.regionId==region.id)>=region.maxIcons)return null;
            var eligible=region.weights.Where(w=>C.SeaEncounter(w.encounterId).prerequisiteFlags.All(f=>S.sailing.storyFlags.Contains(f))).ToArray();
            if(eligible.Length==0)return null;
            uint seed=S.sailing.encounterSeed;double roll=QualityRoller.Random(ref seed)*eligible.Sum(w=>w.weight);
            var selected=eligible.Last();foreach(var w in eligible){roll-=w.weight;if(roll<0){selected=w;break;}}
            var def=C.SeaEncounter(selected.encounterId);
            var e=new SeaEncounterState {instanceId=S.ownerId+":sea:"+(++S.sailing.nextEncounterId),definitionId=def.id,regionId=region.id,
                seed=seed,x=x,y=y,energyCost=def.energyCost,quality=def.quality,resultKey=def.resultKey,
                expiresAt=S.sailing.foregroundSeconds+C.balance.sailing.iconLifetimeSeconds};
            if(def.rewardKind=="goods") {
                e.productId=region.rewardProducts[Math.Min(region.rewardProducts.Length-1,(int)(QualityRoller.Random(ref seed)*region.rewardProducts.Length))];
                e.quantity=region.quantityMin+Math.Min(region.quantityMax-region.quantityMin,(int)(QualityRoller.Random(ref seed)*(region.quantityMax-region.quantityMin+1)));
            }
            S.sailing.encounterSeed=seed;S.sailing.encounters.Add(e);return e;
        }
        public bool ContactSeaEncounter(string instanceId) {
            if(!NormalSailing||DialogueOpen)return false;
            var e=S.sailing.encounters.FirstOrDefault(x=>x.instanceId==instanceId);
            if(e==null||e.claimed||e.status!="Available"||e.regionId!=S.sailing.regionId||e.ignoreUntil>S.sailing.foregroundSeconds||e.expiresAt<=S.sailing.foregroundSeconds)return false;
            double distance=Math.Sqrt(Math.Pow(S.seaX-e.x,2)+Math.Pow(S.seaY-e.y,2));
            if(distance>C.balance.sailing.contactRadius)return false;
            e.status="Prompting";return true;
        }
        public string ResolveSeaEncounter(string instanceId,bool accept) {
            var e=PromptedEncounter;
            if(e==null||e.instanceId!=instanceId||e.claimed)return "sailing.event.unavailable";
            if(!accept){ReleaseEncounter(e);return "sailing.event.declined";}
            if(S.sailing.energy<e.energyCost){ReleaseEncounter(e);return "sailing.event.energy_low";}
            // Stage inventory before committing: invalid product/quality/capacity never partially pays.
            var staged=CatalogLoader.Copy(S.cargo);
            if(e.quantity>0) {
                if(e.quality<0||e.quality>=C.market.qualityFactors.Length)throw new InvalidOperationException("Invalid encounter quality");
                if(Inventory.Weight(staged)+C.Product(e.productId).weight*e.quantity>staged.capacity){ReleaseEncounter(e);return "sailing.event.cargo_full";}
                Inventory.Add(staged,e.productId,e.quality,e.quantity,S.seaFrom);
            }
            S.cargo=staged;S.sailing.energy-=e.energyCost;e.claimed=true;e.status="Resolved";
            string choice=e.instanceId+":accept";
            S.sailing.seenChoices.Add(choice);
            if(e.quantity==0&&!S.sailing.storyFlags.Contains(e.definitionId+".clue"))S.sailing.storyFlags.Add(e.definitionId+".clue");
            return e.resultKey;
        }
        void ReleaseEncounter(SeaEncounterState e) {e.status="Available";e.ignoreUntil=S.sailing.foregroundSeconds+C.balance.sailing.declineSeconds;}
    }
}
