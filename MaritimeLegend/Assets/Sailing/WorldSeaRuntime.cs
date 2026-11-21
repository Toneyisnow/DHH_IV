using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// A new world environment; never instantiates the regional all-assets prefab.
public sealed class WorldSeaRuntime : IDisposable {
 const int Layer=28;
 const float DrawDistance=560,RetainDistance=800;
 readonly GameObject root;readonly Camera camera;readonly ManualSeaV2Camera orbit;
 readonly WorldChunkRepository repository;readonly Material land,water,shipMaterial;
 readonly Material[] skies=new Material[3];readonly ManualSeaV2Ship[] ships=new ManualSeaV2Ship[3];
 readonly Mesh seaMesh,shipMesh;readonly MaterialPropertyBlock props=new MaterialPropertyBlock();
 readonly List<Display> displays=new List<Display>();readonly List<int> priority=new List<int>();readonly HashSet<int> retained=new HashSet<int>();
 readonly List<GameObject> portMarkers=new List<GameObject>();readonly Material portMaterial;
 readonly Dictionary<int,byte[]> explored=new Dictionary<int,byte[]>();SailingState bound;
 double lastRevealX=double.NaN,lastRevealY;float lastRevealRadius;
 readonly Plane[] planes=new Plane[6];double originX,originY,focusX,focusY;bool focused;
 float minimumZoom=12,maximumZoom=180;
 public WorldMapManifest Map {get;}public WorldNavigation Navigation {get;}public WorldNavigation FollowerNavigation {get;}
 public RenderTexture Image {get;}public float Yaw=>orbit.yaw;public float Zoom=>orbit.distance;public float Pitch=>orbit.pitch;
 public ManualSeaV2Environment.SkyMode SkyMode {get;private set;}
 public bool NavigationReady {get;private set;}public int ResidentChunks=>repository.Count;public int PendingChunks=>repository.Pending;
 public int DisplayCount=>displays.Count;public string Diagnostic=>$"海域 {ResidentChunks} / {WorldMap.Columns*WorldMap.Rows} · 加载 {PendingChunks}"+(repository.Failures>0?$" · 重试 {repository.Failures}":"");
 public long EstimatedBytes=>repository.EstimatedBytes;
 public float SightRadius=50;
 struct Display {public int key;public double x,y;public float distance;}
 public WorldSeaRuntime(Transform owner){
  var asset=Resources.Load<TextAsset>("Data/WorldMap/world");if(!asset)throw new InvalidOperationException("Missing world map. Run ArtSource/WorldMap/build_world.py");
  Map=JsonUtility.FromJson<WorldMapManifest>(asset.text);Map.Validate();Resources.UnloadAsset(asset);
  repository=new WorldChunkRepository(Map);Navigation=new WorldNavigation(repository);FollowerNavigation=new WorldNavigation(repository);
  root=new GameObject("World manual sailing");root.transform.SetParent(owner,false);root.layer=Layer;
  camera=new GameObject("World sailing camera").AddComponent<Camera>();camera.transform.SetParent(root.transform,false);camera.enabled=false;camera.cullingMask=1<<Layer;camera.nearClipPlane=.1f;camera.farClipPlane=2000;camera.clearFlags=CameraClearFlags.Skybox;camera.allowHDR=false;
  orbit=camera.gameObject.AddComponent<ManualSeaV2Camera>();orbit.enabled=false;orbit.pitch=35;orbit.skyHeightFraction=.15f;
  Image=new RenderTexture(1920,1080,24){name="World sailing",antiAliasing=2};Image.Create();camera.targetTexture=Image;camera.aspect=16f/9;
  land=new Material(Resources.Load<Shader>("Shaders/WorldSeaLand"));water=new Material(Resources.Load<Shader>("Shaders/WorldSeaWater"));shipMaterial=new Material(Resources.Load<Shader>("Shaders/ManualSeaV2Ship"));
  portMaterial=new Material(land){name="World port markers"};
  seaMesh=new Mesh{name="World water tile"};seaMesh.vertices=new[]{new Vector3(0,0,0),new Vector3(160,0,0),new Vector3(0,0,160),new Vector3(160,0,160)};seaMesh.triangles=new[]{0,2,1,1,2,3};seaMesh.RecalculateNormals();seaMesh.RecalculateBounds();
  shipMesh=new Mesh{name="World existing ship art"};shipMesh.vertices=new[]{new Vector3(-5,-.6f,0),new Vector3(5,-.6f,0),new Vector3(-5,9.4f,0),new Vector3(5,9.4f,0)};shipMesh.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1)};shipMesh.triangles=new[]{0,2,1,1,2,3};shipMesh.RecalculateBounds();
  string[] types={"junk","square","lateen"},dirs={"E","SE","S","SW","W","NW","N","NE"};
  for(int i=0;i<3;i++){
   var go=new GameObject("World ship "+i);go.layer=Layer;go.transform.SetParent(root.transform,false);ships[i]=go.AddComponent<ManualSeaV2Ship>();ships[i].enabled=false;ships[i].view=camera;ships[i].phase=i*2.399f;
   var art=new GameObject("Existing directional ship",typeof(MeshFilter),typeof(MeshRenderer));art.layer=Layer;art.transform.SetParent(go.transform,false);art.GetComponent<MeshFilter>().sharedMesh=shipMesh;art.GetComponent<Renderer>().sharedMaterial=shipMaterial;
   ships[i].billboard=art.transform;ships[i].art=art.GetComponent<Renderer>();ships[i].directions=dirs.Select(d=>Resources.Load<Texture2D>("Sprites/ManualSailing/"+types[i]+"/"+d)).ToArray();
  }
  for(int i=0;i<3;i++){skies[i]=new Material(Resources.Load<Shader>("Shaders/ManualSeaV2Sky"));skies[i].SetFloat("_Mode",i);skies[i].SetFloat("_StarDensity",3);skies[i].SetFloat("_StarBrightness",4);skies[i].SetVector("_SunDirection",-(Quaternion.Euler(i==0?48:8,i==0?-35:180,0)*Vector3.forward));}
  foreach(var port in Map.ports){var marker=GameObject.CreatePrimitive(PrimitiveType.Cylinder);marker.name="World berth "+port.name;marker.layer=Layer;marker.transform.SetParent(root.transform,false);WorldSeaAssets.Destroy(marker.GetComponent<Collider>());marker.transform.localScale=new Vector3(2,3,2);marker.GetComponent<Renderer>().sharedMaterial=portMaterial;marker.SetActive(false);portMarkers.Add(marker);}
  if(LiveGui.Camera)LiveGui.Camera.cullingMask&=~(1<<Layer);SetView(0,85);
 }
 public void SetSkyMode(ManualSeaV2Environment.SkyMode mode){SkyMode=mode;}
 public void ConfigureCamera(float min,float max,float pitch){minimumZoom=Mathf.Max(8,min);maximumZoom=Mathf.Clamp(max,minimumZoom,180);orbit.pitch=Mathf.Clamp(pitch,20,55);SetView(Yaw,Zoom);}
 public void SetView(float yaw,float zoom){orbit.yaw=yaw;orbit.distance=Mathf.Clamp(zoom,minimumZoom,maximumZoom);orbit.ApplyPose();}
 public void Orbit(float drag,float scroll)=>SetView(Yaw+drag*2.2f,Zoom*Mathf.Exp(-scroll*.12f));
 public void Center(double x,double y){
  if(!focused){focusX=x;focused=true;}else focusX+=WorldMap.DeltaX(WorldMap.Wrap(focusX),x);focusY=y;
  if(Math.Abs(focusX-originX)>320||Math.Abs(focusY-originY)>320){originX=Math.Floor(focusX/160)*160;originY=Math.Floor(focusY/160)*160;}
  orbit.focus=new Vector3((float)(focusX-originX),0,(float)(focusY-originY));orbit.ApplyPose();
 }
 Vector3 Local(double x,double y)=>new Vector3((float)(focusX+WorldMap.DeltaX(WorldMap.Wrap(focusX),x)-originX),0,(float)(y-originY));
 public Vector2 Project(Vector2 p){
  var delta=Local(p.x,p.y)-camera.transform.position;float depth=Vector3.Dot(delta,camera.transform.forward);
  if(depth<=0)return new Vector2(-10000,-10000);float half=depth*Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
  return new Vector2((.5f+Vector3.Dot(delta,camera.transform.right)/(2*half*camera.aspect))*1920,(.5f-Vector3.Dot(delta,camera.transform.up)/(2*half))*1080);
 }
 public bool TryUnproject(Vector2 pointer,out Vector2 p){
  // Construct the direction in the camera basis. Inverting near-plane world
  // points can lose centimetres at maximum zoom even with a floating origin.
  float tangent=Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
  var direction=camera.transform.forward+camera.transform.right*((pointer.x/960-1)*tangent*camera.aspect)+camera.transform.up*((1-pointer.y/540)*tangent);
  var start=camera.transform.position;p=default;if(direction.y>=-.00001f)return false;
  float t=-start.y/direction.y;if(t<=0||t*direction.magnitude>2000)return false;
  var v=start+direction*t;p=new Vector2((float)WorldMap.Wrap(v.x+originX),(float)WorldMap.ClampY(v.z+originY));return true;
 }
 public void Update(GameState state){
  Center(state.seaX,state.seaY);BindExploration(state.sailing);Reveal(state.seaX,state.seaY);
  retained.Clear();priority.Clear();displays.Clear();var required=new HashSet<int>();
  Action<double,double,double,HashSet<int>> cover=(x,y,r,set)=>{
   int x0=(int)Math.Floor((x-r)/160),x1=(int)Math.Floor((x+r)/160),y0=Math.Max(0,(int)Math.Floor((y-r)/160)),y1=Math.Min(31,(int)Math.Floor((y+r)/160));
   for(int cy=y0;cy<=y1;cy++)for(int cx=x0;cx<=x1;cx++)set.Add(cy*72+WorldMap.WrapColumn(cx));
  };
  cover(focusX,focusY,40,required);
  if(state.sailing.manualFollowers!=null)foreach(var f in state.sailing.manualFollowers)cover(focusX+WorldMap.DeltaX(state.seaX,f.x),f.y,32,required);
  priority.AddRange(required);foreach(int key in required)retained.Add(key);
  GeometryUtility.CalculateFrustumPlanes(camera,planes);
  double cameraX=camera.transform.position.x+originX,cameraY=camera.transform.position.z+originY;
  int minX=(int)Math.Floor((cameraX-RetainDistance)/160),maxX=(int)Math.Floor((cameraX+RetainDistance)/160);
  int minY=Math.Max(0,(int)Math.Floor((cameraY-RetainDistance)/160)),maxY=Math.Min(31,(int)Math.Floor((cameraY+RetainDistance)/160));
  var prefetch=new List<Display>();
  for(int cy=minY;cy<=maxY;cy++)for(int cx=minX;cx<=maxX;cx++){
   double wx=cx*160,wy=cy*160;int key=cy*72+WorldMap.WrapColumn(cx);
   double dx=Math.Max(0,Math.Abs(wx+80-cameraX)-80),dy=Math.Max(0,Math.Abs(wy+80-cameraY)-80);float distance=(float)Math.Sqrt(dx*dx+dy*dy);
   if(distance>RetainDistance)continue;retained.Add(key);
   if(distance>DrawDistance+160)continue;
   var d=new Display{key=key,x=wx,y=wy,distance=distance};
   var bounds=new Bounds(new Vector3((float)(wx+80-originX),20,(float)(wy+80-originY)),new Vector3(160,70,160));
   bool visible=distance<=DrawDistance&&GeometryUtility.TestPlanesAABB(planes,bounds);
   // All-direction low-detail cushion allows rapid camera turns without waiting for IO.
   if(visible){displays.Add(d);prefetch.Add(d);}else if(distance<DrawDistance)prefetch.Add(d);
  }
  foreach(var d in prefetch.OrderBy(d=>d.distance))if(!required.Contains(d.key))priority.Add(d.key);
  repository.Update(priority,retained,Time.realtimeSinceStartup);
  NavigationReady=required.All(k=>repository.Ready(k));
  if(state.sailing.manualFollowers==null&&NavigationReady){
   state.sailing.manualFollowers=new ManualFollowerState[2];
   double px=state.seaX,py=state.seaY;
   for(int i=0;i<2;i++){double fx=px,fy=py;bool found=false;
    for(int ring=0;ring<3&&!found;ring++)for(int j=0;j<24&&!found;j++){
     double angle=state.heading+Math.PI+j*Math.PI/12,radius=14+ring*8;
     double tx=WorldMap.Wrap(px+Math.Cos(angle)*radius),ty=WorldMap.ClampY(py-Math.Sin(angle)*radius);
     if(FollowerNavigation.Safe(tx,ty)&&WorldMap.Distance(state.seaX,state.seaY,tx,ty)>10){fx=tx;fy=ty;found=true;}
    }
    state.sailing.manualFollowers[i]=new ManualFollowerState{x=fx,y=fy,heading=state.heading};px=fx;py=fy;
   }
  }
 }
 public void StepFollowers(GameState state,double seconds,double speed){
  if(state.sailing.manualFollowers==null)return;double px=state.seaX,py=state.seaY;
  foreach(var f in state.sailing.manualFollowers){
   double dx=WorldMap.DeltaX(f.x,px),dy=py-f.y,dist=Math.Sqrt(dx*dx+dy*dy);
   if(speed>1e-6&&dist>13){f.heading=SailingFormulas.Turn(f.heading,Math.Atan2(-dy,dx),90,seconds);double move=Math.Min(dist-13,speed*Math.Min(1.8,dist/16)*seconds);
    FollowerNavigation.ResolveMove(f.x,f.y,f.x+Math.Cos(f.heading)*move,f.y-Math.Sin(f.heading)*move,out double x,out double y);f.x=x;f.y=y;}
   px=f.x;py=f.y;
  }
 }
 void BindExploration(SailingState state){if(bound==state)return;bound=state;lastRevealX=double.NaN;explored.Clear();if(state.worldExploration==null)state.worldExploration=new List<WorldExploredChunk>();foreach(var c in state.worldExploration){var a=Convert.FromBase64String(c.bits);if(a.Length!=32)throw new InvalidOperationException("Invalid world exploration");explored[c.key]=a;}}
 void Reveal(double x,double y){
  if(WorldMap.Finite(lastRevealX)&&lastRevealRadius==SightRadius&&WorldMap.Distance(lastRevealX,lastRevealY,x,y)<5)return;
  lastRevealX=x;lastRevealY=y;lastRevealRadius=SightRadius;
  const double spacing=10;double radius=SightRadius;
  for(int gy=(int)((y-radius)/spacing);gy<=(y+radius)/spacing;gy++)for(int gx=(int)Math.Floor((x-radius)/spacing);gx<=(x+radius)/spacing;gx++){
   double xx=WorldMap.Wrap((gx+.5)*spacing),yy=(gy+.5)*spacing;if(yy<0||yy>WorldMap.Height||WorldMap.Distance(x,y,xx,yy)>radius)continue;
   int key=WorldMap.Key(xx,yy),bit=((int)(yy%160)/10)*16+(int)(xx%160)/10;
   if(!explored.TryGetValue(key,out var a)){a=new byte[32];explored.Add(key,a);bound.worldExploration.Add(new WorldExploredChunk{key=key,bits=Convert.ToBase64String(a)});}
   byte mask=(byte)(1<<(bit%8));if((a[bit/8]&mask)!=0)continue;a[bit/8]|=mask;bound.worldExploration.Find(c=>c.key==key).bits=Convert.ToBase64String(a);
  }
 }
 public void Render(GameState state,float opening){
  root.SetActive(true);Update(state);var oldSky=RenderSettings.skybox;bool oldFog=RenderSettings.fog;
  try{
   int mode=(int)SkyMode;Shader.SetGlobalFloat("_WorldSeaSkyMode",mode);RenderSettings.skybox=skies[mode];RenderSettings.fog=false;
   var fog=mode==0?new Color(.61f,.79f,.87f):mode==1?new Color(.57f,.35f,.27f):new Color(.055f,.09f,.16f);
   Shader.SetGlobalColor("_WorldSeaFogColor",fog);Shader.SetGlobalVector("_WorldSeaFogRange",new Vector4(300,550,0,0));
   Shader.SetGlobalVector("_WorldSeaOrigin",new Vector4((float)WorldMap.Wrap(originX),(float)originY,0,0));
   Shader.SetGlobalVector("_WorldSeaLight",-(Quaternion.Euler(mode==0?48:8,mode==0?-35:180,0)*Vector3.forward));
   Shader.SetGlobalColor("_WorldSeaTint",mode==0?Color.white:mode==1?new Color(1,.72f,.49f):new Color(.28f,.40f,.62f));
   props.Clear();props.SetFloat("_HasCoast",0);
   Graphics.DrawMesh(seaMesh,Matrix4x4.TRS(new Vector3(orbit.focus.x-1600,-.08f,orbit.focus.z-1600),Quaternion.identity,new Vector3(20,1,20)),water,Layer,camera,0,props,false,false);
   foreach(var d in displays){var e=repository.Find(d.key);if(e==null||!e.ready)continue;
    var pos=new Vector3((float)(d.x-originX),0,(float)(d.y-originY));var matrix=Matrix4x4.TRS(pos,Quaternion.identity,Vector3.one);
    Graphics.DrawMesh(d.distance<330?e.near:e.far,matrix,land,Layer,camera,0,null,false,false);
    props.Clear();props.SetFloat("_HasCoast",1);props.SetTexture("_DistanceMap",e.distance);props.SetVector("_ChunkOrigin",new Vector4(pos.x,pos.z,160,160));
    Graphics.DrawMesh(seaMesh,matrix,water,Layer,camera,0,props,false,false);
   }
   for(int i=0;i<ships.Length;i++){var f=i==0?null:state.sailing.manualFollowers!=null&&state.sailing.manualFollowers.Length>=i?state.sailing.manualFollowers[i-1]:null;
    ships[i].gameObject.SetActive(i==0||f!=null);if(i>0&&f==null)continue;
    ships[i].transform.position=Local(f==null?state.seaX:f.x,f==null?state.seaY:f.y);double angle=f==null?state.heading:f.heading;
    ships[i].heading=new Vector3((float)Math.Cos(angle),0,-(float)Math.Sin(angle));ships[i].sailOpening=opening;ships[i].sailTrim=(float)SailingFormulas.SailDegrees(state.trim);ships[i].windFill=(float)SailingFormulas.WindFill(angle,state.trim,state.wind);ships[i].Refresh();ships[i].art.GetPropertyBlock(props);props.SetColor("_Tint",mode==0?Color.white:mode==1?new Color(1,.73f,.48f):new Color(.36f,.48f,.68f));ships[i].art.SetPropertyBlock(props);
   }
   for(int i=0;i<Map.ports.Length;i++){var p=Map.ports[i];bool visible=WorldMap.Distance(state.seaX,state.seaY,p.x,p.y)<300;portMarkers[i].SetActive(visible);if(visible)portMarkers[i].transform.position=Local(p.x,p.y)+new Vector3(8,3,8);}
   camera.Render();
  }finally{RenderSettings.skybox=oldSky;RenderSettings.fog=oldFog;}
 }
 public string HoverPort(Vector2 pointer){foreach(var p in Map.ports){if(WorldMap.Distance(WorldMap.Wrap(focusX),focusY,p.x,p.y)>300)continue;if(Vector2.Distance(Project(new Vector2((float)p.x,(float)p.y)),pointer)<24)return p.name;}return null;}
 public void Capture(string path){var previous=RenderTexture.active;RenderTexture.active=Image;var tex=new Texture2D(Image.width,Image.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,Image.width,Image.height),0,0);tex.Apply();System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());WorldSeaAssets.Destroy(tex);RenderTexture.active=previous;}
 public void SetActive(bool value){root.SetActive(value);}
 public void Dispose(){repository.Dispose();Image.Release();foreach(var obj in new UnityEngine.Object[]{root,Image,land,water,portMaterial,shipMaterial,seaMesh,shipMesh})if(obj)WorldSeaAssets.Destroy(obj);foreach(var sky in skies)WorldSeaAssets.Destroy(sky);}
}
}
