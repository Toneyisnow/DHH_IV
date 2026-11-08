using System;
using UnityEngine;
namespace MaritimeLegend {
 [Serializable] public sealed class ManualSeaPort {
  public string id,name,art;public int gamePortId=-1;
  public float x,y,z,spawnX,spawnY,height=13,cropX,cropY,cropW=1,cropH=1,aspect=1;
  public Vector3 Position=>new Vector3(x,y,z);
  public Vector2 Spawn=>new Vector2(spawnX,spawnY);
  public float DepartureYaw {get{var d=Position-ManualSeaV2Runtime.World(Spawn);return Mathf.Atan2(d.x,d.z)*Mathf.Rad2Deg;}}
 }
 public static class ManualSeaPortCatalog {
  [Serializable] sealed class Data {public ManualSeaPort[] ports;}
  static ManualSeaPort[] ports;
  public static ManualSeaPort[] All {get{if(ports==null){var asset=Resources.Load<TextAsset>("Environment/SoutheastAsiaV2/ports");if(!asset)throw new InvalidOperationException("Missing Southeast Asia port catalog");ports=JsonUtility.FromJson<Data>(asset.text).ports;}return ports;}}
  public static ManualSeaPort Departure(int gamePortId,string name){foreach(var p in All)if((p.gamePortId>=0&&p.gamePortId==gamePortId)||p.name==name)return p;return null;}
 }
}
