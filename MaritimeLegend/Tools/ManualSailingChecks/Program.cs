// Test-only resource/serialization and managed vector adapters. Game logic is compiled from Assets.
using System;
using System.IO;
using System.Text.Json;
namespace UnityEngine {
 public static class JsonUtility {static readonly JsonSerializerOptions Options=new JsonSerializerOptions{IncludeFields=true};public static T FromJson<T>(string text)=>JsonSerializer.Deserialize<T>(text,Options);public static object FromJson(string text,Type type)=>JsonSerializer.Deserialize(text,type,Options);public static string ToJson(object value)=>JsonSerializer.Serialize(value,Options);}
 public class TextAsset {public string text;public byte[] bytes;}
 public static class Resources {public static T Load<T>(string path) where T:class {string file="Assets/Resources/"+path;return (File.Exists(file+".bytes")?new TextAsset{bytes=File.ReadAllBytes(file+".bytes")}:new TextAsset{text=File.ReadAllText(file+".json")}) as T;}}
 public static class Application {public static string dataPath=>Path.GetFullPath("Assets");public static string persistentDataPath=>"Temp/ManualSailingChecks/Saves";}
 public static class Debug {public static void Log(object value)=>Console.WriteLine(value);}
 public struct Vector2 {
  public float x,y;public Vector2(float x,float y){this.x=x;this.y=y;}
  public static Vector2 zero=>new Vector2();public static Vector2 right=>new Vector2(1,0);public static Vector2 up=>new Vector2(0,1);
  public float sqrMagnitude=>x*x+y*y;public float magnitude=>(float)Math.Sqrt(sqrMagnitude);public Vector2 normalized=>magnitude>1e-5f?this/magnitude:zero;
  public static Vector2 operator +(Vector2 a,Vector2 b)=>new Vector2(a.x+b.x,a.y+b.y);public static Vector2 operator -(Vector2 a,Vector2 b)=>new Vector2(a.x-b.x,a.y-b.y);
  public static Vector2 operator *(Vector2 a,float b)=>new Vector2(a.x*b,a.y*b);public static Vector2 operator /(Vector2 a,float b)=>new Vector2(a.x/b,a.y/b);
  public static float Dot(Vector2 a,Vector2 b)=>a.x*b.x+a.y*b.y;public static float Distance(Vector2 a,Vector2 b)=>(a-b).magnitude;
 }
 public static class Mathf {
  public const float PI=(float)Math.PI;
  public static float Min(float a,float b)=>Math.Min(a,b);public static float Min(params float[] v){float result=float.PositiveInfinity;foreach(var x in v)result=Math.Min(result,x);return result;}
  public static float Max(float a,float b)=>Math.Max(a,b);public static int Max(int a,int b)=>Math.Max(a,b);
  public static int Clamp(int v,int a,int b)=>Math.Clamp(v,a,b);public static float Clamp01(float v)=>Math.Clamp(v,0,1);
  public static int CeilToInt(float v)=>(int)Math.Ceiling(v);public static float Cos(float v)=>(float)Math.Cos(v);public static float Sin(float v)=>(float)Math.Sin(v);
 }
}
namespace UnityEditor {public class MenuItem:Attribute {public MenuItem(string text){}}}
class Program {static void Main(){MaritimeLegend.Editor.ManualSailingChecks.Run();}}
