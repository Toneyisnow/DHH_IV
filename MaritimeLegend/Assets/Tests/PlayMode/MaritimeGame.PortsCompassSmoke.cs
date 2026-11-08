#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {public partial class MaritimeGame {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void StartPortsCompassChecks(){if(Environment.GetCommandLineArgs().Contains("-portsCompassSmoke")){var game=FindAnyObjectByType<MaritimeGame>();if(game)game.StartCoroutine(game.CheckPortsCompass());}}
 IEnumerator CheckPortsCompass(){
  yield return null;
  string output=Path.Combine(Application.dataPath,"..","Evidence","PortsCompass");Directory.CreateDirectory(output);
  LiveGui.Camera.targetTexture=new RenderTexture(1920,1080,24);LiveGui.Camera.targetTexture.Create();
  s=GameService.New(c,0,0,"休闲",StateFactory.Appearance(c),"Ports compass review",42);s.pendingEvents.Clear();s.city=6;g=new GameService(c,s);g.Depart(7);page="航海";paused=true;
  yield return new WaitForSecondsRealtime(2);
  try{
   // Simulate the reported legacy overseas city / sea-centre save.
   var legacy=GameService.New(c,0,0,"休闲",StateFactory.Appearance(c),"Legacy regional migration",37);
   legacy.cities.RemoveRange(8,legacy.cities.Count-8);legacy.atSea=true;legacy.seaFrom=0;legacy.seaX=.4;legacy.seaY=.37;
   legacy.sailing.manualMapId="southeast_asia_v2";legacy.sailing.manualSpawnPort="南海区域入口";
   int gold=legacy.gold;var service=new GameService(c,legacy);service.ApplySoutheastAsiaStart();
   V2Assert(legacy.cities.Count==c.ports.Length&&legacy.gold==gold&&legacy.home==0,"legacy city expansion preserves progress");
   V2Assert(ManualSeaPortCatalog.Departure(legacy.city,c.ports[legacy.city].name)!=null&&legacy.sailing.manualMapId=="","legacy overseas save assigned coastal city");
   int assigned=legacy.city;legacy=JsonUtility.FromJson<GameState>(JsonUtility.ToJson(legacy));new GameService(c,legacy).ApplySoutheastAsiaStart();
   V2Assert(legacy.city==assigned,"random starting city persists across reload");
   var choices=new System.Collections.Generic.HashSet<int>();for(uint seed=1;seed<=300;seed++)choices.Add(GameService.RandomSoutheastAsiaPort(c,seed*7919));
   V2Assert(choices.Count==12,"random start can select every Southeast Asia city");
   foreach(var coastal in ManualSeaPortCatalog.All){s.atSea=false;s.city=coastal.gamePortId;g.Depart(coastal.gamePortId==6?8:6);EnsureManualSea();V2Assert(Vector2.Distance(SeaPosition,coastal.Spawn)<.00001f&&s.sailing.manualSpawnPort==coastal.name&&Mathf.Abs(ManualSeaZoom-10)<.001f,"actual departure and zoom "+coastal.id);}
   s.atSea=false;s.city=6;g.Depart(8);
   EnsureManualSea();
   var departure=ManualSeaPortCatalog.Departure(6,"马六甲");
   V2Assert(Vector2.Distance(SeaPosition,departure.Spawn)<.00001f&&s.sailing.manualSpawnPort=="马六甲","departure beside actual city");
   V2Assert(Mathf.Abs(ManualSeaZoom-10)<.001f,"initial zoom is minimum 8 plus 2");
   foreach(var f in s.sailing.manualFollowers)V2Assert(seaCoast.Distance(seaCoast.Pixels(new Vector2((float)f.x,(float)f.y)))>=8.99f,"departure follower on navigable water");
   var fleet=s.sailing.manualFollowers;var f0=seaCoast.Pixels(new Vector2((float)fleet[0].x,(float)fleet[0].y));var f1=seaCoast.Pixels(new Vector2((float)fleet[1].x,(float)fleet[1].y));
   V2Assert(Vector2.Distance(f0,f1)>=21.99f&&Vector2.Distance(f0,seaCoast.Pixels(SeaPosition))>=21.99f&&Vector2.Distance(f1,seaCoast.Pixels(SeaPosition))>=21.99f,"initial fleet separated from every other ship");
   var position=SeaPosition;s.sailing.manualZoom=46;seaOwner=null;EnsureManualSea();V2Assert(SeaPosition==position&&Mathf.Abs(ManualSeaZoom-46)<.001f,"resume does not reset position or camera");
   s.atSea=false;s.city=6;g.Depart(7);EnsureManualSea();V2Assert(Mathf.Abs(ManualSeaZoom-10)<.001f&&Vector2.Distance(SeaPosition,departure.Spawn)<.00001f,"repeat departure resets city spawn and close camera");
   double[] headings={-Math.PI/2,0,Math.PI/2,Math.PI};float[] expected={0,90,180,270};
   for(int i=0;i<4;i++){s.heading=headings[i];V2Assert(Mathf.Abs(Mathf.DeltaAngle(CompassBearing,expected[i]))<.01f,"compass cardinal "+expected[i]);}
   s.heading=.6;float bearing=CompassBearing;indianOcean.SetView(123,100);V2Assert(Mathf.Abs(CompassBearing-bearing)<.01f,"compass independent of camera orbit");
   V2Assert(!SetManualSeaTarget(CompassRect.center),"compass blocks navigation clicks");
   V2Assert(Resources.Load<Texture2D>("Sprites/UI/ManualSailing/Compass/sheet")!=null,"compass atlas loaded");
  }catch(Exception e){Debug.LogException(e);Application.Quit(1);yield break;}
  indianOcean.SetView(s.sailing.manualYaw,10);paused=false;yield return new WaitForSecondsRealtime(.5f);LiveGui.Capture(Path.Combine(output,"departure.png"));
  for(int i=0;i<ManualSeaPortCatalog.All.Length;i++){
   var pos=ManualSeaV2Ports.Positions[i];
   var dataPort=ManualSeaPortCatalog.All[i];
   SetIndianReview(dataPort.Spawn,dataPort.DepartureYaw,40);s.heading=-Math.PI/2;paused=false;
   yield return new WaitForSecondsRealtime(.5f);
   try{
    var port=GameObject.Find("Coastal port - "+ManualSeaV2Ports.Names[i]);V2Assert(port!=null,"port exists "+i);
    var camera=GameObject.Find("Permanent hex exploration overlay").transform.parent.GetComponent<Camera>();
    var mesh=port.GetComponent<MeshFilter>().sharedMesh;V2Assert(mesh.vertexCount>100&&mesh.bounds.size.z>2,"solid 3D geometry "+i);
    var screen=camera.WorldToViewportPoint(port.transform.position);var pointer=new Vector2(screen.x*1920,(1-screen.y)*1080);
    // A 3D town has genuine gaps between buildings; probe rendered surfaces, not the old billboard centre.
    bool hit=false;var vertices=mesh.vertices;var faces=mesh.triangles;
    for(int f=0;f<faces.Length&&!hit;f+=12){var local=(vertices[faces[f]]+vertices[faces[f+1]]+vertices[faces[f+2]])/3f;var point=camera.WorldToViewportPoint(port.transform.TransformPoint(local));if(point.z<=0)continue;var candidate=new Vector2(point.x*1920,(1-point.y)*1080);if(indianOcean.HoverPort(candidate)==ManualSeaV2Ports.Names[i]){pointer=candidate;hit=true;}}
    V2Assert(hit,"hover visible 3D surface "+i);
    SeaExploration.Hex(pos.x,pos.z,out int q,out int r);var data=s.sailing.southeastAsiaExploration;int k=(r-SeaExploration.MinR)*SeaExploration.Width+q-SeaExploration.MinQ,old=data.tiles[k];data.tiles[k]=0;
    V2Assert(indianOcean.HoverPort(pointer)==null,"unknown port name hidden "+i);data.tiles[k]=old;
    LiveGui.Capture(Path.Combine(output,"port-"+i+".png"));
   }catch(Exception e){Debug.LogException(e);Application.Quit(1);yield break;}
  }
  var review=ManualSeaPortCatalog.All[0];
  for(int angle=0;angle<4;angle++){
   SetIndianReview(review.Spawn,review.DepartureYaw+angle*90,40);yield return new WaitForSecondsRealtime(.4f);
   var model=GameObject.Find("Coastal port - "+review.name);V2Assert(Quaternion.Angle(model.transform.rotation,Quaternion.identity)<.001f,"city stays fixed during camera orbit");
   LiveGui.Capture(Path.Combine(output,"orbit-"+angle+".png"));
  }
  foreach(var coastal in ManualSeaPortCatalog.All){
   s.atSea=false;s.city=coastal.gamePortId;s.pendingEvents.Clear();Change("城市");yield return new WaitForSecondsRealtime(.25f);
   var scene=FindAnyObjectByType<CityScene>();V2Assert(scene!=null&&scene.Current.cityId==coastal.gamePortId&&scene.Interactions.Length==6,"regional city opens with correct identity "+coastal.id);
  }
  Debug.Log("PORTS_COMPASS_PASS");Application.Quit(0);
 }
}}
#endif
