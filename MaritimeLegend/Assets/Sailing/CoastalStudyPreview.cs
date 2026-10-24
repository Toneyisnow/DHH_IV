using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
namespace MaritimeLegend {
public sealed class CoastalStudyPreview:MonoBehaviour {
 [Serializable]public class Placement {public string asset;public float x,y,z,scale,rotation;}
 [Serializable]public class Layout {public int seed;public float shoreMinX,shoreMaxX;public float[] shore;public Placement[] placements;}
 Camera view;CoastalPixelInk pixelInk;GameObject scenery;Material landMaterial,terrainMaterial,mountainMaterial,waterMaterial;Texture2D shoreTexture;Layout layout;
 float yaw=0,pitch=43,zoom=31;Vector3 focus=new Vector3(0,1,5);bool clay;int renderers;Font font;
 void Start(){try{Initialize();}catch(Exception e){Debug.LogException(e);enabled=false;if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coastalStudySmoke")>=0)Application.Quit(1);}}
 void Initialize(){
  Application.targetFrameRate=60;QualitySettings.antiAliasing=4;QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.VeryHigh;QualitySettings.shadowDistance=180;QualitySettings.shadowCascades=4;
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.60f,.65f,.72f);RenderSettings.ambientEquatorColor=new Color(.45f,.46f,.40f);RenderSettings.ambientGroundColor=new Color(.25f,.25f,.22f);RenderSettings.fog=false;
  scenery=new GameObject("Configured coastal landscape");
  layout=JsonUtility.FromJson<Layout>(Resources.Load<TextAsset>("Environment/CoastalStudy/layout").text);
  landMaterial=new Material(Resources.Load<Shader>("Shaders/CoastalStudyGround"));
  mountainMaterial=new Material(Resources.Load<Shader>("Shaders/CoastalStudyMountain"));
  var grass=Resources.Load<Texture2D>("Textures/Environment/CoastalStudy/GrassPainted");
  if(!grass)throw new InvalidOperationException("Missing painted grass texture");
  terrainMaterial=new Material(landMaterial);terrainMaterial.SetTexture("_GrassTex",grass);terrainMaterial.SetFloat("_GroundDetail",1);
  waterMaterial=new Material(Resources.Load<Shader>("Shaders/CoastalStudyWater"));
  Place("BayGround",Vector3.zero,1,0);
  foreach(var p in layout.placements)Place(p.asset,new Vector3(p.x,p.y,p.z),p.scale,p.rotation);
  shoreTexture=new Texture2D(layout.shore.Length,1,TextureFormat.RFloat,false,true){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
  var pixels=new Color[layout.shore.Length];for(int i=0;i<pixels.Length;i++)pixels[i]=new Color(layout.shore[i],0,0,1);shoreTexture.SetPixels(pixels);shoreTexture.Apply(false,true);waterMaterial.SetTexture("_Shore",shoreTexture);mountainMaterial.SetTexture("_Shore",shoreTexture);
  var water=GameObject.CreatePrimitive(PrimitiveType.Plane);water.name="Independent sea and river surface";water.transform.position=new Vector3(0,-.025f,0);water.transform.localScale=Vector3.one*60;water.GetComponent<Renderer>().sharedMaterial=waterMaterial;Destroy(water.GetComponent<Collider>());
  var sun=new GameObject("Warm afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(50,155,0);sun.color=new Color(1,.92f,.78f);sun.intensity=1.25f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.85f;sun.shadowBias=.035f;sun.shadowNormalBias=.15f;
  view=new GameObject("Coastal study camera").AddComponent<Camera>();view.orthographic=true;view.nearClipPlane=.3f;view.farClipPlane=450;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.28f,.43f,.50f);view.allowMSAA=true;view.allowHDR=false;SetCamera();
  pixelInk=view.gameObject.AddComponent<CoastalPixelInk>();SetCamera();
  font=Font.CreateDynamicFontFromOSFont("Microsoft YaHei",20);
  Validate();if(Array.IndexOf(Environment.GetCommandLineArgs(),"-coastalStudySmoke")>=0)StartCoroutine(Smoke());
 }
 void Place(string asset,Vector3 position,float scale,float rotation){
  var prefab=Resources.Load<GameObject>("Prefabs/Environment/CoastalStudy/"+asset);
  if(!prefab)throw new InvalidOperationException("Missing study prefab "+asset);
  var o=Instantiate(prefab,position,Quaternion.Euler(0,rotation,0),scenery.transform);o.transform.localScale=Vector3.one*scale;
  foreach(var r in o.GetComponentsInChildren<MeshRenderer>()){
   var materials=new Material[r.sharedMaterials.Length];for(int i=0;i<materials.Length;i++)materials[i]=asset=="BayGround"?terrainMaterial:(asset.StartsWith("RockPeak")||asset.StartsWith("EarthHill"))?mountainMaterial:landMaterial;r.sharedMaterials=materials;r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;renderers++;
  }
 }
 void SetCamera(){view.orthographicSize=zoom;view.transform.rotation=Quaternion.Euler(pitch,yaw,0);var position=focus-view.transform.forward*130;
  view.transform.position=position;
 }
 void Update(){if(!view)return;
  if(Input.GetKeyDown(KeyCode.Escape))Application.Quit();
  if(Input.GetKeyDown(KeyCode.G))ToggleClay();
  if(Input.GetKeyDown(KeyCode.P))ToggleInk();
  if(Input.GetKeyDown(KeyCode.R)){yaw=0;pitch=43;zoom=31;focus=new Vector3(0,1,5);}
  if(Input.GetMouseButton(1)){yaw+=Input.GetAxis("Mouse X")*2;pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*2,25,75);}
  zoom=Mathf.Clamp(zoom-Input.mouseScrollDelta.y*2,15,44);
  var move=new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical"));focus+=Quaternion.Euler(0,yaw,0)*move*Time.unscaledDeltaTime*18;focus.x=Mathf.Clamp(focus.x,-35,35);focus.z=Mathf.Clamp(focus.z,-15,30);SetCamera();
 }
 void ToggleClay(){clay=!clay;landMaterial.SetFloat("_Clay",clay?1:0);terrainMaterial.SetFloat("_Clay",clay?1:0);waterMaterial.SetFloat("_Clay",clay?1:0);mountainMaterial.SetFloat("_Clay",clay?1:0);}
 void ToggleInk(){pixelInk.InkEnabled=!pixelInk.InkEnabled;}
 void Validate(){
  if(layout.placements.Length<30)throw new Exception("Insufficient configured assets");
  foreach(string shader in new[]{"CoastalStudyGround","CoastalStudyWater","CoastalPixelInk","CoastalStudyMountain","CoastalOutlineGeometry"})if(!Resources.Load<Shader>("Shaders/"+shader).isSupported)throw new Exception("Unsupported style shader "+shader);
  foreach(var f in scenery.GetComponentsInChildren<MeshFilter>())if(f.sharedMesh.colors32.Length!=f.sharedMesh.vertexCount)throw new Exception("Missing vertex paint "+f.name);
  foreach(var f in scenery.GetComponentsInChildren<MeshFilter>())if(f.name.StartsWith("RockPeak")||f.name.StartsWith("EarthHill")){
   var mesh=f.sharedMesh;var vertices=mesh.vertices;var colors=mesh.colors32;float top=0,bottom=0;int nt=0,nb=0;
   for(int v=0;v<vertices.Length;v++){if(vertices[v].y>mesh.bounds.max.y*.8f){top+=colors[v].r/255f;nt++;}if(vertices[v].y<.3f){bottom+=colors[v].r/255f;nb++;}}
   if(nt==0||nb==0||top/nt-bottom/nb<.15f)throw new Exception("Mountain crest/foot gradient missing: "+f.name);
  }
  var groundFilter=scenery.transform.GetChild(0).GetComponentInChildren<MeshFilter>();var ground=groundFilter.sharedMesh;
  if(ground.bounds.size.x<190||ground.bounds.size.y>3||ground.bounds.size.z<100)throw new Exception("FBX axes or ground scale incorrect: "+ground.bounds);
  var collider=groundFilter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=ground;groundFilter.gameObject.layer=30;Physics.SyncTransforms();
  foreach(var p in layout.placements){RaycastHit hit;if(!Physics.Raycast(new Vector3(p.x,100,p.z),Vector3.down,out hit,200,1<<30)||Mathf.Abs(hit.point.y-p.y)>.25f)throw new Exception("Asset is not seated on configured land: "+p.asset+" at "+p.x+","+p.z);}
  for(float x=-44;x<=44;x+=2){
   if(x>=0&&x<=8)continue; // River mouth has a deliberately recessed bed.
   float t=(x-layout.shoreMinX)/(layout.shoreMaxX-layout.shoreMinX)*(layout.shore.Length-1);int i=Mathf.FloorToInt(t);float z=Mathf.Lerp(layout.shore[i],layout.shore[i+1],t-i);RaycastHit hit;
   if(!Physics.Raycast(new Vector3(x,100,z+.1f),Vector3.down,out hit,200,1<<30)||Mathf.Abs(hit.point.y)>.08f)throw new Exception("Shore not at sea level "+x);
   if(Physics.Raycast(new Vector3(x,100,z-.15f),Vector3.down,200,1<<30))throw new Exception("Land crosses the configured sea boundary "+x);
   if(Mathf.Abs(x)>12&&Physics.Raycast(new Vector3(x,100,z+3),Vector3.down,out hit,200,1<<30)&&hit.point.y>.16f)throw new Exception("Beach apron is too steep "+x);
  }
  Destroy(collider);
  Debug.Log("COASTAL_STUDY_ASSETS_PASS: "+layout.placements.Length+" placed assets, "+renderers+" mesh renderers, vertex colors and axes verified");
 }
 void Capture(string file){
  var directory=Path.GetFullPath(Path.Combine(Application.dataPath,"..","Evidence"));Directory.CreateDirectory(directory);
  var rt=RenderTexture.GetTemporary(1920,1080,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Default,4);var previous=RenderTexture.active;var target=view.targetTexture;
  view.targetTexture=rt;SetCamera();view.Render();RenderTexture.active=rt;var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Path.Combine(directory,file+".png"),image.EncodeToPNG());Destroy(image);view.targetTexture=target;SetCamera();RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);
 }
 IEnumerator Smoke(){yield return new WaitForSecondsRealtime(2);Capture("coastal-study");ToggleInk();yield return null;Capture("coastal-unfiltered");ToggleInk();ToggleClay();yield return null;Capture("coastal-geometry");ToggleClay();focus=new Vector3(-10,1,2);zoom=20;SetCamera();yield return null;Capture("coastal-close");yaw=25;SetCamera();yield return null;Capture("coastal-rotated");if(pixelInk.RenderedFrames<3){Debug.LogError("HD ink pass did not render");Application.Quit(1);yield break;}Debug.Log("COASTAL_STUDY_PASS: HD antialiased ink, flat beach, textured grass, palette gradient, overview, raw comparison, geometry, close and rotated inspection; no depth of field");Application.Quit(0);}
 void OnGUI(){if(!font)return;var style=new GUIStyle(GUI.skin.label){font=font,fontSize=20,normal={textColor=new Color(.95f,.92f,.80f)}};GUI.Box(new Rect(20,20,740,82),GUIContent.none);GUI.Label(new Rect(35,28,720,28),"海湾样景  ·  "+(pixelInk.InkEnabled?"高清描边":"无描边对照"),style);style.fontSize=15;GUI.Label(new Rect(35,61,720,27),"右键旋转   滚轮缩放   WASD 平移   P 高清描边   G 灰模   R 复位   Esc 退出",style);}
 void OnDestroy(){if(landMaterial)Destroy(landMaterial);if(terrainMaterial)Destroy(terrainMaterial);if(mountainMaterial)Destroy(mountainMaterial);if(waterMaterial)Destroy(waterMaterial);if(shoreTexture)Destroy(shoreTexture);if(font)Destroy(font);}
}
}
