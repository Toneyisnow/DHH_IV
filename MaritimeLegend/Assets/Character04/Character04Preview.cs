using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace MaritimeLegend.CharacterArt {
 public sealed class Character04Preview : MonoBehaviour {
  public GameObject shortModel,longModel;
  public AnimationClip[] shortClips,longClips;
  public Camera previewCamera;
  public Material red,navy,teal;
  int coat,hair,color,fabric,action; float time,yaw=180;bool playing=true,closeup;
  readonly string[] actions={"Idle","Turn","HeadTurn","Gesture","Nod","Walk"};
  readonly string[] fabrics={"Linen","Silk","Leather"};
  Renderer[] shortRenderers,longRenderers;
  public static bool IsHair(string n)=>n.StartsWith("Hair fitted")||n.StartsWith("Swept fringe")||n.StartsWith("Side lock")||n.StartsWith("Back lock");
  public static bool IsCoat(string n)=>n.StartsWith("Outer coat")||n.StartsWith("Gold hem")||n.StartsWith("Shoulder mantle")||n.StartsWith("Mantle clasp");
  void Awake(){shortRenderers=shortModel.GetComponentsInChildren<Renderer>(true);longRenderers=longModel.GetComponentsInChildren<Renderer>(true);ApplyWardrobe();}
  IEnumerator Start(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-character04Smoke")>=0)yield return Smoke();}
  string ClipName(){string a=actions[action];if(a=="Turn")return "Turn_"+fabrics[fabric]+(hair==1?"_Long":"");if(a=="HeadTurn")return "HeadTurn_"+(hair==1?"Long":"Bob");return a;}
  public static AnimationClip Find(AnimationClip[] clips,string name){foreach(var c in clips)if(c.name==name||c.name.EndsWith("|"+name)||c.name.EndsWith("_"+name))return c;return null;}
  void Update(){if(playing)time+=Time.deltaTime;Sample();}
  void Sample(){string name=ClipName();var a=Find(shortClips,name);var b=Find(longClips,name);if(a!=null)a.SampleAnimation(shortModel,time%a.length);if(b!=null)b.SampleAnimation(longModel,time%b.length);transform.localRotation=Quaternion.Euler(0,yaw,0);}
  void ApplyWardrobe(){
   foreach(var r in shortRenderers)r.enabled=IsHair(r.name)?hair==0:IsCoat(r.name)?coat==0:true;
   foreach(var r in longRenderers)r.enabled=IsHair(r.name)?hair==1:IsCoat(r.name)?coat==1:false;
   Material target=color==0?red:color==1?navy:teal;
   foreach(var set in new[]{shortRenderers,longRenderers})foreach(var r in set){var ms=r.sharedMaterials;for(int i=0;i<ms.Length;i++)if(ms[i]!=null&&(ms[i].name=="red"||ms[i].name=="navy"||ms[i].name=="teal"))ms[i]=target;r.sharedMaterials=ms;}
  }
  void OnGUI(){
   GUI.skin.label.fontSize=16;GUI.skin.button.fontSize=16;
   GUILayout.BeginArea(new Rect(18,18,330,Screen.height-36),GUI.skin.box);
   GUILayout.Label("CHARACTER 04 / UNITY PREVIEW");GUILayout.Label("Actual meshes + toon materials");
   GUILayout.Space(12);GUILayout.Label("Outerwear");int c=GUILayout.SelectionGrid(coat,new[]{"Short","Long"},2);
   GUILayout.Label("Hair");int h=GUILayout.SelectionGrid(hair,new[]{"Bob","Long"},2);
   GUILayout.Label("Outerwear color");int co=GUILayout.SelectionGrid(color,new[]{"Red","Navy","Teal"},3);
   GUILayout.Label("Fabric response (Turn action)");int f=GUILayout.SelectionGrid(fabric,fabrics,3);
   GUILayout.Label("Action");int a=GUILayout.SelectionGrid(action,actions,3);
   if(c!=coat||h!=hair||co!=color||f!=fabric||a!=action){coat=c;hair=h;color=co;fabric=f;action=a;time=0;ApplyWardrobe();Sample();}
   GUILayout.Space(12);if(GUILayout.Button(playing?"Pause":"Play"))playing=!playing;
   float scrub=GUILayout.HorizontalSlider(time%4,0,3.95f);if(Mathf.Abs(scrub-time%4)>.02f){time=scrub;playing=false;Sample();}
   GUILayout.Label("View rotation");yaw=GUILayout.HorizontalSlider(yaw,-180,180);
   if(GUILayout.Button(closeup?"Full body":"Face close-up")){closeup=!closeup;SetCamera();}
   GUILayout.Space(16);GUILayout.Label("Prototype: flat-color materials.\nNo concept-art texture set yet.\nBaked secondary motion.\nNo live cloth/hair collisions.");
   GUILayout.EndArea();
  }
  void SetCamera(){previewCamera.orthographicSize=closeup?.75f:2.05f;Vector3 target=new Vector3(0,closeup?2.95f:1.75f,0);previewCamera.transform.position=target+new Vector3(.9f,.3f,-8);previewCamera.transform.LookAt(target);}
  void Capture(string path){
   var rt=new RenderTexture(900,900,24);var old=RenderTexture.active;var rect=previewCamera.rect;
   previewCamera.rect=new Rect(0,0,1,1);previewCamera.targetTexture=rt;previewCamera.Render();RenderTexture.active=rt;
   var tex=new Texture2D(900,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,900,900),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());
   previewCamera.targetTexture=null;previewCamera.rect=rect;RenderTexture.active=old;Destroy(tex);rt.Release();Destroy(rt);
  }
  IEnumerator Smoke(){
   yield return null;
   foreach(var name in new[]{"Idle","Turn_Linen","Turn_Silk","Turn_Leather","Turn_Linen_Long","Turn_Silk_Long","Turn_Leather_Long","HeadTurn_Bob","HeadTurn_Long","Gesture","Nod","Walk"})
    if(Find(shortClips,name)==null||Find(longClips,name)==null)throw new Exception("Missing clip: "+name);
   foreach(var r in shortRenderers)foreach(var m in r.sharedMaterials)if(m==null||m.shader==null||!m.shader.isSupported)throw new Exception("Missing/unsupported material: "+r.name);
   for(coat=0;coat<2;coat++)for(hair=0;hair<2;hair++)for(color=0;color<3;color++)for(fabric=0;fabric<3;fabric++){ApplyWardrobe();action=1;time=1;Sample();yield return null;}
   coat=0;hair=0;color=0;fabric=0;action=0;time=0;ApplyWardrobe();Sample();
   string dir=Path.Combine(Application.dataPath,"../Documentation/Character04Evidence");Directory.CreateDirectory(dir);
   yield return new WaitForEndOfFrame();Capture(Path.Combine(dir,"unity-short.png"));yield return new WaitForSeconds(.5f);
   coat=1;hair=1;color=1;action=2;time=1;playing=false;ApplyWardrobe();Sample();
   yield return new WaitForEndOfFrame();Capture(Path.Combine(dir,"unity-long.png"));yield return new WaitForSeconds(.5f);
   Debug.Log("CHARACTER04_UNITY_SMOKE_PASS: 36 combinations, 12 clips, supported materials");Application.Quit(0);
  }
 }
}
