// Test-only Unity serialization/resource adapter; not part of the player.
using System;
using System.IO;
using System.Text.Json;
namespace UnityEngine {
 public static class JsonUtility {static JsonSerializerOptions options=new JsonSerializerOptions{IncludeFields=true};public static T FromJson<T>(string value)=>JsonSerializer.Deserialize<T>(value,options);public static string ToJson(object value)=>JsonSerializer.Serialize(value,options);}
 public class TextAsset {public string text;}
 public static class Resources {public static T Load<T>(string path) where T:class =>new TextAsset{text=File.ReadAllText("Assets/Resources/"+path+".json")} as T;}
 public static class Application {public static string dataPath=>Path.GetFullPath("Assets");public static string persistentDataPath=>"Temp/StockHarness";}
 public static class Debug {public static void Log(object text)=>Console.WriteLine(text);}
}
namespace UnityEditor {public class MenuItem:Attribute {public MenuItem(string value){}}}
class Program {static void Main(string[] args){MaritimeLegend.Editor.InitialStockChecks.Run();MaritimeLegend.Editor.DataConfigurationChecks.Run();if(Array.IndexOf(args,"--campaign")>=0)MaritimeLegend.Editor.DomainChecks.Run();}}
