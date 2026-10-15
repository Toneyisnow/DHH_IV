using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public static class SaveStore {
  public static string TestDirectory;
  public static string DirectoryPath { get { if(TestDirectory!=null)return TestDirectory;return Path.Combine(Application.persistentDataPath,"Saves"); } }
  public static string PathFor(int slot,bool auto=false){return Path.Combine(DirectoryPath,"slot"+slot+(auto?".auto":"")+".json");}
  static string Hash(string s){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(s))).Replace("-","");}
  public static void Save(GameState s,bool auto=false){Directory.CreateDirectory(DirectoryPath);s.lastUtc=DateTime.UtcNow.ToString("O");string payload=JsonUtility.ToJson(s),json=JsonUtility.ToJson(new SaveEnvelope{payload=payload,hash=Hash(payload)});string path=PathFor(s.slot,auto),temp=path+".tmp";File.WriteAllText(temp,json,Encoding.UTF8);if(File.Exists(path))File.Replace(temp,path,path+".bak");else File.Move(temp,path);}
  public static GameState Load(int slot,bool auto=false,bool backup=false){string p=PathFor(slot,auto)+(backup?".bak":"");var env=JsonUtility.FromJson<SaveEnvelope>(File.ReadAllText(p));if(env==null||env.hash!=Hash(env.payload))throw new Exception("存档校验失败，请使用备份恢复");if(env.version>1)throw new Exception("此存档来自更新版本");var s=JsonUtility.FromJson<GameState>(env.payload);if(s.version!=1)throw new Exception("不支持的存档版本，原档已保留");return s;}
 }
}
