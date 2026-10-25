using System;
using System.IO;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class ManualSeaDemo : MonoBehaviour {
 MaritimeGame game;int warmFrames;float elapsed;int stage;bool verify;Vector2 before;double heading;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){var args=Environment.GetCommandLineArgs();if(args.Contains("-manualSeaDemo")||args.Contains("-manualSeaSmoke"))new GameObject("Manual sailing preview").AddComponent<ManualSeaDemo>();}
 void Start(){DontDestroyOnLoad(gameObject);verify=Environment.GetCommandLineArgs().Contains("-manualSeaSmoke");game=FindAnyObjectByType<MaritimeGame>();
  string root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","ManualSeaUserData"));Directory.CreateDirectory(root);SaveStore.TestDirectory=root;UserDataPaths.TestRoot=root;
  game.s=GameService.New(game.c,0,0,"休闲",StateFactory.Appearance(game.c),"印度洋试航",42);game.g=new GameService(game.c,game.s);game.s.pendingEvents.Clear();game.g.Depart(1);game.Change("航海");game.EnsureManualSea();
  if(verify){LiveGui.Camera.targetTexture=new RenderTexture(1920,1080,24);LiveGui.Camera.targetTexture.Create();}
 }
 string Evidence(string name){var dir=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Evidence"));Directory.CreateDirectory(dir);return Path.Combine(dir,name+".png");}
 void Capture(string name){LiveGui.Capture(Evidence(name));game.CaptureIndianMap(Evidence(name+"-map"));}
 void Check(bool ok,string why){if(!ok)throw new Exception(why);}
 void Update(){if(!verify)return;if(warmFrames++<12)return;elapsed+=Time.unscaledDeltaTime;if(elapsed<(stage==0?3:1.3f))return;
  try{
   if(stage==0){
    Check(game.s.sailing.manualMapId=="indian_ocean","Regional initialization");
    var nav=new CoastNavigation(Resources.Load<TextAsset>("Environment/IndianOcean/coast-distance").bytes);
    var layout=JsonUtility.FromJson<IndianOceanRenderer.Layout>(Resources.Load<TextAsset>("Environment/IndianOcean/layout").text);
    Check(layout.ports.Length==4,"Indian ports");foreach(var port in layout.ports){var p=nav.Pixels(new Vector2(port.x,port.y));Check(nav.Distance(p)>25,"Port spawn safe");for(int i=0;i<16;i++){float a=i*Mathf.PI/8;var end=nav.MovePixels(p,new Vector2(Mathf.Cos(a),Mathf.Sin(a))*200);Check(nav.Distance(end)>=-.001f,"Swept coast clearance");}}
    var settings=SailingScene.Active;Check(settings!=null,"Scene camera settings exist");
    float oldMin=settings.minimumZoom,oldMax=settings.maximumZoom,oldPitch=settings.pitch;
    settings.minimumZoom=90;settings.maximumZoom=130;settings.pitch=55;game.EnsureManualSea();
    Check(Mathf.Abs(game.ManualSeaPitch-55)<.01f&&game.ManualSeaZoom>=90,"Live scene pitch and nearest limit");
    game.SetIndianReview(game.SeaPosition,0,900);Check(Mathf.Abs(game.ManualSeaZoom-130)<.01f,"Live farthest limit");
    game.SetIndianReview(game.SeaPosition,0,1);Check(Mathf.Abs(game.ManualSeaZoom-90)<.01f,"Live nearest limit");
    foreach(float pitch in new[]{20f,40f,70f}){settings.pitch=pitch;game.EnsureManualSea();var uv=game.SeaPosition+new Vector2(.01f,.01f);Check(Vector2.Distance(uv,game.UnprojectManualSea(game.ProjectManualSea(uv)))<.00002f,"Picking after live pitch change");}
    settings.minimumZoom=oldMin;settings.maximumZoom=oldMax;settings.pitch=oldPitch;game.EnsureManualSea();game.SetIndianReview(game.SeaPosition,0,70);
    foreach(string family in new[]{"Angular","Rolling","Snow"}){float previous=0;foreach(string size in new[]{"Small","Medium","Large"}){string asset=family+"_"+size;var prefab=Resources.Load<GameObject>("Prefabs/Environment/IndianOcean/"+asset);Check(prefab!=null&&layout.placements.Any(p=>p.asset==asset),"Landform placed: "+asset);var mesh=prefab.GetComponentInChildren<MeshFilter>().sharedMesh;Check(mesh.bounds.size.y>previous,"Three size grades: "+asset);previous=mesh.bounds.size.y;}}
    foreach(string asset in new[]{"Dune_Small","Cliff_Large"})Check(Resources.Load<GameObject>("Prefabs/Environment/IndianOcean/"+asset)!=null&&layout.placements.Any(p=>p.asset==asset),"Special landform: "+asset);
    Capture("indian-spawn");Check(!game.SetManualSeaTarget(new Vector2(900,100)),"Sky accepted");
    foreach(float yaw in new[]{0f,65f,180f,285f}){game.SetIndianReview(new Vector2(.48f,.72f),yaw,85);var uv=new Vector2(.49f,.73f);Check(Vector2.Distance(uv,game.UnprojectManualSea(game.ProjectManualSea(uv)))<.00001f,"Orbit picking roundtrip");}
    game.SetIndianReview(new Vector2(.48f,.72f),0,70);game.s.heading=0;game.s.wind=0;game.s.pendingEvents.Clear();before=game.SeaPosition;game.SetManualSeaTarget(game.ProjectManualSea(before+new Vector2(.001f,0)));
   }else if(stage==1){Check(game.SeaPosition.x>before.x+.003f&&game.s.sailing.speed>0&&!game.HasManualSeaTarget,"Cruise beyond waypoint");before=game.SeaPosition;heading=game.s.heading;
   }else if(stage==2){Check(game.SeaPosition.x>before.x+.002f&&Math.Abs(game.s.heading-heading)<.001,"Heading preserved");Capture("indian-sailing");Check(LiveGui.ClickText("停船",true),"Stop button");
   }else if(stage==3){Check(game.s.sailing.speed<.00001,"Explicit stop");game.SetIndianReview(new Vector2(.47f,.43f),0,70);
   }else if(stage==4){Capture("indian-west-coast");game.SetIndianReview(new Vector2(.61f,.17f),25,120);
   }else if(stage==5){Capture("indian-snow-mountains");game.SetIndianReview(new Vector2(.22f,.32f),-30,100);
   }else if(stage==6){Capture("indian-desert");game.SetIndianReview(new Vector2(.5f,.5f),0,510);
   }else if(stage==7){Capture("indian-full-map");game.SetIndianReview(new Vector2(875f/1672,467f/941),-25,48);
   }else if(stage==8){Capture("indian-cliff");game.SetIndianReview(new Vector2(1010f/1672,362f/941),0,48);
   }else if(stage==9){Capture("indian-rolling");
    game.s.pendingEvents.Clear();game.s.heading=0;game.s.wind=0;game.s.windTimer=999;
    game.ApplyManualSailInput(0,0,true);game.ApplyManualSailInput(1,.2,false);
    Check(Mathf.Abs((float)SailingFormulas.SailDegrees(game.s.trim)-9)<.01f&&Mathf.Abs(game.InstrumentWheelAngle-120)<.01f,"D rotates sail and fast clockwise wheel");
    game.ApplyManualSailInput(0,0,true);game.ApplyManualSailInput(-1,.2,false);
    Check(Mathf.Abs((float)SailingFormulas.SailDegrees(game.s.trim)+9)<.01f&&Mathf.Abs(Mathf.DeltaAngle(0,game.InstrumentWheelAngle)+120)<.01f,"A rotates sail and fast counterclockwise wheel");
    Check(Math.Abs(game.s.heading)<.001,"Trim keys preserve click-controlled heading");
    Check(!game.SetManualSeaTarget(new Vector2(1792,300))&&!game.SetManualSeaTarget(new Vector2(1792,510)),"Instrument panels block map clicks");
    foreach(float yaw in new[]{0f,65f,180f}){game.SetIndianReview(new Vector2(.47f,.43f),yaw,70);var d=(game.ProjectManualSea(game.SeaPosition+new Vector2(.01f,0))-game.ProjectManualSea(game.SeaPosition)).normalized;float angle=(game.InstrumentShipAngle-90)*Mathf.Deg2Rad;Check(Vector2.Dot(d,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)))>.999f,"HUD heading follows screen camera");}
    foreach(float trim in new[]{.5f,.8f,.9f,1f}){game.s.trim=trim;int expected=trim==.5f?3:trim==.8f?2:trim==.9f?1:0;Check(game.InstrumentSailState==expected,"Four wind/sail states");}
    game.SetIndianReview(new Vector2(.47f,.43f),35,70);game.ApplyManualSailInput(0,0,true);
   }else if(stage==10){Capture("instruments-full");game.s.trim=.8;game.ApplyManualSailInput(1,.03,false);
   }else if(stage==11){Capture("instruments-medium");game.s.trim=.9;game.ApplyManualSailInput(1,.03,false);
   }else if(stage==12){Capture("instruments-small");game.s.trim=1;game.ApplyManualSailInput(1,.03,false);
   }else if(stage==13){Capture("instruments-none");Debug.Log("MANUAL_SEA_PASS: instrument screen heading, sail/helm input directions and speed, four wind states, panel click blocking, live scene zoom bounds and pitch, picking at three pitches, 11 landform types and size grades, Indian Ocean, four safe Indian spawns, orbit picking at four headings, waypoint fly-through, preserved course, explicit stop, swept coast clearance");Application.Quit(0);}
   stage++;elapsed=0;
  }catch(Exception e){Debug.LogException(e);Application.Quit(1);}
 }
}
}
