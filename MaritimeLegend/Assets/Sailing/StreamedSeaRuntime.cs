using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// World adapter around the original renderer. It owns only map residency and coordinate conversion.
public sealed class StreamedSeaRuntime:IDisposable {
 readonly ManualSeaV2Runtime renderer;readonly StreamedSeaTerrain terrain;readonly StreamedSeaAtlas atlas;
 readonly StreamedSeaPorts ports;
 readonly GameState viewState=new GameState();readonly Mesh waterTile;readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
 readonly List<int> priority=new List<int>();readonly HashSet<int> retained=new HashSet<int>();readonly List<Display> display=new List<Display>();
 readonly Dictionary<long,StreamedHex> explored=new Dictionary<long,StreamedHex>();readonly Color[] fog=new Color[SeaExploration.Width*SeaExploration.Height];
 SailingState owner;double focusX,focusY;bool focused;int lastQ=int.MinValue,lastR;float lastSight;
 struct Display {public int key;public float x,y;}
 public WorldMapManifest Map {get;}public StreamedSeaNavigation Navigation {get;}public StreamedSeaNavigation FollowerNavigation {get;}
 public RenderTexture Image=>renderer.Image;public float Yaw=>renderer.Yaw;public float Zoom=>renderer.Zoom;public float Pitch=>renderer.Pitch;
 public bool NavigationReady {get;private set;}public int ResidentChunks=>terrain.Count;public int PendingChunks=>terrain.Pending;public int Failures=>terrain.Failures;public int Evictions=>terrain.Evictions;
 public int DisplayCount=>display.Count;public long EstimatedBytes=>terrain.Bytes;public float SightRadius=72;public float CitySightRadius=36;
 public string Diagnostic=>$"海域 {ResidentChunks} · 加载 {PendingChunks}"+(Failures>0?$" · 重试 {Failures}":"");
 public StreamedSeaRuntime(Transform parent){
  var text=Resources.Load<TextAsset>("Data/StreamedSeasV2/atlas");if(!text)throw new InvalidOperationException("Missing streamed regional atlas");atlas=JsonUtility.FromJson<StreamedSeaAtlas>(text.text);atlas.Validate();Resources.UnloadAsset(text);
  Map=new WorldMapManifest{version=1,worldId=WorldMap.Id,unitsPerDegree=32,chunkCells=64,columns=72,rows=32,ports=atlas.ports,chunks=new WorldChunkRecord[2304]};
  for(int i=0;i<2304;i++)Map.chunks[i]=new WorldChunkRecord{x=i%72,y=i/72,ocean=atlas.ocean[i],key="Data/StreamedSeasV2/Chunks/"+(i%72).ToString("00")+"_"+(i/72).ToString("00")};Map.Validate();
  terrain=new StreamedSeaTerrain(atlas);Navigation=new StreamedSeaNavigation(terrain);FollowerNavigation=new StreamedSeaNavigation(terrain);
  renderer=new ManualSeaV2Runtime(parent,"Environment/StreamedSeasV2/Runtime",false){ExternalExploration=true};renderer.DrawTerrain=Draw;
  ports=new StreamedSeaPorts(parent,renderer.View,terrain);
  waterTile=new Mesh{name="SEA streamed water plane"};waterTile.vertices=new[]{Vector3.zero,new Vector3(160,0,0),new Vector3(0,0,160),new Vector3(160,0,160)};waterTile.triangles=new[]{0,2,1,1,2,3};waterTile.RecalculateBounds();
  viewState.sailing=new SailingState();viewState.sailing.manualFollowers=new[]{new ManualFollowerState(),new ManualFollowerState()};
 }
 public void SetView(float yaw,float zoom)=>renderer.SetView(yaw,zoom);
 public void ConfigureCamera(float min,float max,float pitch)=>renderer.ConfigureCamera(min,max,pitch);
 public void Orbit(float drag,float scroll)=>renderer.Orbit(drag,scroll);
 public void SetSkyMode(ManualSeaV2Environment.SkyMode mode)=>renderer.SetSkyMode(mode);
 public void AdvanceWeather(SailingState state,double wind,double seconds,double speed)=>renderer.AdvanceWeather(state,wind,seconds,speed);
 Vector2 Local(double x,double y)=>new Vector2((float)((focusX+WorldMap.DeltaX(WorldMap.Wrap(focusX),x))/960+.5),(float)(.5-y/540));
 public void Center(double x,double y){focusX=focused?focusX+WorldMap.DeltaX(WorldMap.Wrap(focusX),x):x;focusY=y;focused=true;renderer.Center(Local(x,y));renderer.SetStreamingRange(new Vector2((float)focusX,(float)focusY));}
 public Vector2 Project(Vector2 p){var camera=renderer.View;var d=ManualSeaV2Runtime.World(Local(p.x,p.y))-camera.transform.position;float depth=Vector3.Dot(d,camera.transform.forward);if(depth<=0)return new Vector2(-10000,-10000);float half=depth*Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);return new Vector2((.5f+Vector3.Dot(d,camera.transform.right)/(2*half*camera.aspect))*1920,(.5f-Vector3.Dot(d,camera.transform.up)/(2*half))*1080);}
 public bool TryUnproject(Vector2 pointer,out Vector2 point){var camera=renderer.View;float tan=Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);var dir=camera.transform.forward+camera.transform.right*((pointer.x/960-1)*tan*camera.aspect)+camera.transform.up*((1-pointer.y/540)*tan);var start=camera.transform.position;point=default;if(dir.y>=-.00001f)return false;float t=-start.y/dir.y;if(t<=0||t*dir.magnitude>12000)return false;var p=start+dir*t;point=new Vector2((float)WorldMap.Wrap(p.x),(float)WorldMap.ClampY(p.z));return true;}
 public void Update(GameState state){
  Center(state.seaX,state.seaY);priority.Clear();retained.Clear();display.Clear();var required=new HashSet<int>();
  int fx=(int)Math.Floor(focusX/160),fy=(int)Math.Floor(focusY/160);
  var candidates=new List<Tuple<int,float>>();
  // All-direction cushion: rotating the original camera does not invalidate the load set.
  for(int y=Math.Max(0,fy-6);y<=Math.Min(31,fy+6);y++)for(int x=fx-6;x<=fx+6;x++){
   float dx=Mathf.Max(0,Mathf.Abs((float)focusX-(x*160+80))-80),dy=Mathf.Max(0,Mathf.Abs((float)focusY-(y*160+80))-80),d=Mathf.Sqrt(dx*dx+dy*dy);int key=y*72+WorldMap.WrapColumn(x);
   if(d>850)continue;retained.Add(key);if(d<=620){candidates.Add(Tuple.Create(key,d));display.Add(new Display{key=key,x=x*160,y=y*160});}if(d<=Mathf.Max(45,SightRadius+21))required.Add(key);
  }
  priority.AddRange(candidates.OrderBy(c=>c.Item2).Select(c=>c.Item1));terrain.Update(priority,retained,Time.realtimeSinceStartup);NavigationReady=required.All(terrain.Ready);
  if(state.sailing.manualFollowers==null&&NavigationReady){
   state.sailing.manualFollowers=new ManualFollowerState[2];var occupied=new List<Vector2>{new Vector2((float)state.seaX,(float)state.seaY)};
   for(int i=0;i<2;i++){bool found=false;double px=state.seaX,py=state.seaY;
    for(int ring=0;ring<5&&!found;ring++)for(int k=0;k<32&&!found;k++){double a=state.heading+Math.PI+k*Math.PI/16;double x=WorldMap.Wrap(state.seaX+Math.Cos(a)*(14+ring*9)),y=WorldMap.ClampY(state.seaY-Math.Sin(a)*(14+ring*9));
     if(FollowerNavigation.Safe(x,y)&&occupied.All(p=>WorldMap.Distance(p.x,p.y,x,y)>12)){px=x;py=y;found=true;}}
    if(!found){state.sailing.manualFollowers=null;NavigationReady=false;return;}state.sailing.manualFollowers[i]=new ManualFollowerState{x=px,y=py,heading=state.heading};occupied.Add(new Vector2((float)px,(float)py));
   }
  }
  if(state.sailing.manualFollowers!=null)foreach(var f in state.sailing.manualFollowers)NavigationReady&=terrain.Ready(WorldMap.Key(f.x,f.y));
  UpdateExploration(state.sailing);
 }
 public void StepFollowers(GameState state,double seconds,double speed){
  if(state.sailing.manualFollowers==null)return;double lx=state.seaX,ly=state.seaY;
  // Original follower law, expressed in original map pixels; original clearance/slide kernel.
  float pixelSpeed=(float)speed;float gap=Mathf.Lerp(18,24,Mathf.Clamp01(pixelSpeed/2.4f));
  foreach(var f in state.sailing.manualFollowers){double dx=WorldMap.DeltaX(f.x,lx)/960*CoastNavigation.Width,dy=(f.y-ly)/540*CoastNavigation.Height,dist=Math.Sqrt(dx*dx+dy*dy);
   if(dist>.001){double target=Math.Atan2(dy,dx);f.heading=SailingFormulas.Turn(f.heading,target,turnRate,seconds);double alignment=Math.Max(0,Math.Cos(target-f.heading)),travel=Math.Min(Math.Max(0,dist-gap),Math.Max(2.4,pixelSpeed*1.5)*seconds*alignment);
    FollowerNavigation.ResolveMove(f.x,f.y,f.x+Math.Cos(f.heading)*travel/CoastNavigation.Width*960,f.y-Math.Sin(f.heading)*travel/CoastNavigation.Height*540,out var x,out var y);f.x=x;f.y=y;}
   lx=f.x;ly=f.y;
  }
 }
 public double turnRate=90;
 void Draw(Camera camera,Material water,ManualSeaV2Instances props){
  // Continuous base ocean also extends beyond the polar navigation walls.
  properties.Clear();properties.SetVector("_MapBounds",new Vector4(-1,-1,1,1));
  Graphics.DrawMesh(waterTile,Matrix4x4.TRS(new Vector3((float)focusX-6000,-.02f,(float)focusY-6000),Quaternion.identity,Vector3.one*75),water,28,camera,0,properties,false,false);
  var cells=new List<ManualSeaV2Instances.Cell>();var planes=GeometryUtility.CalculateFrustumPlanes(camera);
  foreach(var d in display){var t=terrain.Find(d.key);if(t==null||!t.Ready)continue;var bounds=new Bounds(new Vector3(d.x+80,15,d.y+80),new Vector3(160,96,160));if(!GeometryUtility.TestPlanesAABB(planes,bounds))continue;
   var matrix=Matrix4x4.Translate(new Vector3(d.x,0,d.y));Graphics.DrawMesh(t.mesh,matrix,props.material,28,camera,0,null,UnityEngine.Rendering.ShadowCastingMode.On,true);
   properties.Clear();properties.SetTexture("_DepthMap",t.depth);float extent=StreamedSeaTerrain.Side*StreamedSeaTerrain.Step;
   properties.SetVector("_MapBounds",new Vector4(d.x-StreamedSeaTerrain.Step*1.5f,d.y-StreamedSeaTerrain.Step*1.5f,extent,extent));
   Graphics.DrawMesh(waterTile,matrix,water,28,camera,0,properties,false,false);
   if(t.displayProps==null||t.displayX!=d.x){t.displayX=d.x;t.displayProps=t.data.props.Select(cell=>{var transforms=new Matrix4x4[cell.transforms.Length];for(int i=0;i<transforms.Length;i++)transforms[i]=matrix*cell.transforms[i];return new ManualSeaV2Instances.Cell{variant=cell.variant,bounds=new Bounds(cell.bounds.center+new Vector3(d.x,0,d.y),cell.bounds.size),transforms=transforms};}).ToArray();}cells.AddRange(t.displayProps);
  }props.cells=cells.ToArray();props.lodDistance=150;
 }
 static long FogKey(int q,int r){double x=SeaExploration.TileSpacing*(q+r*.5),z=18*r;SeaExploration.Hex(WorldMap.Wrap(x),z,out int a,out int b);return ((long)a<<32)|(uint)b;}
 void UpdateExploration(SailingState state){
  if(owner!=state){owner=state;explored.Clear();if(state.streamedExploration==null)state.streamedExploration=new List<StreamedHex>();foreach(var h in state.streamedExploration)explored[((long)h.x<<32)|(uint)h.y]=h;lastQ=int.MinValue;}
  SeaExploration.Hex(focusX,focusY,out int q,out int r);bool moved=lastQ!=q||lastR!=r||lastSight!=SightRadius;lastQ=q;lastR=r;lastSight=SightRadius;
  int extent=(int)Math.Ceiling((SightRadius+21)/18);bool changed=false;
  for(int y=r-extent;y<=r+extent;y++)for(int x=q-extent;x<=q+extent;x++){double wx=SeaExploration.TileSpacing*(x+y*.5),wy=y*18,dist=Math.Sqrt((wx-focusX)*(wx-focusX)+(wy-focusY)*(wy-focusY));if(dist>SightRadius+SeaExploration.TileSpacing)continue;long key=FogKey(x,y);int value=dist<=SightRadius?2:1;
   if(!explored.TryGetValue(key,out var h)){h=new StreamedHex{x=(int)(key>>32),y=(int)key};explored.Add(key,h);state.streamedExploration.Add(h);}if(h.value<value){h.value=value;changed=true;}}
  if(!moved&&!changed)return;
  for(int y=0;y<64;y++)for(int x=0;x<96;x++){var key=FogKey(q+x-48,r+y-32);int value=explored.TryGetValue(key,out var h)?h.value:0;float a=SeaExploration.FogOpacity(value);fog[y*96+x]=new Color(a,a,a,1);}renderer.SetExploration(fog,new Vector2(q,r));
 }
 public void Render(GameState state,float opening){
  Update(state);var uv=Local(state.seaX,state.seaY);viewState.seaX=uv.x;viewState.seaY=uv.y;viewState.heading=state.heading;viewState.trim=state.trim;viewState.wind=state.wind;
  viewState.sailing.cloudSeed=state.sailing.cloudSeed;viewState.sailing.cloudOffsetX=state.sailing.cloudOffsetX;viewState.sailing.cloudOffsetZ=state.sailing.cloudOffsetZ;
  for(int i=0;i<2;i++){var f=state.sailing.manualFollowers?[i];var pos=f==null?uv:Local(f.x,f.y);var proxy=viewState.sailing.manualFollowers[i];proxy.x=pos.x;proxy.y=pos.y;proxy.heading=f==null?state.heading:f.heading;}
  ports.SetActive(true);ports.Update(Map.ports,focusX,focusY,CitySightRadius,renderer.SkyMode);
  Shader.SetGlobalFloat("_SeaLargeWorld",1);try{renderer.Render(viewState,opening);}finally{Shader.SetGlobalFloat("_SeaLargeWorld",0);}
 }
 public string HoverPort(Vector2 pointer)=>ports.Hover(pointer);
 public void Capture(string path)=>renderer.Capture(path);public void SetActive(bool active){renderer.SetActive(active);ports.SetActive(active);}
 public void Dispose(){ports.Dispose();terrain.Dispose();renderer.Dispose();UnityEngine.Object.Destroy(waterTile);}
}
}
