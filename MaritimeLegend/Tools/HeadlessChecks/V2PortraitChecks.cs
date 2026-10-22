using System;
using MaritimeLegend.Domain;
using UnityEngine;

static class V2PortraitChecks {
 public static void Run(){
  void Check(bool value){if(!value)throw new Exception("V2 portrait compatibility check failed");}
  int[] expected={3,4,1,2,0,4,5,6,0,0};
  for(int i=0;i<10;i++){
   var appearance=new Appearance{baseId=i,primaryColor=7};
   string key=V2PortraitPreset.Resolve(appearance);
   Check(key==(expected[i]==0?"":expected[i].ToString("00")+"-C8"));
   Check(appearance.baseId==i&&appearance.portraitV2=="");
  }
  Check(V2PortraitPreset.Resolve(null)=="");
  Check(V2PortraitPreset.Resolve(new Appearance{baseId=-1})=="");
  var old=JsonUtility.FromJson<Appearance>("{\"baseId\":2,\"hairId\":3,\"accessoryIds\":[1,1,0,1,0]}");
  Check(V2PortraitPreset.Resolve(old)=="01-H4");
  var cape=new Appearance{baseId=6,outfitId=2,accessoryIds=new[]{0,0,1,0,1}};
  Check(V2PortraitPreset.Resolve(cape)=="05-M3");
  cape.portraitV2="08-A3";
  Check(V2PortraitPreset.Resolve(JsonUtility.FromJson<Appearance>(JsonUtility.ToJson(cape)))=="08-A3");
  Console.WriteLine("V2_PORTRAIT_COMPATIBILITY_PASS: legacy mapping, approved slot-1 mapping, unsupported identities, null/short accessories, explicit preset serialization; no save mutation.");
 }
}
