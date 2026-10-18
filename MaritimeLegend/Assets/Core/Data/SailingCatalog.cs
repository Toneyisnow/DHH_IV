using System;
using System.Linq;
namespace MaritimeLegend.Domain {
    public partial class Catalog {
        public SeaRegionDef[] seaRegions;
        public EncounterDef[] seaEncounters;
        public BackgroundDef[] backgrounds;
        public SailingDefaults sailingDefaults;
        public LocalizationPlan localizationPlan;
        public SeaRegionDef SeaRegion(string id)=>seaRegions.Single(x=>x.id==id);
        public EncounterDef SeaEncounter(string id)=>seaEncounters.Single(x=>x.id==id);
        void ValidateSailing() {
            Require(seaRegions!=null&&seaRegions.Length==7&&seaRegions.Select(x=>x.id).Distinct().Count()==7,"seven unique sea regions");
            Require(seaEncounters!=null&&seaEncounters.Length>0&&seaEncounters.Select(x=>x.id).Distinct().Count()==seaEncounters.Length,"sea encounters");
            Require(sailingDefaults!=null&&sailingDefaults.crewByShipType.Length==ships.Length&&sailingDefaults.crewByShipType.All(x=>x>0),"mock crew defaults");
            Require(sailingDefaults.portRegions.Length==ports.Length&&sailingDefaults.portRegions.All(id=>seaRegions.Any(r=>r.id==id)),"port region mapping");
            Require(skills.Any(x=>x.id==sailingDefaults.supplySkillId),"supply skill reference");
            foreach(var r in seaRegions) {
                Require(r.eventsPerGameDay>=0&&r.eventsPerGameDay<=10&&r.maxIcons>0&&r.maxIcons<=32&&r.quantityMin>0&&r.quantityMax>=r.quantityMin,"region bounds "+r.id);
                Require(r.weights!=null&&r.weights.Length>0&&r.weights.All(w=>w.weight>0&&seaEncounters.Any(e=>e.id==w.encounterId)),"encounter weights "+r.id);
                Require(r.rewardProducts!=null&&r.rewardProducts.Length>0&&r.rewardProducts.All(id=>products.Any(p=>p.id==id)),"reward products "+r.id);
            }
            foreach(var e in seaEncounters)Require(e.energyCost>=0&&e.quality>=0&&e.quality<market.qualityFactors.Length&&e.prerequisiteFlags!=null&&!string.IsNullOrEmpty(e.promptKey)&&!string.IsNullOrEmpty(e.resultKey)&&new[]{"goods","clue"}.Contains(e.rewardKind),"encounter definition "+e.id);
            Require(backgrounds!=null&&backgrounds.Length==4&&backgrounds.Select(x=>x.id).Distinct().Count()==4&&backgrounds.All(x=>x.goldMultiplier>0&&x.supplyMultiplier>0&&x.hullMultiplier>0),"background placeholders");
            Require(localizationPlan!=null&&localizationPlan.locales.SequenceEqual(new[]{"zh-CN","en-US","zh-TW","ja-JP","ko-KR"}),"locales");
        }
    }
    [Serializable] public class SeaRegionDef {
        public string id,nameKey,mapAssetKey; public bool mock;
        public double eventsPerGameDay; public int maxIcons,quantityMin,quantityMax;
        public EncounterWeight[] weights; public string[] rewardProducts;
    }
    [Serializable] public class EncounterWeight { public string encounterId; public int weight; }
    [Serializable] public class EncounterDef {
        public string id,promptKey,resultKey,rewardKind,exclusiveGroup,followupId;
        public bool mock; public int energyCost,quality,saveVersion; public string[] prerequisiteFlags;
    }
    [Serializable] public class BackgroundDef {
        public string id,nameKey,prologueKey,materialProductId; public bool mock;
        public double goldMultiplier,supplyMultiplier,hullMultiplier; public int materialQuantity;
    }
    [Serializable] public class SailingDefaults {
        public bool mock; public string supplySkillId; public int[] crewByShipType; public string[] portRegions;
    }
    [Serializable] public class LocalizationPlan { public string sourceLocale,fallbackLocale; public string[] locales; public bool translationsComplete; }
    // All units and mock status are documented in Documentation/Sailing1016Architecture.md.
    [Serializable] public class SailingParameters {
        public double realSecondsPerGameDay,foodPerCrewDay,waterPerCrewDay,logisticsFactor;
        public double supplyReductionPerLevel,minimumSupplyFactor,energyPerDay,minimumEnergyPerDay;
        public double navigationEnergyReduction,supplyEnergyReduction,initialWater,maximumEnergy;
        public int baseDailyXp,xpPerNavigationLevel,xpPerVisionLevel;
        public double minimumDailyDistance,shortageSpeedFactor,turnDegreesPerSecond,sailDegreesPerSecond;
        public double defaultSailDegrees,declineSeconds,iconLifetimeSeconds,contactRadius;
        public double waterUnitPrice,energyUnitPrice;
    }
}
