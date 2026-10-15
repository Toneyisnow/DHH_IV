using System;
using System.IO;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// User preferences are separate from shipped defaults and from per-slot progression.
public static class SettingsStore {
    public static string PathName=>Path.Combine(UserDataPaths.Settings,"user.json");
    public static Settings Load(Catalog catalog,Settings legacy=null){
        if(!File.Exists(PathName))return legacy==null?StateFactory.Settings(catalog):CatalogLoader.Copy(legacy);
        try{return Read(PathName);}catch(Exception original){
            if(File.Exists(PathName+".bak")){Debug.Log("Settings backup recovery: "+original.Message);return Read(PathName+".bak");}
            throw new InvalidDataException("User settings are invalid; original retained: "+PathName,original);
        }
    }
    static Settings Read(string path){var value=JsonUtility.FromJson<Settings>(File.ReadAllText(path));Validate(value);return value;}
    static void Validate(Settings s){
        if(s==null)throw new InvalidDataException("Empty settings");
        foreach(var f in typeof(Settings).GetFields())if(f.FieldType==typeof(float)){
            float v=(float)f.GetValue(s);if(float.IsNaN(v)||float.IsInfinity(v)||v<0)throw new InvalidDataException("Invalid setting "+f.Name);
        }
        if(s.textSpeed<=0||s.moveSpeed<=0||s.battleSpeed<=0||s.brightness<=0)throw new InvalidDataException("Invalid settings speed/brightness");
    }
    public static void Save(Settings value){Validate(value);AtomicJsonFile.Write(PathName,JsonUtility.ToJson(value));}
}
}
