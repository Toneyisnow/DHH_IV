using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// Standalone scene for trying the world without touching player saves.
public sealed class WorldSeaDemo : MonoBehaviour {
 WorldSeaRuntime sea;GameState state;GameService game;bool stopped=true,hasTarget;Vector2 target;float opening=1;
 void Start(){var catalog=CatalogLoader.Load();state=GameService.New(catalog,0,0,"休闲",StateFactory.Appearance(catalog),"世界试航",42);state.pendingEvents.Clear();game=new GameService(catalog,state);game.Depart(1);sea=new WorldSeaRuntime(transform);game.EnterWorldSailing(sea.Map);game.SeaNavigation=sea.Navigation;sea.SetView(0,85);if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-worldSmoke")>=0)StartCoroutine(Smoke());}
 void Update(){if(sea==null)return;double dt=System.Math.Min(.1,Time.deltaTime);sea.Update(state);game.WorldStreamingBlocked=!sea.NavigationReady;
  if(Input.GetKeyDown(KeyCode.Space))stopped=!stopped;
  if(Input.GetKey(KeyCode.W))opening=Mathf.Min(1,opening+(float)dt);if(Input.GetKey(KeyCode.S))opening=Mathf.Max(0,opening-(float)dt);
  game.AdjustSail((Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0),dt,Input.GetKeyDown(KeyCode.R));
  sea.Orbit(Input.GetMouseButton(1)?Input.GetAxis("Mouse X"):0,Input.mouseScrollDelta.y);
  var mouse=Input.mousePosition;var pointer=new Vector2(mouse.x/Screen.width*1920,(1-mouse.y/Screen.height)*1080);
  if(Input.GetMouseButtonDown(0)&&pointer.y>145&&pointer.y<940&&sea.TryUnproject(pointer,out target)){hasTarget=true;stopped=false;}
  if(!game.WorldStreamingBlocked)for(double left=dt;left>1e-9;){double step=System.Math.Min(left,1.0/60);left-=step;
   if(hasTarget){if(WorldMap.Distance(state.seaX,state.seaY,target.x,target.y)<1){stopped=true;hasTarget=false;}else game.SteerToward(target.x,target.y,step);}
   double speed=game.Sail(step,stopped?0:opening);sea.StepFollowers(state,step,speed);
  }
  sea.Render(state,opening);
 }
 void OnGUI(){if(sea==null)return;var saved=GUI.matrix;GUI.matrix=Matrix4x4.Scale(new Vector3(Screen.width/1920f,Screen.height/1080f,1));GUI.DrawTexture(new Rect(0,0,1920,1080),sea.Image);
  GUI.Box(new Rect(20,20,1050,115),"World manual sailing | independent test voyage");
  GUI.Label(new Rect(40,48,1000,28),$"Longitude {state.seaX/32-180:F3} | Latitude {state.seaY/32-80:F3} | Chunks {sea.ResidentChunks}/2304 | Pending {sea.PendingChunks} | {(sea.NavigationReady?stopped?"Stopped":"Sailing":"Loading")}");
  GUI.Label(new Rect(40,80,1000,28),"LMB sail | RMB orbit | Wheel zoom | Space stop | W/S sails | A/D trim | R reset trim");
  if(GUI.Button(new Rect(1100,25,180,45),"Lisbon"))Jump(sea.Map.Port(0).x,sea.Map.Port(0).y);
  if(GUI.Button(new Rect(1300,25,180,45),"Malacca"))Jump(sea.Map.Port(6).x,sea.Map.Port(6).y);
  if(GUI.Button(new Rect(1500,25,180,45),"Date line"))Jump(WorldMap.Width-12,60*32);
  if(GUI.Button(new Rect(1700,25,180,45),"80 N"))Jump(180*32,WorldMap.Height);
  for(int i=0;i<3;i++)if(GUI.Button(new Rect(1100+i*200,85,180,40),new[]{"Sunny","Sunset","Night"}[i]))sea.SetSkyMode((ManualSeaV2Environment.SkyMode)i);
  GUI.matrix=saved;
 }
 void Jump(double x,double y){state.seaX=x;state.seaY=y;state.sailing.manualFollowers=null;stopped=true;hasTarget=false;sea.Update(state);}
 static void Require(bool ok,string message){if(ok)return;Debug.LogError("WORLD_PLAYER_FAIL: "+message);Application.Quit(1);throw new System.Exception(message);}
 System.Collections.IEnumerator Smoke(){
  float deadline=Time.realtimeSinceStartup+90;
  while(!sea.NavigationReady||sea.PendingChunks>0){Require(Time.realtimeSinceStartup<deadline,"initial loading timeout");yield return null;}
  var args=System.Environment.GetCommandLineArgs();int at=System.Array.IndexOf(args,"-worldReview");string folder=at>=0?args[at+1]:System.IO.Path.Combine(Application.dataPath,"..","Review");System.IO.Directory.CreateDirectory(folder);
  System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"player-result.txt"),"RUNNING\n");
  sea.Capture(System.IO.Path.Combine(folder,"player-departure.png"));
  Jump(WorldMap.Width-2,WorldMap.Height/2);state.heading=state.wind=0;state.trim=.5;
  while(!sea.NavigationReady){Require(Time.realtimeSinceStartup<deadline,"date line loading timeout");yield return null;}
  stopped=false;yield return new WaitForSecondsRealtime(2);stopped=true;
  Require(state.seaX<100,"live update crosses date line");Require(state.sailing.manualFollowers!=null&&state.sailing.manualFollowers.Length==2,"live fleet remains present");
  foreach(var f in state.sailing.manualFollowers)Require(WorldMap.Distance(state.seaX,state.seaY,f.x,f.y)<80,"follower stays across wrapped seam");
  double beforeX=state.seaX,beforeY=state.seaY;yield return new WaitForSecondsRealtime(.5f);Require(WorldMap.Distance(beforeX,beforeY,state.seaX,state.seaY)<1e-8,"stop freezes fleet leader");
  sea.SetView(270,180);sea.Render(state,opening);var point=new Vector2((float)state.seaX,(float)state.seaY);var screen=sea.Project(point);bool hit=sea.TryUnproject(screen,out var picked);double error=WorldMap.Distance(point.x,point.y,picked.x,picked.y);Require(hit&&error<.02,"rotated maximum zoom picking: hit="+hit+" error="+error+" point="+point+" picked="+picked+" screen="+screen);
  sea.Capture(System.IO.Path.Combine(folder,"player-date-line.png"));
  Jump(180*32,WorldMap.Height);state.heading=state.wind=-System.Math.PI/2;
  while(!sea.NavigationReady){Require(Time.realtimeSinceStartup<deadline,"polar loading timeout");yield return null;}
  stopped=false;yield return new WaitForSecondsRealtime(1);stopped=true;Require(System.Math.Abs(state.seaY-WorldMap.Height)<1e-8,"live polar wall");
  sea.Capture(System.IO.Path.Combine(folder,"player-north-wall.png"));
  System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"player-result.txt"),"PASS: real player Update, asynchronous loading, cyclic crossing, two followers, stop, rotated picking, north wall.\n");Debug.Log("WORLD_PLAYER_SMOKE_PASS");Application.Quit(0);
 }
 void OnDestroy(){sea?.Dispose();}
}
}
