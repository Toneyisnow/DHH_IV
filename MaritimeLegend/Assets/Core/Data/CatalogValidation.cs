using System;
using System.Linq;
namespace MaritimeLegend.Domain {
public partial class Catalog {
    void Require(bool ok,string message){if(!ok)throw new InvalidOperationException("Game data: "+message);}
    void ValidateData(){
        Require(products!=null&&recipes!=null&&ports!=null&&partners!=null&&ships!=null&&skills!=null&&events!=null&&quests!=null&&factories!=null&&industries!=null&&market!=null&&balance!=null&&initial!=null,"missing category");
        Require(industries.Length>0&&skills.Length>0&&ports.Length>0&&ships.Length>0,"empty category");
        Require(market.qualityFactors!=null&&market.qualityFactors.Length==SaveContract.QualityCount&&market.qualityFactors.All(x=>x>0&&!double.IsInfinity(x)&&!double.IsNaN(x)),"quality factors");
        Require(market.referenceStock>0&&market.maxStock>0&&market.spread>=0&&market.spread<2&&market.dailyReversion>=0&&market.dailyReversion<=1&&market.pressureMin<=market.pressureMax,"market parameters");
        Require(balance.economy.processedStockTarget<=market.maxStock&&balance.economy.processedSupplyQuality<market.qualityFactors.Length,"NPC stock/quality bounds");
        Require(factories.Length==industries.Length,"factory/industry dimensions");
        Require(ports.Select(x=>x.id).SequenceEqual(Enumerable.Range(0,ports.Length)),"ports must have contiguous IDs in manifest order");
        Require(ships.Select(x=>x.id).SequenceEqual(Enumerable.Range(0,ships.Length)),"ship IDs");
        Require(partners.Select(x=>x.id).SequenceEqual(Enumerable.Range(0,partners.Length)),"partner IDs");
        Require(skills.Select(x=>x.id).Distinct().Count()==skills.Length&&events.Select(x=>x.id).Distinct().Count()==events.Length,"duplicate skill/event ID");
        foreach(var p in products)Require(p.industry>=0&&p.industry<industries.Length&&new[]{"R","M","C"}.Contains(p.category)&&p.stackSize>0&&p.allowedCities!=null&&p.allowedCities.All(i=>i>=0&&i<ports.Length),"product "+p.id);
        foreach(var p in ports){Require(p.raw!=null&&p.raw.Distinct().Count()==p.raw.Length&&p.raw.All(id=>products.Any(x=>x.id==id&&x.category=="R")),"raw references "+p.id);Require(p.productionFeeFactors!=null&&p.productionDurationFactors!=null&&p.productionFeeFactors.Length==industries.Length&&p.productionDurationFactors.Length==industries.Length&&p.productionFeeFactors.All(x=>x>0)&&p.productionDurationFactors.All(x=>x>0),"city modifiers "+p.id);}
        foreach(var r in recipes)Require(r.industry>=0&&r.industry<industries.Length&&r.fee>=0&&r.minLevel>0&&r.foregroundSeconds>0&&r.awayCityMinutes>0&&r.offlineHours>0,"recipe "+r.id);
        foreach(var p in partners)Require(p.port>=0&&p.port<ports.Length&&p.fee>=0&&skills.Any(x=>x.id==p.skill1)&&skills.Any(x=>x.id==p.skill2),"partner "+p.id);
        foreach(var s in ships)Require(s.capacity>0&&s.speed>0&&s.price>=0&&s.slots>0,"ship "+s.id);
        foreach(var s in skills)Require(s.maxLevel>0,"skill level "+s.id);
        foreach(var f in factories)Require(f.maxLevel>0&&f.price>=0,"factory "+f.industry);
        foreach(var e in events)Require(e.effects!=null&&e.prerequisites!=null&&e.prerequisites.All(id=>events.Any(x=>x.id==id)),"event references "+e.id);
        Require(objectives!=null,"missing objectives");foreach(var field in typeof(ObjectiveProducts).GetFields())Require(products.Any(p=>p.id==(string)field.GetValue(objectives)),"objective product "+field.Name);
        var start=initial.creation;
        Require(initial.game.version==SaveContract.Version&&initial.game.seed!=0&&initial.game.gold>=0&&initial.game.cargo.capacity>0&&initial.warehouse.capacity>0&&initial.ship.hull>0&&initial.ship.food>0,"initial state");
        Require(start.startingIndustryCount>0&&start.startingIndustryCount<=industries.Length&&start.startingSkillIndices!=null&&start.startingSkillIndices.All(i=>i>=0&&i<skills.Length)&&start.startingSkillLevel>0&&start.startingFactoryLevel>0&&start.starterMaterialBatches>0,"creation defaults");
        Require(start.starterMaterialQuality>=0&&start.starterMaterialQuality<market.qualityFactors.Length&&balance.economy.initialSecondaryQuality>=0&&balance.economy.initialSecondaryQuality<market.qualityFactors.Length,"initial quality indices");
        Require(initial.appearance.accessoryIds.Length>0&&initial.route.quality>=0&&initial.route.quality<market.qualityFactors.Length&&initial.route.loops>0,"initial appearance/route");
        Require(balance.navigation.mapMinX<balance.navigation.mapMaxX&&balance.navigation.mapMinY<balance.navigation.mapMaxY,"map bounds");
        Require(balance.production.minimumEffectiveLevel<=balance.production.maxEffectiveLevel&&balance.production.partnerTrustDivisor>0,"production level range");
        foreach(var field in typeof(PartnerRolesParameters).GetFields())Require((int)field.GetValue(balance.partnerRoles)<partners.Length,"partner role "+field.Name);
        foreach(var field in typeof(SkillsParameters).GetFields().Where(f=>f.Name.EndsWith("Skill")))Require((int)field.GetValue(balance.skills)<skills.Length,"skill role "+field.Name);
        Require(balance.quests.finalPort<ports.Length&&balance.production.chemistIndustry<industries.Length,"quest/industry references");
    }
}
// Persisted format identities and PRNG bit shifts are protocol/algorithm constants, not balance values.
public static class SaveContract {public const int Version=1,QualityCount=6,LegacyMarketQuality=3;}
}
