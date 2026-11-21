using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace MaritimeLegend.Editor {
public static class RegionalCityElementsBuilder {
 public static readonly string[] Regions={"Europe","Nordic","Mediterranean","Africa","Asia","SouthAmerica"};
 static readonly string[] Kinds={"Road","House","Tree","Market","Eave","Bird"};
 const string Sprites="Assets/Resources/Sprites/Cities/", Prefabs="Assets/Resources/Prefabs/Cities/", Models="Assets/Resources/Models/Cities/", Materials="Assets/Resources/Materials/Cities/";
 [Serializable] class Catalog {public string version="1.0",animation="Continuous mesh wind and local bird flight; not frame-by-frame sprite animation";public List<Entry> elements=new List<Entry>();}
 [Serializable] class Entry {public string region,element,prefab,sprite;public int x,y,width,height;public float period,amplitude;}
 static string Review {
  get {var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-regionalReview");return at>=0?Path.GetFullPath(args[at+1]):Path.GetFullPath("../ArtSource/RegionalCityElements/Review");}
 }
 [MenuItem("Maritime Legend/City Art/Build Regional Element Library")]
 public static void BuildFromMenu(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())Build();}
 public static void Build(){
  var shader=Shader.Find("Maritime/RegionalCityElement");if(shader==null)throw new Exception("Regional shader missing");
  var catalog=new Catalog();
  for(int region=0;region<Regions.Length;region++){
   string id=Regions[region];foreach(string root in new[]{Sprites,Prefabs,Models,Materials})Directory.CreateDirectory(root+id);
   AssetDatabase.Refresh();
   string atlasPath=Sprites+id+"/Elements.png";ConfigureTexture(atlasPath,true);
   var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
   if(atlas.width!=1536||atlas.height!=1024)throw new Exception(id+": unexpected atlas dimensions");
   var pixels=atlas.GetPixels32();
   for(int k=0;k<Kinds.Length;k++){
    RectInt rect=Bounds(pixels,atlas.width,k);
    if(k==0){rect.xMin+=5;rect.yMin+=5;rect.xMax-=5;rect.yMax-=5;}
    string path=Sprites+id+"/"+Kinds[k]+".png";
    var cut=new Texture2D(rect.width,rect.height,TextureFormat.RGBA32,false);
    cut.SetPixels(atlas.GetPixels(rect.x,rect.y,rect.width,rect.height));cut.Apply();
    File.WriteAllBytes(path,cut.EncodeToPNG());UnityEngine.Object.DestroyImmediate(cut);
    AssetDatabase.ImportAsset(path);ConfigureTexture(path,false);
    var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    float height=k==1?6:k==2?6.3f:k==3?3.2f:k==4?.9f:k==5?.85f:4;
    float width=k==0?4:height*rect.width/rect.height;
    if(k==5){width=1.8f;height=width*rect.height/rect.width;}
    var material=new Material(shader){name=id+" "+Kinds[k],mainTexture=texture};
    float amplitude=k==2?.075f:k==3?.035f:k==4?.07f:k==5?.17f:0;
    float period=k==2?8:k==3?6:k==4?5:7;
    material.SetFloat("_Amplitude",amplitude);material.SetFloat("_Period",period);material.SetFloat("_Phase",region*.61f+k*.37f);material.SetFloat("_Bird",k==5?1:0);
    material=PutAsset(material,Materials+id+"/"+Kinds[k]+".mat");
    var mesh=PutAsset(Grid(width,height,k),Models+id+"/"+Kinds[k]+".asset");
    var root=new GameObject(id+"_"+Kinds[k]);
    var visual=new GameObject("Visual",typeof(MeshFilter),typeof(MeshRenderer));visual.transform.SetParent(root.transform,false);
    visual.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=visual.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
    renderer.shadowCastingMode=k==0?ShadowCastingMode.Off:ShadowCastingMode.TwoSided;renderer.receiveShadows=true;
    if(k==0){visual.transform.localRotation=Quaternion.Euler(90,0,0);visual.transform.localPosition=new Vector3(0,.015f,-2);var floor=root.AddComponent<BoxCollider>();floor.center=new Vector3(0,-.04f,0);floor.size=new Vector3(4,.08f,4);AddDust(root,region);}
    else visual.transform.localRotation=Quaternion.Euler(32,0,0);
    if(k==5){var motion=root.AddComponent<RegionalCityMotion>();motion.phase=region*.7f;motion.period=24+region;}
    PrefabUtility.SaveAsPrefabAsset(root,Prefabs+id+"/"+Kinds[k]+".prefab");UnityEngine.Object.DestroyImmediate(root);
    catalog.elements.Add(new Entry{region=id,element=Kinds[k],sprite="Sprites/Cities/"+id+"/"+Kinds[k],prefab="Prefabs/Cities/"+id+"/"+Kinds[k],x=rect.x,y=rect.y,width=rect.width,height=rect.height,period=period,amplitude=amplitude});
   }
   // Houses remain rigid; a separately authored eave supplies their wind movement.
   var house=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+id+"/House.prefab"));
   var eave=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+id+"/Eave.prefab"),house.transform);
   eave.name="Wind eave (separate layer)";eave.transform.localPosition=new Vector3(0,2.55f,1.48f);eave.transform.localScale=Vector3.one*.65f;
   PrefabUtility.SaveAsPrefabAsset(house,Prefabs+id+"/House.prefab");UnityEngine.Object.DestroyImmediate(house);
   var sample=CreateSample(id);PrefabUtility.SaveAsPrefabAsset(sample,Prefabs+id+"/Sample.prefab");UnityEngine.Object.DestroyImmediate(sample);
   ConfigureTexture(atlasPath,false);
  }
  Directory.CreateDirectory("Assets/Resources/Data/Presentation/Cities");File.WriteAllText("Assets/Resources/Data/Presentation/Cities/RegionalElements.json",JsonUtility.ToJson(catalog,true));
  AssetDatabase.Refresh();AssetDatabase.SaveAssets();
  CaptureReview();CreatePreviewScene();
  Debug.Log("REGIONAL_CITY_LIBRARY_PASS: 6 styles, 36 element prefabs, 6 sample prefabs; atlas bounds and rendered motion checked.");
 }
 internal static T PutAsset<T>(T asset,string path) where T:UnityEngine.Object {
  var existing=AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
  if(existing==null)AssetDatabase.CreateAsset(asset,path);
  else {EditorUtility.CopySerialized(asset,existing);UnityEngine.Object.DestroyImmediate(asset);}
  return AssetDatabase.LoadAssetAtPath<T>(path);
 }
 internal static void ConfigureTexture(string path,bool readable){
  var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Default;imp.alphaSource=TextureImporterAlphaSource.FromInput;imp.alphaIsTransparency=true;imp.isReadable=readable;imp.mipmapEnabled=false;imp.filterMode=FilterMode.Point;imp.wrapMode=TextureWrapMode.Clamp;imp.textureCompression=TextureImporterCompression.Uncompressed;imp.npotScale=TextureImporterNPOTScale.None;imp.maxTextureSize=2048;imp.SaveAndReimport();
 }
 static RectInt Bounds(Color32[] pixels,int width,int cell){
  int left=cell%3*512,bottom=(1-cell/3)*512,minX=left+512,minY=bottom+512,maxX=left,maxY=bottom,count=0;
  for(int y=bottom;y<bottom+512;y++)for(int x=left;x<left+512;x++)if(pixels[y*width+x].a>=179){minX=Math.Min(minX,x);minY=Math.Min(minY,y);maxX=Math.Max(maxX,x);maxY=Math.Max(maxY,y);count++;}
  if(count<1000)throw new Exception("Atlas element has no usable alpha: "+cell);
  if(minX==left||maxX==left+511||minY==bottom||maxY==bottom+511)throw new Exception("Atlas element crosses a cell boundary: "+cell);
  return new RectInt(minX,minY,maxX-minX+1,maxY-minY+1);
 }
 internal static Mesh Grid(float width,float height,int kind){
  int n=kind==0||kind==1?1:32;var vertices=new Vector3[(n+1)*(n+1)];var uv=new Vector2[vertices.Length];var colors=new Color[vertices.Length];var triangles=new int[n*n*6];
  for(int y=0;y<=n;y++)for(int x=0;x<=n;x++){
   int i=y*(n+1)+x;float u=x/(float)n,v=y/(float)n;vertices[i]=new Vector3((u-.5f)*width,v*height,0);uv[i]=new Vector2(u,v);
   float weight=kind==2?Mathf.Pow(Mathf.SmoothStep(0,1,Mathf.InverseLerp(.24f,1,v)),1.2f):kind==4?Mathf.Pow(1-v,1.7f):kind==3?Mathf.Sin(u*Mathf.PI)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.57f,.73f,v))*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.88f,1,v))):kind==5?Mathf.SmoothStep(0,1,Mathf.Abs(u-.6f)*2)*Mathf.SmoothStep(0,1,Mathf.InverseLerp(.35f,.72f,v)):0;
   colors[i]=new Color(weight,0,0,1);
   if(x<n&&y<n){int t=(y*n+x)*6;triangles[t]=i;triangles[t+1]=i+n+1;triangles[t+2]=i+1;triangles[t+3]=i+1;triangles[t+4]=i+n+1;triangles[t+5]=i+n+2;}
  }
  var mesh=new Mesh{name=Kinds[kind]+" anchored wind grid",vertices=vertices,uv=uv,colors=colors,triangles=triangles};mesh.RecalculateNormals();mesh.RecalculateBounds();var b=mesh.bounds;b.Expand(.5f);mesh.bounds=b;return mesh;
 }
 internal static void AddDust(GameObject root,int region){
  string path=Materials+Regions[region]+"/Dust.mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
  if(mat==null){mat=new Material(Shader.Find("Maritime/CityAtmosphere"));AssetDatabase.CreateAsset(mat,path);}
  var child=new GameObject("Slow ground dust",typeof(ParticleSystem));child.transform.SetParent(root.transform,false);child.transform.localPosition=Vector3.up*.15f;
  var ps=child.GetComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=ps.main;main.duration=10;main.loop=true;main.startLifetime=6;main.startSpeed=.055f;main.startSize=.025f;main.startColor=new Color(.80f,.72f,.55f,.22f);main.maxParticles=16;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.prewarm=true;
  var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(3.8f,.18f,3.8f);
  var emission=ps.emission;emission.rateOverTime=1.4f;
  var velocity=ps.velocityOverLifetime;velocity.enabled=true;velocity.x=.035f;velocity.y=.015f;velocity.z=.01f;
  var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.7f,.3f),new GradientAlphaKey(0,1)});fade.color=gradient;
  ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=mat;ps.useAutoRandomSeed=false;ps.randomSeed=(uint)(840+region);
 }
 static GameObject Place(string id,string kind,Transform parent,Vector3 p){var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+id+"/"+kind+".prefab"),parent);go.transform.localPosition=p;return go;}
 static GameObject CreateSample(string id){
  var root=new GameObject(id+" Regional City Sample");
  for(int z=-1;z<=1;z++)for(int x=-1;x<=1;x++)Place(id,"Road",root.transform,new Vector3(x*4,0,z*4));
  Place(id,"House",root.transform,new Vector3(-2.5f,0,1));Place(id,"Tree",root.transform,new Vector3(3.4f,0,2.5f));Place(id,"Market",root.transform,new Vector3(2.3f,0,-2));Place(id,"Bird",root.transform,new Vector3(.5f,7,2));return root;
 }
 internal static Camera SetupStage(){
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.3f,.35f,.41f);
  var sun=new GameObject("Warm regional daylight").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.94f,.82f);sun.intensity=1.05f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.9f;sun.shadowBias=.025f;sun.shadowNormalBias=.15f;sun.transform.rotation=Quaternion.Euler(48,-48,0);
  QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=60;QualitySettings.shadowResolution=ShadowResolution.High;
  var camera=new GameObject("Regional Preview Camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.rotation=Quaternion.Euler(32,0,0);camera.transform.position=new Vector3(0,2.8f,0)-camera.transform.forward*24;camera.orthographic=true;camera.orthographicSize=7.2f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.55f,.66f,.70f);camera.farClipPlane=80;camera.nearClipPlane=.1f;camera.renderingPath=RenderingPath.Forward;return camera;
 }
 static void CaptureReview(){
  Directory.CreateDirectory(Review);var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var camera=SetupStage();
  var rt=new RenderTexture(1024,768,24,RenderTextureFormat.ARGB32);camera.targetTexture=rt;
  Shader.SetGlobalFloat("_RegionalCityPreview",1);
  try {
   foreach(string id in Regions){
    var sample=CreateSample(id);byte[] first=null;
    for(int frame=0;frame<96;frame++){
     float time=frame/12f;Shader.SetGlobalFloat("_RegionalCityTime",time);
     foreach(var flight in sample.GetComponentsInChildren<RegionalCityMotion>())flight.Sample(time);
     foreach(var ps in sample.GetComponentsInChildren<ParticleSystem>())ps.Simulate(time+3,true,true,true);
     camera.Render();var prior=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();
     var bytes=tex.EncodeToPNG();File.WriteAllBytes(Path.Combine(Review,id+"-"+frame.ToString("00")+".png"),bytes);
     if(frame==0)first=bytes;else if(frame==95&&Convert.ToBase64String(first)==Convert.ToBase64String(bytes))throw new Exception("No rendered motion for "+id);
     UnityEngine.Object.DestroyImmediate(tex);RenderTexture.active=prior;
    }
    UnityEngine.Object.DestroyImmediate(sample);
   }
   VerifyElementMotion(camera,rt);
   foreach(var message in ShaderUtil.GetShaderMessages(Shader.Find("Maritime/RegionalCityElement")))
    if(message.severity.ToString()=="Error")throw new Exception(message.message);
  }finally{Shader.SetGlobalFloat("_RegionalCityPreview",0);camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
  File.WriteAllText(Path.Combine(Review,"PASS.txt"),"Six atlas alpha bounds validated; 36 elements and six samples authored; 96 frames at 12 fps captured per region; isolated motion verified for every element; regional shader has no compilation errors. Review images are Unity renders, not concept mockups.");
 }
 static Color32[] ReadFrame(Camera camera,RenderTexture rt){
  camera.Render();var prior=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();var result=texture.GetPixels32();UnityEngine.Object.DestroyImmediate(texture);RenderTexture.active=prior;return result;
 }
 static void VerifyElementMotion(Camera camera,RenderTexture rt){
  var results=new System.Text.StringBuilder("region,element,changedPixels\n");
  foreach(string id in Regions)foreach(string kind in Kinds){
   var element=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs+id+"/"+kind+".prefab"));
   // Verify wing deformation independently of translational flight.
   foreach(var flight in element.GetComponentsInChildren<RegionalCityMotion>()){flight.radius=0;flight.bob=0;}
   Color32[] before=null;
   for(int frame=0;frame<2;frame++){
    float t=frame==0?.3f:2.1f;Shader.SetGlobalFloat("_RegionalCityTime",t);
    foreach(var ps in element.GetComponentsInChildren<ParticleSystem>())ps.Simulate(t+3,true,true,true);
    var pixels=ReadFrame(camera,rt);
    if(frame==0)before=pixels;
    else {int changed=0;for(int i=0;i<pixels.Length;i++)if(Math.Abs(pixels[i].r-before[i].r)+Math.Abs(pixels[i].g-before[i].g)+Math.Abs(pixels[i].b-before[i].b)>8)changed++;
     results.AppendLine(id+","+kind+","+changed);if(changed<3)throw new Exception("Element motion missing: "+id+"/"+kind+" ("+changed+" pixels)");}
   }
   UnityEngine.Object.DestroyImmediate(element);
  }
  File.WriteAllText(Path.Combine(Review,"motion-checks.csv"),results.ToString());
 }
 static void CreatePreviewScene(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SetupStage();
  for(int i=0;i<Regions.Length;i++){var sample=CreateSample(Regions[i]);sample.SetActive(i==0);}
  Directory.CreateDirectory("Assets/Scenes/ArtPreview");EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/ArtPreview/RegionalCityElements.unity");AssetDatabase.SaveAssets();
 }
}
}
