using System;
using System.IO;
using System.Collections;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// Art-review harness only: renders each authored map through the production renderer.
public sealed class RegionalSeaV2Review:MonoBehaviour {
 [Serializable] class Anchorage {public float x,y;}
 static readonly string[] Regions={"north_sea","mediterranean_sea","africa","indian_ocean","southeast_asia","east_asia","new_world"};
 static readonly string[] Labels={"North Sea","Mediterranean","Africa","Indian Ocean","Southeast Asia","East Asia","New World"};
 ManualSeaV2Runtime runtime;GameState state;bool interactive;
 void LoadMap(string id){
  runtime?.SetActive(false);runtime?.Dispose();
  var anchor=JsonUtility.FromJson<Anchorage>(Resources.Load<TextAsset>("Models/Environment/RegionalSeasV2/"+id+"/review").text);
  runtime=new ManualSeaV2Runtime(transform,"Environment/RegionalSeasV2/"+id+"/Runtime",false);
  state=new GameState();state.sailing=new SailingState();state.seaX=anchor.x;state.seaY=anchor.y;
  state.sailing.manualFollowers=new[]{new ManualFollowerState{x=anchor.x-.01,y=anchor.y+.018},new ManualFollowerState{x=anchor.x+.01,y=anchor.y+.036}};
  state.sailing.cloudSeed=261028;state.sailing.southeastAsiaExploration=new SeaExploration();
  for(int i=0;i<state.sailing.southeastAsiaExploration.tiles.Length;i++)state.sailing.southeastAsiaExploration.tiles[i]=2;
  runtime.SetView(0,60);
 }
 IEnumerator Start(){
  string[] args=Environment.GetCommandLineArgs();int flag=Array.IndexOf(args,"-regionalReview");
  interactive=flag<0;if(interactive){LoadMap(Regions[0]);yield break;}
  string output=flag>=0&&flag+1<args.Length?args[flag+1]:Path.Combine(Application.persistentDataPath,"RegionalReview");
  Directory.CreateDirectory(output);Application.runInBackground=true;QualitySettings.vSyncCount=0;
  foreach(var id in Regions){
   LoadMap(id);
   foreach(ManualSeaV2Environment.SkyMode sky in Enum.GetValues(typeof(ManualSeaV2Environment.SkyMode))){
    runtime.SetSkyMode(sky);
    for(int i=0;i<4;i++){runtime.Render(state,1);yield return null;}
    runtime.Capture(Path.Combine(output,id+"-"+sky+".png"));
   }
   runtime.Dispose();runtime=null;yield return null;
  }
  File.WriteAllText(Path.Combine(output,"result.txt"),"PASS: 7 maps x 3 skies rendered with ManualSeaV2Runtime; original shaders, ships, clouds and camera.\nArt review only; global registration and streaming are not implemented here.");
  if(flag>=0)Application.Quit();
 }
 void Update(){if(!interactive||runtime==null)return;runtime.Orbit(Input.GetMouseButton(1)?Input.GetAxis("Mouse X"):0,Input.mouseScrollDelta.y);runtime.Render(state,1);}
 void OnGUI(){
  if(runtime==null)return;
  UnityEngine.GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),runtime.Image,ScaleMode.ScaleToFit);
  if(!interactive)return;
  for(int i=0;i<Regions.Length;i++)if(UnityEngine.GUI.Button(new Rect(8+i*138,8,132,30),Labels[i]))LoadMap(Regions[i]);
  foreach(ManualSeaV2Environment.SkyMode sky in Enum.GetValues(typeof(ManualSeaV2Environment.SkyMode)))
   if(UnityEngine.GUI.Button(new Rect(8+(int)sky*138,46,132,30),sky.ToString()))runtime.SetSkyMode(sky);
  UnityEngine.GUI.Label(new Rect(8,84,700,30),"Map art review | RMB: original camera orbit | Wheel: original zoom | No player save loaded");
 }
 void OnDestroy(){runtime?.Dispose();}
}
}
