using System;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// Owns the V2 scene as an isolated render layer. The existing game remains the simulation owner.
public sealed class ManualSeaV2Runtime : IDisposable {
 const int Layer=28;
 readonly GameObject root; readonly Camera camera; readonly ManualSeaV2Camera orbit;
 readonly ManualSeaV2Environment environment; readonly ManualSeaV2Instances instances;
 readonly ManualSeaV2Ship[] ships; readonly Material water,cloud,fog; readonly Texture2D exploration;
 readonly Transform fogVeil;readonly ManualSeaV2CloudLayer cloudLayer;
 readonly Color[] pixels=new Color[SeaExploration.Width*SeaExploration.Height];
 float minimumZoom=24,maximumZoom=120;
 public float SightRadius=72; float lastSight=-1;
 SeaExploration knowledge; Vector3 lastRevealPosition=new Vector3(float.PositiveInfinity,0,0);
 public RenderTexture Image {get;}
 public ManualSeaV2Environment.SkyMode SkyMode=>environment.mode;
 public void SetSkyMode(ManualSeaV2Environment.SkyMode mode){environment.mode=mode;}
 public float Zoom=>orbit.distance;public float Pitch=>orbit.pitch;public float Yaw=>orbit.yaw;
 public static Vector3 World(Vector2 uv)=>new Vector3((uv.x-.5f)*960,0,(.5f-uv.y)*540);
 public static Vector2 UV(Vector3 p)=>new Vector2(p.x/960+.5f,.5f-p.z/540);
 public ManualSeaV2Runtime(Transform owner){
  if(LiveGui.Camera)LiveGui.Camera.cullingMask&=~(1<<Layer);
  var prefab=Resources.Load<GameObject>("Environment/SoutheastAsiaV2/Runtime");
  if(!prefab)throw new InvalidOperationException("V2 runtime prefab is missing. Run Prepare integrated V2 scene.");
  root=UnityEngine.Object.Instantiate(prefab,owner);root.name="Manual Sailing V2 runtime";
  environment=root.GetComponentInChildren<ManualSeaV2Environment>(true);camera=environment.view;orbit=camera.GetComponent<ManualSeaV2Camera>();
  instances=root.GetComponentInChildren<ManualSeaV2Instances>(true);ships=root.GetComponentsInChildren<ManualSeaV2Ship>(true);
  water=new Material(environment.water);foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true))if(renderer.sharedMaterial==environment.water)renderer.sharedMaterial=water;environment.water=water;
  Image=new RenderTexture(1920,1080,24){name="Manual Sailing V2",antiAliasing=2};Image.Create();camera.targetTexture=Image;camera.aspect=16f/9;camera.farClipPlane=12000;camera.depthTextureMode|=DepthTextureMode.Depth;
  cloud=new Material(Resources.Load<Material>("Materials/SoutheastAsiaV2/Clouds"));fog=new Material(Resources.Load<Shader>("Shaders/ManualSeaV2Exploration"));
  var cloudObject=new GameObject("Cartoon cloud layer");cloudObject.transform.SetParent(root.transform,false);cloudObject.transform.localPosition=Vector3.up*7.5f;cloudLayer=cloudObject.AddComponent<ManualSeaV2CloudLayer>();cloudLayer.Initialize(camera,cloud);
  fogVeil=LayerPlane("Permanent hex exploration overlay",56,fog);fogVeil.SetParent(camera.transform,false);
  exploration=new Texture2D(SeaExploration.Width,SeaExploration.Height,TextureFormat.RGBA32,false,true){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};fog.SetTexture("_Exploration",exploration);
  SetView(0,120);
 }
 Transform LayerPlane(string name,float height,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Quad);go.name=name;go.layer=Layer;go.transform.SetParent(root.transform,false);go.transform.position=new Vector3(0,height,0);go.transform.rotation=Quaternion.Euler(90,0,0);go.transform.localScale=new Vector3(22000,22000,1);UnityEngine.Object.Destroy(go.GetComponent<Collider>());go.GetComponent<Renderer>().sharedMaterial=material;go.GetComponent<Renderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return go.transform;}
 public void ConfigureCamera(float min,float max,float pitch){orbit.pitch=Mathf.Clamp(pitch,20,55);minimumZoom=Mathf.Max(1,min);maximumZoom=Mathf.Max(minimumZoom,max);SetView(Yaw,Zoom);}
 public void SetView(float yaw,float zoom){orbit.yaw=yaw;orbit.distance=Mathf.Clamp(zoom,minimumZoom,maximumZoom);orbit.skyHeightFraction=.15f;orbit.ApplyPose();
  // Camera-facing compositing avoids clipping the exploration veil when zooming below cloud height.
  float depth=camera.nearClipPlane+.1f,height=2*depth*Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
  fogVeil.localPosition=Vector3.forward*depth;fogVeil.localRotation=Quaternion.identity;fogVeil.localScale=new Vector3(height*camera.aspect*1.002f,height*1.002f,1);
 }
 public void Orbit(float drag,float scroll){SetView(Yaw+drag*2.2f,Zoom*Mathf.Exp(-scroll*.12f));}
 public void Center(Vector2 uv){orbit.focus=World(uv);orbit.ApplyPose();}
 public void Follow(Vector2 uv)=>Center(uv);
 public Vector2 Project(Vector2 uv){var p=camera.WorldToViewportPoint(World(uv));return p.z<=0?new Vector2(-10000,-10000):new Vector2(p.x*1920,(1-p.y)*1080);}
 public bool TryUnproject(Vector2 pointer,out Vector2 uv){var ray=camera.ViewportPointToRay(new Vector3(pointer.x/1920,1-pointer.y/1080,0));if(new Plane(Vector3.up,Vector3.zero).Raycast(ray,out float d)){uv=UV(ray.GetPoint(d));return true;}uv=default;return false;}
 public Vector2 Unproject(Vector2 p)=>TryUnproject(p,out var uv)?uv:UV(orbit.focus);
 public void Reveal(SailingState state,Vector2 position){
  var data=state.southeastAsiaExploration??(state.southeastAsiaExploration=new SeaExploration());var p=World(position);
  bool rebound=knowledge!=data;if(!rebound&&(p-lastRevealPosition).sqrMagnitude<.000001f&&lastSight==SightRadius)return;knowledge=data;lastRevealPosition=p;lastSight=SightRadius;
  if(data.Reveal(p.x,p.z,SightRadius)||rebound){for(int i=0;i<pixels.Length;i++){float a=data.tiles[i]==2?0:data.tiles[i]==1?.4f:1;pixels[i]=new Color(a,a,a,1);}exploration.SetPixels(pixels);exploration.Apply(false);}
 }
 public void AdvanceWeather(SailingState state,double wind,double seconds,double speed)=>cloudLayer.AdvanceWeather(state,wind,seconds,speed);
 public void Render(GameState state,float opening){
  cloudLayer.BindWeather(state.sailing);
  var position=new Vector2((float)state.seaX,(float)state.seaY);Reveal(state.sailing,position);Center(position);
  var oldSky=RenderSettings.skybox;var ambient=RenderSettings.ambientLight;var ambientMode=RenderSettings.ambientMode;var sun=RenderSettings.sun;bool oldFog=RenderSettings.fog;
  try{
   root.SetActive(true);environment.Apply();Shader.SetGlobalFloat("_SeaCloudEnabled",1);
   if(Input.GetKeyDown(KeyCode.Alpha1))environment.mode=ManualSeaV2Environment.SkyMode.Sunny;
   if(Input.GetKeyDown(KeyCode.Alpha2))environment.mode=ManualSeaV2Environment.SkyMode.Sunset;
   if(Input.GetKeyDown(KeyCode.Alpha3))environment.mode=ManualSeaV2Environment.SkyMode.Night;
   environment.Apply();Shader.SetGlobalVector("_SeaCloudLight",-environment.sun.transform.forward);
   for(int i=0;i<ships.Length;i++){var f=i==0?null:state.sailing.manualFollowers[i-1];Vector2 uv=i==0?position:new Vector2((float)f.x,(float)f.y);double heading=i==0?state.heading:f.heading;
    ships[i].transform.position=World(uv);var direction=i==0?SeaInstrumentMath.WorldDirection(heading):new Vector2((float)Math.Cos(heading),(float)Math.Sin(heading));ships[i].heading=new Vector3(direction.x,0,-direction.y);
    ships[i].sailTrim=(float)SailingFormulas.SailDegrees(state.trim);ships[i].sailOpening=opening;ships[i].windFill=(float)SailingFormulas.WindFill(state.heading,state.trim,state.wind);ships[i].Refresh();
   }
   var sky=environment.skies[(int)environment.mode];
   fog.SetFloat("_FogSkyMode",sky.GetFloat("_Mode"));fog.SetVector("_FogSunDirection",sky.GetVector("_SunDirection"));
   fog.SetFloat("_FogStarDensity",sky.GetFloat("_StarDensity"));fog.SetFloat("_FogStarBrightness",sky.GetFloat("_StarBrightness"));
   cloudLayer.Render(-environment.sun.transform.forward);instances.Render();camera.Render();
  }finally{Shader.SetGlobalFloat("_SeaCloudEnabled",0);RenderSettings.skybox=oldSky;RenderSettings.ambientLight=ambient;RenderSettings.ambientMode=ambientMode;RenderSettings.sun=sun;RenderSettings.fog=oldFog;}
 }
 public void SetActive(bool value){if(root.activeSelf!=value)root.SetActive(value);}
 public void Capture(string path){var previous=RenderTexture.active;RenderTexture.active=Image;var tex=new Texture2D(Image.width,Image.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,Image.width,Image.height),0,0);tex.Apply();System.IO.File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.Destroy(tex);RenderTexture.active=previous;}
 public void Dispose(){Image.Release();foreach(var obj in new UnityEngine.Object[]{root,Image,water,cloud,fog,exploration})if(obj)UnityEngine.Object.Destroy(obj);}
}
}
