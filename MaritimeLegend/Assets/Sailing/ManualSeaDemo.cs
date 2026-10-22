using System;
using System.IO;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class ManualSeaDemo : MonoBehaviour {
 MaritimeGame game;float elapsed;int stage;bool verify;Vector2 before;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-manualSeaDemo")||args.Contains("-manualSeaSmoke"))new GameObject("Manual sailing preview").AddComponent<ManualSeaDemo>();}
 void Start(){DontDestroyOnLoad(gameObject);verify=Environment.GetCommandLineArgs().Contains("-manualSeaSmoke");game=FindAnyObjectByType<MaritimeGame>();
  string root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","ManualSeaUserData"));Directory.CreateDirectory(root);SaveStore.TestDirectory=root;UserDataPaths.TestRoot=root;
  game.s=GameService.New(game.c,0,0,"休闲",StateFactory.Appearance(game.c),"海上试航",42);game.g=new GameService(game.c,game.s);game.s.pendingEvents.Clear();game.g.Depart(1);game.Change("航海");game.EnsureManualSea();
  if(verify){LiveGui.Camera.targetTexture=new RenderTexture(1920,1080,24);LiveGui.Camera.targetTexture.Create();}
 }
 void Update(){if(!verify)return;elapsed+=Time.unscaledDeltaTime;if(elapsed<3)return;
  try{
   if(stage==0){
    foreach(var p in new[]{new Vector2(300,300),new Vector2(1500,800),new Vector2(960,540)}){var v=SeaProjection.Project(SeaProjection.Unproject(p,Vector2.one*.5f),Vector2.one*.5f);if(Vector2.Distance(p,v)>.1f)throw new Exception("Projection round trip");}
    if(game.SetManualSeaTarget(new Vector2(900,100)))throw new Exception("Sky accepted as target");
    before=game.SeaPosition;if(!game.SetManualSeaTarget(new Vector2(1600,700)))throw new Exception("Click target rejected");stage++;elapsed=0;
   }else if(stage==1){if(Vector2.Distance(before,game.SeaPosition)<.0001f)throw new Exception("Fleet did not move");
    var screen=SeaProjection.Project(game.SeaPosition,game.SeaCamera);if(screen.x<909||screen.x>1011||screen.y<549||screen.y>571)throw new Exception("Camera dead zone failed");
    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Application.dataPath,"..","Evidence")));LiveGui.Capture(Path.GetFullPath(Path.Combine(Application.dataPath,"..","Evidence","manual-sea.png")));game.SetManualSeaTarget(screen);before=game.SeaPosition;stage++;elapsed=0;
   }else{if(Vector2.Distance(before,game.SeaPosition)>.0005f)throw new Exception("Arrival did not stop");Debug.Log("MANUAL_SEA_PASS: arrival stop, camera dead zone, screen/map round trips, sky rejection, click target, real domain movement, rendered capture");Application.Quit(0);}
  }catch(Exception e){Debug.LogException(e);Application.Quit(1);}
 }
}
}
