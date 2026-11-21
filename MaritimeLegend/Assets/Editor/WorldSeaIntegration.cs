using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
public static class WorldSeaIntegration {
 const string Scene="Assets/Scenes/WorldSailingTest.unity";
 static WorldSeaRuntime runtime;static GameObject owner;static GameState state;static int stage;static double started,readyAt;static string output;
 [MenuItem("Maritime Legend/World Sailing/Create test scene")]
 public static void CreateScene(){
  Directory.CreateDirectory("Assets/Scenes");
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  new GameObject("World sailing test voyage").AddComponent<WorldSeaDemo>();
  var camera=new GameObject("Display camera").AddComponent<Camera>();camera.cullingMask=0;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
  EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(),Scene);AssetDatabase.SaveAssets();
 }
 [MenuItem("Maritime Legend/World Sailing/Play test scene")]
 public static void Play(){if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;if(!File.Exists(Scene))CreateScene();else EditorSceneManager.OpenScene(Scene);EditorApplication.isPlaying=true;}
 public static void BuildPreview(){
  CreateScene();PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
  Directory.CreateDirectory("Build");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Scene},locationPathName="Build/WorldSailing.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("World preview build failed");Debug.Log("WORLD_PREVIEW_BUILD_PASS");
 }
 // Invoke without -quit: update callback waits for actual asynchronous resource completion.
 public static void Verify(){
  var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-worldReview");output=at>=0?Path.GetFullPath(args[at+1]):Path.GetFullPath("../ArtSource/WorldMap/Review");Directory.CreateDirectory(output);
  try{
   WorldSailingChecks.Run();
   foreach(string name in new[]{"Maritime/WorldSea/Land","Maritime/WorldSea/Water","Maritime/ManualSeaV2/Sky"}){var shader=Shader.Find(name);if(!shader||ShaderUtil.ShaderHasError(shader))throw new Exception("World shader error "+name);}
   CreateScene();owner=new GameObject("World render verification");runtime=new WorldSeaRuntime(owner.transform);
   var catalog=CatalogLoader.Load();state=GameService.New(catalog,0,0,"休闲",StateFactory.Appearance(catalog),"World review",42);state.pendingEvents.Clear();var game=new GameService(catalog,state);game.Depart(1);game.EnterWorldSailing(runtime.Map);
   stage=0;started=EditorApplication.timeSinceStartup;readyAt=0;EditorApplication.update+=Tick;
  }catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
 }
 static void Tick(){try{
  runtime.Update(state);runtime.Render(state,1);
  double now=EditorApplication.timeSinceStartup;
  if(now-started>180)throw new Exception("World streaming timeout at stage "+stage+" "+runtime.Diagnostic);
  if(!runtime.NavigationReady||runtime.PendingChunks>0){readyAt=0;return;}
  if(readyAt==0){readyAt=now;return;}if(now-readyAt<6)return;
  if(runtime.ResidentChunks>WorldChunkRepository.Capacity)throw new Exception("Unbounded world residency");
  var pos=new Vector2((float)state.seaX,(float)state.seaY);var screen=runtime.Project(pos);
  if(!runtime.TryUnproject(screen,out var back)||WorldMap.Distance(pos.x,pos.y,back.x,back.y)>.02)throw new Exception("World picking roundtrip");
  runtime.Capture(Path.Combine(output,"runtime-"+stage+".png"));
  Debug.Log($"WORLD_RENDER_STAGE {stage} resident={runtime.ResidentChunks} visible={runtime.DisplayCount} estimatedBytes={runtime.EstimatedBytes} position={state.seaX},{state.seaY}");
  if(stage==0){var p=runtime.Map.Port(6);state.seaX=p.x;state.seaY=p.y;runtime.SetView(25,120);}
  else if(stage==1){state.seaX=WorldMap.Width-2;state.seaY=60*32;runtime.SetView(90,100);}
  else if(stage==2){runtime.Navigation.ResolveMove(state.seaX,state.seaY,state.seaX+4,state.seaY,out double x,out double y);if(x>3||runtime.Navigation.Loading)throw new Exception("Streamed date line blocked");state.seaX=x;state.seaY=y;}
  else if(stage==3){state.seaX=180*32;state.seaY=WorldMap.Height;runtime.SetView(0,120);}
  else {Finish();return;}
  state.sailing.manualFollowers=null;stage++;started=now;readyAt=0;
 }catch(Exception e){EditorApplication.update-=Tick;Debug.LogException(e);runtime?.Dispose();EditorApplication.Exit(1);}}
 static void Finish(){
  EditorApplication.update-=Tick;runtime.Dispose();UnityEngine.Object.DestroyImmediate(owner);
  File.WriteAllText(Path.Combine(output,"runtime-result.txt"),"PASS: asynchronous world resources; bounded residency; 5 rendered views; port departure, cyclic seam crossing, polar boundary, picking.\nNot a hardware frame-time benchmark.\n");
  Debug.Log("WORLD_RENDER_VERIFY_PASS");EditorApplication.Exit(0);
 }
}
}
