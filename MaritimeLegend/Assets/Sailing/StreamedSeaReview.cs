using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class StreamedSeaReview:MonoBehaviour {
 StreamedSeaRuntime sea;GameState state;bool automated;string output;int checks;
 void Check(bool condition,string message){checks++;if(!condition)throw new Exception("STREAM CHECK: "+message);}
 void Place(double x,double y){state.seaX=x;state.seaY=y;state.sailing.manualFollowers=null;sea.Center(x,y);}
 IEnumerator Ready(){float start=Time.realtimeSinceStartup;do{sea.Render(state,1);Check(sea.ResidentChunks<=StreamedSeaTerrain.Capacity,"bounded residency");if(Time.realtimeSinceStartup-start>90)throw new Exception("Streaming timeout: "+sea.Diagnostic);yield return null;}while(!sea.NavigationReady||sea.PendingChunks>0);}
 IEnumerator Start(){
  var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-streamReview");automated=index>=0;output=automated?args[index+1]:Application.persistentDataPath;Directory.CreateDirectory(output);Application.runInBackground=true;
  if(automated){File.WriteAllText(Path.Combine(output,"result.txt"),"RUNNING");Application.logMessageReceived+=OnLog;}
  sea=new StreamedSeaRuntime(transform);state=new GameState{sailing=new SailingState()};state.sailing.cloudSeed=261028;var port=sea.Map.Port(6);Place(port.x,port.y);sea.SetView(0,60);
  if(!automated)yield break;
  Check(!sea.Navigation.Safe(port.x,port.y),"unloaded coast blocks navigation");yield return Ready();
  Check(sea.Navigation.Safe(port.x,port.y),"loaded departure is navigable");sea.Capture(Path.Combine(output,"departure.png"));
  foreach(var sky in new[]{ManualSeaV2Environment.SkyMode.Sunset,ManualSeaV2Environment.SkyMode.Night}){sea.SetSkyMode(sky);sea.Render(state,1);yield return null;sea.Capture(Path.Combine(output,sky+".png"));}sea.SetSkyMode(ManualSeaV2Environment.SkyMode.Sunny);
  var screen=sea.Project(new Vector2((float)state.seaX,(float)state.seaY));Check(sea.TryUnproject(screen,out var picked)&&WorldMap.Distance(picked.x,picked.y,state.seaX,state.seaY)<.02,"projection roundtrip");
  int initial=sea.ResidentChunks;
  Place(WorldMap.Width-.5,2560);yield return Ready();
  sea.Navigation.ResolveMove(state.seaX,state.seaY,state.seaX+1,state.seaY,out var x,out var y);Check(x<1&&!sea.Navigation.Loading,"east cyclic crossing");state.seaX=x;sea.Render(state,1);yield return null;sea.Capture(Path.Combine(output,"east-west-seam.png"));
  sea.Navigation.ResolveMove(x,y,x-1,y,out x,out y);Check(x>WorldMap.Width-1,"west cyclic crossing");
  Place(960,WorldMap.Height);yield return Ready();sea.Navigation.ResolveMove(state.seaX,state.seaY,state.seaX+1,state.seaY+2,out x,out y);Check(y==WorldMap.Height&&x>state.seaX,"north wall tangent");sea.Render(state,1);sea.Capture(Path.Combine(output,"north-wall.png"));
  Place(960,0);yield return Ready();sea.Navigation.ResolveMove(state.seaX,state.seaY,state.seaX-1,-2,out x,out y);Check(y==0&&x<state.seaX,"south wall tangent");
  foreach(int id in new[]{0,5,7,8,6}){var p=sea.Map.Port(id);Place(p.x,p.y);yield return Ready();sea.Orbit(120,0);sea.Render(state,1);yield return null;}
  Check(sea.Evictions>0,"distant terrain is released");Check(state.sailing.streamedExploration.Count>0,"exploration persists across regions");
  int key=WorldMap.Key(port.x,port.y);var mask=new bool[2304];for(int i=0;i<mask.Length;i++)mask[i]=i!=key;
  var failureAtlas=new StreamedSeaAtlas{version=2,worldId="regional_world_v2",ocean=mask,ports=sea.Map.ports};
  using(var retry=new StreamedSeaTerrain(failureAtlas)){retry.ResourceKey=_=>"Data/StreamedSeasV2/intentional-missing-test";var wanted=new List<int>{key};var keep=new HashSet<int>{key};float started=Time.realtimeSinceStartup;
   while(retry.Failures==0&&Time.realtimeSinceStartup-started<15){retry.Update(wanted,keep,Time.realtimeSinceStartup);yield return null;}
   Check(retry.Failures==1&&!retry.TryDistance(port.x,port.y,out _),"failed load blocks navigation");retry.ResourceKey=null;
   while(!retry.Ready(key)&&Time.realtimeSinceStartup-started<25){retry.Update(wanted,keep,Time.realtimeSinceStartup);yield return null;}
   Check(retry.Ready(key)&&retry.Failures==0,"failed resource retries successfully");
  }
  var saved=JsonUtility.FromJson<GameState>(JsonUtility.ToJson(state));Check(saved.sailing.streamedExploration.Count==state.sailing.streamedExploration.Count,"exploration save roundtrip");
  File.WriteAllText(Path.Combine(output,"result.txt"),$"PASS {checks} checks; async loading, safe departure, picking, bidirectional wrap, two polar walls, distant eviction, exploration save, missing-resource blocking/retry. Resident {sea.ResidentChunks}; evictions {sea.Evictions}; estimated bytes {sea.EstimatedBytes}.\n");Application.Quit();
 }
 void Update(){if(automated||sea==null)return;sea.Orbit(Input.GetMouseButton(1)?Input.GetAxis("Mouse X"):0,Input.mouseScrollDelta.y);sea.Update(state);sea.Render(state,1);}
 void OnLog(string message,string trace,LogType type){if(type!=LogType.Exception)return;File.WriteAllText(Path.Combine(output,"result.txt"),"FAIL: "+message+"\n"+trace);Application.Quit(1);}
 void OnGUI(){if(sea==null)return;UnityEngine.GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),sea.Image,ScaleMode.ScaleToFit);if(!automated)UnityEngine.GUI.Label(new Rect(12,12,900,30),sea.Diagnostic+" | Streamed map review | RMB orbit, wheel zoom");}
 void OnDestroy(){Application.logMessageReceived-=OnLog;sea?.Dispose();}
}
}
