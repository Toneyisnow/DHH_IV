using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
public static class SailingArchitectureChecks {
    sealed class OpenWater : ISeaPlacementPolicy { public bool IsNavigable(string region,double x,double y)=>true; }
    static void Check(bool ok,string label){if(!ok)throw new Exception("SAILING1016: "+label);}
    static void Near(double a,double b,string label)=>Check(Math.Abs(a-b)<1e-7,label+" "+a+" != "+b);
    static GameService Fresh(Catalog c) {
        var s=GameService.New(c,0,0,"休闲",StateFactory.Appearance(c),"Sailing1016",12345);
        s.pendingEvents.Clear();s.skillLevels=new int[c.skills.Length];
        var g=new GameService(c,s);g.Depart(1);return g;
    }
    [MenuItem("Maritime Legend/Run sailing architecture checks")]
    public static void Run() {
        var c=CatalogLoader.Load();var p=c.balance.sailing;
        Check(GameCalendar.Date(0)=="1700-01-01"&&GameCalendar.Date(29)=="1700-01-30"&&GameCalendar.Date(30)=="1700-02-01"&&GameCalendar.Date(360)=="1701-01-01","360-day calendar");
        var cost=SailingFormulas.Daily(p,8,0,0,0);Near(cost.food,1.6,"example food");Near(cost.water,2.4,"example water");Near(cost.energy,6,"example energy");Check(cost.experience==5,"example XP");
        var a=Fresh(c);var b=Fresh(c);a.Tick(3*p.realSecondsPerGameDay+17,false);
        for(int i=0;i<3;i++)b.Tick(p.realSecondsPerGameDay,false);b.Tick(17,false);
        Check(a.S.sailing.calendar.elapsedGameDays==3,"catch up three days");Near(a.S.ships[0].food,100-3*cost.food,"no legacy second consumption");
        Near(a.S.ships[0].water,b.S.ships[0].water,"partition independent daily cost");Near(a.S.sailing.energy,b.S.sailing.energy,"partition independent energy");
        Check(a.ManualFleet.experience==0,"idle sailing gives no XP");
        double food=a.S.ships[0].food;var restored=new GameService(c,CatalogLoader.Copy(a.S));restored.Tick(0,false);Near(restored.S.ships[0].food,food,"restore doesn't settle twice");
        restored.Tick(p.realSecondsPerGameDay-17,false);Near(restored.S.ships[0].food,food-cost.food,"restored fraction finishes exactly once");
        var faster=Fresh(CatalogLoader.Copy(c));faster.Tick(90,false);faster.C.balance.sailing.realSecondsPerGameDay=60;
        faster.Tick(30,false);Check(faster.S.sailing.calendar.elapsedGameDays==1,"balance change preserves half-day fraction");
        a.S.sailing.lastDayDistance=p.minimumDailyDistance;a.Tick(p.realSecondsPerGameDay,false);Check(a.ManualFleet.experience==5,"travel earns one daily award");
        a.S.sailing.energy=0;a.S.sailing.lastDayDistance=p.minimumDailyDistance;a.Tick(p.realSecondsPerGameDay,false);Check(a.ManualFleet.experience==5&&a.ManualFleet.shortage,"shortage stops normal XP");
        a.Rescue();Check(a.S.sailing.energy==p.maximumEnergy&&!a.S.atSea,"rescue breaks shortage deadlock");
        var input=Fresh(c);double heading=input.S.heading;input.AdjustSail(1,.2);Near(input.S.heading,heading,"keyboard does not steer");double trim=input.S.trim;
        input.SteerToward(input.S.seaX,input.S.seaY+1,.1);Near(input.S.trim,trim,"mouse does not trim");
        Check(Math.Abs(input.S.heading-heading)<=p.turnDegreesPerSecond*.1*Math.PI/180+1e-9,"bounded turn rate");
        Check(!input.SetSailingMode("battle"),"casual mode disallows battle");input.S.mode="经典";heading=input.S.heading;
        Check(input.SetSailingMode("battle")&&input.SetSailingMode("normal"),"classic mode transitions");Near(input.S.heading,heading,"mode preserves heading");Near(input.S.trim,trim,"mode preserves sail");
        var events=Fresh(c);Check(events.TrySpawnSeaEncounter(events.S.seaX,events.S.seaY)==null,"no spawning without geometry policy");
        events.SeaPlacement=new OpenWater();
        // Force a goods fixture without changing the shipped relative weights.
        var testCatalog=CatalogLoader.Copy(c);testCatalog.SeaRegion(events.S.sailing.regionId).weights=new[]{new EncounterWeight{encounterId="wreck",weight=1}};
        events=new GameService(testCatalog,events.S){SeaPlacement=new OpenWater()};
        var e=events.TrySpawnSeaEncounter(events.S.seaX,events.S.seaY);var other=events.TrySpawnSeaEncounter(events.S.seaX,events.S.seaY);
        Check(e!=null&&e.quantity>=2&&e.quantity<=8&&e.productId!=null,"configured reward materialized at spawn");
        Check(events.ContactSeaEncounter(e.instanceId)&&!events.ContactSeaEncounter(e.instanceId),"one prompt only");
        double clock=events.S.sailing.calendar.secondsIntoDay,x=events.S.seaX,fg=events.S.sailing.foregroundSeconds;
        events.Tick(25,false);events.Sail(1,1);Near(events.S.sailing.calendar.secondsIntoDay,clock,"dialogue freezes calendar");Near(events.S.seaX,x,"dialogue freezes displacement");Near(events.S.sailing.foregroundSeconds,fg,"dialogue freezes decline clock");
        string fixedReward=e.productId;int fixedQuantity=e.quantity;uint fixedSeed=e.seed;
        events.ResolveSeaEncounter(e.instanceId,false);Check(!events.ContactSeaEncounter(e.instanceId),"decline cooldown");
        Check(events.ContactSeaEncounter(other.instanceId),"cooldown is instance local");events.ResolveSeaEncounter(other.instanceId,false);
        events.Tick(9,false);Check(!events.ContactSeaEncounter(e.instanceId),"nine seconds still cooling");events.Tick(1,false);Check(events.ContactSeaEncounter(e.instanceId),"ten foreground seconds expires");
        events.S.cargo.capacity=0;double energy=events.S.sailing.energy;
        Check(events.ResolveSeaEncounter(e.instanceId,true)=="sailing.event.cargo_full","capacity validation");Near(events.S.sailing.energy,energy,"failed reward doesn't deduct energy");
        Check(events.Inventory.Count(events.S.cargo,e.productId)==0&&!e.claimed,"failed reward doesn't grant goods");
        events.Tick(10,false);events.S.cargo.capacity=10000;events.S.sailing.energy=0;events.ContactSeaEncounter(e.instanceId);
        Check(events.ResolveSeaEncounter(e.instanceId,true)=="sailing.event.energy_low"&&!e.claimed,"insufficient energy releases prompt");
        events.Tick(10,false);events.S.sailing.energy=p.maximumEnergy;events.ContactSeaEncounter(e.instanceId);
        var snapshot=CatalogLoader.Copy(events.S);events=new GameService(testCatalog,snapshot);e=events.PromptedEncounter;
        Check(e.productId==fixedReward&&e.quantity==fixedQuantity&&e.seed==fixedSeed,"load preserves pending result");
        events.ResolveSeaEncounter(e.instanceId,true);Check(e.claimed&&e.status=="Resolved","accepted transaction completed");
        Near(events.S.sailing.energy,p.maximumEnergy-e.energyCost,"energy paid once");Check(events.Inventory.Count(events.S.cargo,e.productId)==e.quantity,"goods committed once");
        var reloaded=new GameService(testCatalog,CatalogLoader.Copy(events.S));Check(!reloaded.ContactSeaEncounter(e.instanceId),"claimed collision rejected after load");
        Check(reloaded.ResolveSeaEncounter(e.instanceId,true)=="sailing.event.unavailable","claimed reward cannot repeat");
        string previousDirectory=SaveStore.TestDirectory;
        try {
            SaveStore.TestDirectory=Path.GetFullPath(Path.Combine("Temp","Sailing1016Saves",Guid.NewGuid().ToString("N")));
            SaveStore.Save(reloaded.S);
            var disk=new GameService(testCatalog,SaveStore.Load(reloaded.S.slot));
            Check(!disk.ContactSeaEncounter(e.instanceId)&&disk.Inventory.Count(disk.S.cargo,e.productId)==e.quantity,"disk save preserves reward and claimed state");
            Near(disk.S.sailing.energy,reloaded.S.sailing.energy,"disk save preserves energy");
        } finally { SaveStore.TestDirectory=previousDirectory; }
        var old=Fresh(c).S;old.sailing=null;old.ships[0].sailingVersion=0;old.worldHours=999;
        var migrated=new GameService(c,old);Check(migrated.S.sailing.calendar.elapsedGameDays==0&&migrated.ManualFleet.lastSettledDay==0,"old save additive migration has no retroactive debt");
        migrated.S.sailing.energy=0;migrated.S.ships[0].water=0;var again=new GameService(c,CatalogLoader.Copy(migrated.S));Near(again.S.sailing.energy,0,"zero energy not mistaken for missing field");Near(again.S.ships[0].water,0,"zero water survives load");
        var factory=Fresh(CatalogLoader.Copy(c));factory.S.orders.Add(new Order{id="clock-test",recipe=c.recipes[0].id,city=0,effectiveLevel=1});
        double duration=factory.Duration(factory.S.orders[0],false);factory.Tick(1,true);Near(factory.S.orders[0].progress,1/duration,"production continues with world paused");Near(factory.S.sailing.calendar.secondsIntoDay,0,"paused game clock");
        factory.C.balance.sailing.realSecondsPerGameDay=.1;factory.Tick(1,false);Near(factory.S.orders[0].progress,2/duration,"game calendar cannot accelerate real production");
        var loc=new LocalizationService(c.localizationPlan);
        foreach(var locale in c.localizationPlan.locales)foreach(var def in c.seaEncounters) {
            Check(!loc.Text(locale,def.promptKey).StartsWith("["),"localized prompt "+locale);
            Check(!loc.Text(locale,def.resultKey,"test",8,12).StartsWith("["),"localized result "+locale);
        }
        // Repeated seeded weighted draws should track the configured relative weights.
        var counts=new int[4];var random=Fresh(c);random.SeaPlacement=new OpenWater();
        var region=c.SeaRegion(random.S.sailing.regionId);
        for(int i=0;i<10000;i++){random.S.sailing.encounters.Clear();var draw=random.TrySpawnSeaEncounter(random.S.seaX,random.S.seaY);counts[Array.FindIndex(region.weights,w=>w.encounterId==draw.definitionId)]++;}
        for(int i=0;i<counts.Length;i++)Check(Math.Abs(counts[i]/10000.0-region.weights[i].weight/(double)region.weights.Sum(w=>w.weight))<.025,"weighted distribution "+i);
        Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/Sailing1016TestResults.txt","PASS: calendar, fractional/multiple days, no duplicate settlement on load, idle/shortage XP, rescue, independent controls, modes, guarded geometry, stable rewards, per-instance cooldown, dialogue pause, atomic failure/success, additive migration, production time separation, five-locale event text, 10000 weighted draws.\n");
        Debug.Log("SAILING_1016_CHECKS_PASS");
    }
}}
