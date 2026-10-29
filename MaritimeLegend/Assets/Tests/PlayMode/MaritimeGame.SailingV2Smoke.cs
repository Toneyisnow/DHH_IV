#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {public partial class MaritimeGame {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void StartV2Checks(){if(Environment.GetCommandLineArgs().Contains("-sailingV2Smoke")){
  var game=FindAnyObjectByType<MaritimeGame>();if(game)game.StartCoroutine(game.CheckV2());
 }}
 IEnumerator CheckV2(){
  yield return null;
  string output=Path.Combine(Application.dataPath,"..","Evidence","SailingV2");Directory.CreateDirectory(output);
  try{
   Screen.SetResolution(1920,1080,FullScreenMode.Windowed);
   LiveGui.Camera.targetTexture=new RenderTexture(1920,1080,24);LiveGui.Camera.targetTexture.Create();
   var fog=new SeaExploration();fog.Reveal(0,0,72);
   foreach(var center in new[]{Vector2.zero,new Vector2(7.2f,8.3f)}){
    var circle=new SeaExploration();circle.Reveal(center.x,center.y,72);
    for(int r=SeaExploration.MinR;r<SeaExploration.MinR+SeaExploration.Height;r++)for(int q=SeaExploration.MinQ;q<SeaExploration.MinQ+SeaExploration.Width;q++){
     double dx=SeaExploration.TileSpacing*(q+r*.5)-center.x,dz=SeaExploration.Radius*1.5*r-center.y,d=Math.Sqrt(dx*dx+dz*dz);
     int expected=d<=72?2:d<=72+SeaExploration.TileSpacing?1:0;
     if(circle.At(q,r)!=expected)throw new Exception("Reveal is not a ship-centered world circle");
    }
    V2Assert(circle.tiles.Any(v=>v==1),"one-tile circular half-covered band");
   }
   V2Assert(true,"circular world-distance visibility, including ship away from hex center");
   var old=(int[])fog.tiles.Clone();fog.Reveal(180,-80);fog.Reveal(0,0,12);
   V2Assert(fog.tiles.Zip(old,(a,b)=>a>=b).All(x=>x),"exploration is monotonic");
   V2Assert(JsonUtility.FromJson<SeaExploration>(JsonUtility.ToJson(fog)).tiles.SequenceEqual(fog.tiles),"fog JSON round trip");
   s=GameService.New(c,0,0,"休闲",StateFactory.Appearance(c),"V2 integration test",42);s.pendingEvents.Clear();g=new GameService(c,s);g.Depart(1);s.sailing.cloudSeed=4227;page="航海";paused=true;
  }catch(Exception e){Debug.LogException(e);Application.Quit(1);yield break;}
  yield return new WaitForSecondsRealtime(3);
  try{
   EnsureManualSea();V2Assert(s.sailing.manualMapId=="southeast_asia_v2","manual sailing enters V2");
   V2Assert(seaCoast.Distance(seaCoast.Pixels(SeaPosition))>=18,"safe anchorage");
   V2Assert(Mathf.Abs(ManualSeaPitch-35)<.01,"35 degree camera");
   Vector2 screen=ProjectManualSea(SeaPosition);V2Assert(Vector2.Distance(UnprojectManualSea(screen),SeaPosition)<.0001,"perspective projection round trip");
   throttle=0;V2Assert(InstrumentSailState==0,"furled sail panel shows no wind fill");throttle=1;
   double trim=s.trim;ApplyManualSailInput(1,.2,false);V2Assert(s.trim!=trim,"A/D sail trim");
   V2Assert(!SetManualSeaTarget(new Vector2(1780,300)),"right panel blocks navigation clicks");
   Vector2 target=ProjectManualSea(SeaPosition+new Vector2(.015f,0));V2Assert(SetManualSeaTarget(target),"left-click target");
   var before=SeaPosition;throttle=1;TickManualSea(.2);V2Assert(Vector2.Distance(before,SeaPosition)>0,"sail simulation moves ship");
   seaStopped=true;before=SeaPosition;TickManualSea(.2);V2Assert(Vector2.Distance(before,SeaPosition)<.00001,"stop control");
   var cells=(int[])s.sailing.southeastAsiaExploration.tiles.Clone();SaveStore.TestDirectory=Path.Combine(output,"TestSaves");SaveStore.Save(s);var loaded=SaveStore.Load(s.slot);s=loaded;g=new GameService(c,s);EnsureManualSea();
   V2Assert(s.sailing.southeastAsiaExploration.tiles.SequenceEqual(cells),"game save reload retains fog");
   seaOwner=null;s.sailing.manualMapId="";EnsureManualSea();V2Assert(s.sailing.southeastAsiaExploration.tiles.SequenceEqual(cells),"return and depart retain exploration");
   foreach(float zoom in new[]{24f,60f,120f}){
    indianOcean.SetView(0,zoom);V2Assert(Mathf.Abs(ManualSeaZoom-zoom)<.01,"actual zoom "+zoom);
    indianOcean.Render(s,throttle);
    var horizonFog=V2Pixel(indianOcean.Image,960,907);var sky=V2Pixel(indianOcean.Image,960,1015);
    V2Assert(horizonFog.b>.8f&&horizonFog.g>.6f,"unknown fog matches sunny horizon at zoom "+zoom);
    V2Assert(sky.b>.15f,"sky not covered at zoom "+zoom);
    if(zoom==24){
     // A known-unknown elevated target avoids mistaking already explored nearby peaks for a leak.
     var peak=GameObject.CreatePrimitive(PrimitiveType.Cube);peak.name="Fog depth test peak";peak.layer=28;
     peak.transform.position=ManualSeaV2Runtime.World(SeaPosition)+new Vector3(0,25,240);peak.transform.localScale=Vector3.one*4;
     var veilRenderer=GameObject.Find("Permanent hex exploration overlay").GetComponent<Renderer>();var view=veilRenderer.transform.parent.GetComponent<Camera>();
     var projected=view.WorldToViewportPoint(peak.transform.position);int px=(int)(projected.x*1920),py=(int)(projected.y*1080);
     V2Assert(projected.y>.85f&&projected.y<1,"depth test target is above sea horizon");
     veilRenderer.enabled=false;indianOcean.Render(s,throttle);var exposed=V2Pixel(indianOcean.Image,px,py);
     veilRenderer.enabled=true;indianOcean.Render(s,throttle);var hidden=V2Pixel(indianOcean.Image,px,py);indianOcean.Capture(Path.Combine(output,"diagnostic-hidden.png"));
     peak.SetActive(false);Destroy(peak);
     indianOcean.Render(s,throttle);var emptySky=V2Pixel(indianOcean.Image,px,py);indianOcean.Capture(Path.Combine(output,"diagnostic-sky.png"));
     Debug.Log("FOG_SKY_COMPARE hidden="+hidden+" empty="+emptySky+" exposed="+exposed+" viewport="+projected);
     V2Assert(Vector3.Distance(new Vector3(hidden.r,hidden.g,hidden.b),new Vector3(emptySky.r,emptySky.g,emptySky.b))<.025f,"unexplored elevated geometry blends into matching sky");
     V2Assert(Vector3.Distance(new Vector3(exposed.r,exposed.g,exposed.b),new Vector3(emptySky.r,emptySky.g,emptySky.b))>.05f,"occlusion test target visibly differs from sky");
    }
    indianOcean.Capture(Path.Combine(output,"zoom-"+zoom+".png"));
   }
   indianOcean.SetView(0,1);V2Assert(ManualSeaZoom==24,"near zoom clamp");indianOcean.SetView(0,900);V2Assert(ManualSeaZoom==120,"far zoom clamp");
   // Compare the same world pixel with and without the veil in the same animation frame.
   Vector2 sample=Vector2.zero;bool found=false;var data=s.sailing.southeastAsiaExploration;
   for(int r=SeaExploration.MinR;r<SeaExploration.MinR+SeaExploration.Height&&!found;r++)for(int q=SeaExploration.MinQ;q<SeaExploration.MinQ+SeaExploration.Width;q++){
    if(data.At(q,r)!=1)continue;
    var uv=ManualSeaV2Runtime.UV(new Vector3((float)(SeaExploration.TileSpacing*(q+r*.5)),0,SeaExploration.Radius*1.5f*r));
    var p=ProjectManualSea(uv);if(p.x>100&&p.x<1650&&p.y>560&&p.y<900){sample=p;found=true;break;}
   }
   V2Assert(found,"half-covered ring is visible in gameplay view");
   indianOcean.Render(s,throttle);var covered=V2Pixel(indianOcean.Image,(int)sample.x,1080-(int)sample.y);
   var veil=GameObject.Find("Permanent hex exploration overlay").GetComponent<Renderer>();veil.enabled=false;indianOcean.Render(s,throttle);
   var clear=V2Pixel(indianOcean.Image,(int)sample.x,1080-(int)sample.y);veil.enabled=true;
   float ratio=(covered.r+covered.g+covered.b)/Mathf.Max(.001f,clear.r+clear.g+clear.b);
   V2Assert(ratio>.48f&&ratio<.7f,"half fog visibly attenuates background: "+ratio);
   indianOcean.Render(s,throttle);indianOcean.Capture(Path.Combine(output,"map-cloud-fog.png"));
   var veilMaterial=veil.sharedMaterial;var originalStates=veilMaterial.GetTexture("_Exploration");
   var environment=GameObject.Find("Manual Sailing V2 runtime").GetComponentInChildren<ManualSeaV2Environment>();var originalMode=environment.mode;
   veilMaterial.SetTexture("_Exploration",Texture2D.whiteTexture);
   foreach(var mode in new[]{ManualSeaV2Environment.SkyMode.Sunny,ManualSeaV2Environment.SkyMode.Sunset,ManualSeaV2Environment.SkyMode.Night}){
    indianOcean.SetSkyMode(mode);V2Assert(indianOcean.SkyMode==mode,"test sky button action "+mode);indianOcean.Render(s,throttle);
    var near=V2Pixel(indianOcean.Image,960,800);var far=V2Pixel(indianOcean.Image,960,916);
    V2Assert(Mathf.Max(near.r,Mathf.Max(near.g,near.b))<.015f,"below the 10 percent horizon band stays black in "+mode);
    V2Assert(mode==ManualSeaV2Environment.SkyMode.Sunny?far.b>.8f:mode==ManualSeaV2Environment.SkyMode.Sunset?far.r>far.b*2:far.b>far.r,"horizon fog follows sky palette in "+mode);
    veilMaterial.SetTexture("_Exploration",originalStates);indianOcean.Render(s,throttle);indianOcean.Capture(Path.Combine(output,"horizon-"+mode+".png"));veilMaterial.SetTexture("_Exploration",Texture2D.whiteTexture);
   }
   veilMaterial.SetTexture("_Exploration",originalStates);environment.mode=originalMode;
   var cloudLayer=GameObject.Find("Cartoon cloud layer");V2Assert(cloudLayer&&Mathf.Abs(cloudLayer.transform.position.y-7.5f)<.01f,"cloud base lowered another 25 percent to 7.5");
   int previousCloudCount=0;for(int z=-9;z<=9;z++)for(int x=-13;x<=13;x++)if(Mathf.Repeat((x*17+z*59+x*z*13)/101f,1)>=.62f)previousCloudCount++;
   int cloudCount=cloudLayer.GetComponent<ManualSeaV2CloudLayer>().CloudCount;
   V2Assert(cloudCount>previousCloudCount*1.9f&&cloudCount<previousCloudCount*2.1f,"approximately twice the clouds: "+previousCloudCount+" -> "+cloudCount);
   var clouds=cloudLayer.GetComponent<ManualSeaV2CloudLayer>();var weather=s.sailing;
   double ox=weather.cloudOffsetX,oz=weather.cloudOffsetZ,slow=c.balance.navigation.windSpeed*4*c.balance.navigation.minSailSpeed*.02;
   clouds.AdvanceWeather(weather,0,10,slow);
   V2Assert(Math.Abs(weather.cloudOffsetX-ox-slow*960*10)<1e-8&&Math.Abs(weather.cloudOffsetZ-oz)<1e-8,"clouds drift east at minimum ship speed");
   double east=weather.cloudOffsetX;clouds.AdvanceWeather(weather,Math.PI/2,10,slow);
   V2Assert(Math.Abs(weather.cloudOffsetX-east)<1e-8&&Math.Abs(weather.cloudOffsetZ-oz+slow*540*10)<1e-8,"wind turns without cloud teleport");
   var restoredWeather=JsonUtility.FromJson<MaritimeLegend.Domain.SailingState>(JsonUtility.ToJson(weather));clouds.BindWeather(restoredWeather);
   V2Assert(restoredWeather.cloudSeed==weather.cloudSeed&&(clouds.Drift-new Vector3((float)weather.cloudOffsetX,0,(float)weather.cloudOffsetZ)).sqrMagnitude<1e-8,"cloud seed and drift survive save reload");
   clouds.BindWeather(weather);weather.cloudOffsetX=ox;weather.cloudOffsetZ=oz;
   veil.enabled=false;indianOcean.SetView(0,60);indianOcean.Render(s,throttle);indianOcean.Capture(Path.Combine(output,"cloud-detail.png"));
   veil.enabled=true;indianOcean.SetView(0,120);indianOcean.Render(s,throttle);
  }catch(Exception e){Debug.LogException(e);Application.Quit(1);yield break;}
  yield return new WaitForSecondsRealtime(2);
  paused=false;yield return null;yield return null;LiveGui.Capture(Path.Combine(output,"game-manual-sailing.png"));
  page="城市";yield return new WaitForSecondsRealtime(2);
  V2Assert(!GameObject.Find("Manual Sailing V2 runtime"),"V2 render layer inactive outside sailing");
  Debug.Log("SAILING_V2_INTEGRATION_PASS");Application.Quit(0);
 }
 static Color V2Pixel(RenderTexture target,int x,int y){var old=RenderTexture.active;RenderTexture.active=target;var tex=new Texture2D(1,1,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(x,y,1,1),0,0);tex.Apply();var color=tex.GetPixel(0,0);Destroy(tex);RenderTexture.active=old;return color;}
 static void V2Assert(bool result,string label){if(!result)throw new Exception("V2 CHECK FAILED: "+label);Debug.Log("V2_CHECK_PASS "+label);}
}}
#endif
