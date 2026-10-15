using System.IO;
using UnityEngine;
namespace MaritimeLegend {
public static class UserDataPaths {
    public static string TestRoot;
    public static string Root=>TestRoot??Application.persistentDataPath;
    public static string Saves=>Path.Combine(Root,"Saves");
    public static string Settings=>Path.Combine(Root,"Settings");
}
public static class AtomicJsonFile {
    public static void Write(string path,string json){
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        string temp=path+".tmp";File.WriteAllText(temp,json,System.Text.Encoding.UTF8);
        if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);
    }
}
}
