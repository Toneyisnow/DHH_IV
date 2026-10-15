using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace MaritimeLegend.Tests {
[Serializable] public class TestValue {public string id,usage;public double value;}
[Serializable] public class TestValues {public TestValue[] values;}
public static class TestData {
 public static string DirectoryOverride;
 static readonly Dictionary<string,TestValues> cache=new Dictionary<string,TestValues>();
 public static double Double(string suite,string id){
  if(!cache.TryGetValue(suite,out var data)){
   string folder=DirectoryOverride??Environment.GetEnvironmentVariable("MARITIME_TEST_DATA");
   if(string.IsNullOrEmpty(folder))folder=Path.Combine(Application.dataPath,"Tests","Data");
   if(!Directory.Exists(folder))folder=Path.GetFullPath(Path.Combine(Application.dataPath,"..","TestData"));
   data=JsonUtility.FromJson<TestValues>(File.ReadAllText(Path.Combine(folder,suite+".json")));
   if(data==null||data.values==null||data.values.Select(x=>x.id).Distinct().Count()!=data.values.Length)throw new InvalidDataException("Invalid test scenario "+suite);
   cache.Add(suite,data);
  }
  return data.values.Single(x=>x.id==id).value;
 }
 public static int Int(string suite,string id)=>checked((int)Double(suite,id));
 public static uint UInt(string suite,string id)=>checked((uint)Double(suite,id));
 public static float Float(string suite,string id)=>(float)Double(suite,id);
}
}
