using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
public static class DataConfigurationChecks {
    static void Check(bool ok,string name){if(!ok)throw new Exception("DATA CONFIG: "+name);}
    static string Read(string path)=>File.ReadAllText(Path.Combine("Assets/Resources/Data",path+".json"));
    static void Reject(Func<string,string> read,string label){bool rejected=false;try{CatalogLoader.Load(read);}catch{rejected=true;}Check(rejected,label);}
    [MenuItem("Maritime Legend/Run data configuration checks")]
    public static void Run(){
        var c=CatalogLoader.Load(Read);
        Check(c.products.Select(x=>x.id).SequenceEqual(c.productOrder)&&c.recipes.Select(x=>x.id).SequenceEqual(c.recipeOrder),"stable catalog ordering");
        var a=GameService.New(c,0,0,c.initial.game.mode,StateFactory.Appearance(c),"Config",c.initial.game.seed);
        var b=GameService.New(c,0,0,c.initial.game.mode,StateFactory.Appearance(c),"Config",c.initial.game.seed);
        a.settings.master=0;a.cities[0].warehouse.capacity++;
        Check(b.settings.master==c.initial.settings.master&&b.cities[0].warehouse.capacity==c.initial.warehouse.capacity,"new games do not share defaults");
        Reject(path=>path=="Balance/quality"?Read(path).Replace("minimumLambda","unknownLambda"):Read(path),"unknown/missing field rejected");
        Reject(path=>path=="Balance/quality"?"{}":Read(path),"empty group rejected");
        Reject(path=>path=="Balance/production"?File.ReadAllText("Assets/Tests/Data/InvalidDivisorZero.json"):Read(path),"zero divisor rejected");
        Reject(path=>path=="Balance/production"?File.ReadAllText("Assets/Tests/Data/InvalidDivisorFraction.json"):Read(path),"fractional integer rejected");
        Reject(path=>path=="Cities/ports"?Read(path).Replace("\"raw\": [","\"raw\": [\"missing-product\","):Read(path),"invalid reference rejected");
        var changed=CatalogLoader.Copy(c);changed.balance.upgrades.shipPrice++;
        var state=GameService.New(changed,0,0,c.initial.game.mode,StateFactory.Appearance(c),"Price",c.initial.game.seed);
        var g=new GameService(changed,state);int gold=state.gold;g.UpgradeShip(0);Check(state.gold==gold-changed.balance.upgrades.shipPrice,"upgrade reads configured price");
        changed.balance.quality.baseLambda=c.balance.quality.baseLambda+1;
        var roller=new QualityRoller(changed);uint seed=state.seed;
        int quality=roller.Roll(changed.initial.creation.starterMaterialQuality,changed.initial.creation.startingSkillLevel,0,ref seed);
        Check(quality>=0&&quality<=changed.initial.creation.starterMaterialQuality,"quality parameters retain bounds");
        string previousRoot=UserDataPaths.TestRoot,previousSaves=SaveStore.TestDirectory;
        try {
            UserDataPaths.TestRoot=Path.GetFullPath(Path.Combine("Temp","DataConfigurationChecks",Guid.NewGuid().ToString("N")));SaveStore.TestDirectory=null;
            var settings=StateFactory.Settings(c);SettingsStore.Save(settings);settings.master=0;SettingsStore.Save(settings);
            Check(SettingsStore.Load(c).master==0,"settings persist independently");
            File.WriteAllText(SettingsStore.PathName,"invalid json");Check(SettingsStore.Load(c).master==c.initial.settings.master,"settings backup recovery");
            SaveStore.Save(state);SaveStore.Save(state);Check(SaveStore.Load(state.slot).gold==state.gold,"atomic save roundtrip");
            Check(Path.GetDirectoryName(SettingsStore.PathName)!=SaveStore.DirectoryPath&&!SaveStore.DirectoryPath.Contains("Resources"),"user data isolation");
            var before=state.cities.SelectMany(x=>x.market).Sum(x=>x.stock);var loaded=SaveStore.Load(state.slot);Check(before==loaded.cities.SelectMany(x=>x.market).Sum(x=>x.stock),"saved stock unaffected by defaults");
        } finally {UserDataPaths.TestRoot=previousRoot;SaveStore.TestDirectory=previousSaves;}
        Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/DataConfigurationTestResults.txt","PASS: split loader; unknown/missing fields; zero divisors; integer types; broken product references; independent defaults; configured upgrade cost; quality bounds; isolated user data; settings backup recovery; atomic save roundtrip.\n");
        Debug.Log("DATA_CONFIGURATION_CHECKS_PASS");
    }
}
}
