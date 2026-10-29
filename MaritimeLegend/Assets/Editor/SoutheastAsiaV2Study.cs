using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
namespace MaritimeLegend.Editor {
public static class SoutheastAsiaV2Study {
 const string ScenePath="Assets/Scenes/SoutheastAsiaSailingV2.unity";
 const string Feature="SoutheastAsiaV2";
 static int w,h;static float width,height;static float[] heights,coast,forest;
 static string Review {get {var d=new DirectoryInfo(Application.dataPath);while(d!=null&&!File.Exists(Path.Combine(d.FullName,"AGENTS.md")))d=d.Parent;var p=Path.Combine(d.FullName,"ArtSource/ManualSailingV2/SoutheastAsia/Review");Directory.CreateDirectory(p);return p;}}
 static float Sample(float[] data,float x,float z){float px=Mathf.Clamp((x/width+.5f)*(w-1),0,w-1.001f),py=Mathf.Clamp((.5f-z/height)*(h-1),0,h-1.001f);int ix=(int)px,iy=(int)py;return Mathf.Lerp(Mathf.Lerp(data[iy*w+ix],data[iy*w+ix+1],px-ix),Mathf.Lerp(data[(iy+1)*w+ix],data[(iy+1)*w+ix+1],px-ix),py-iy);}
 static T Save<T>(T value,string category,string name)where T:UnityEngine.Object {
  string folder="Assets/Resources/"+category+"/"+Feature;if(!Directory.Exists(folder)){Directory.CreateDirectory(folder);AssetDatabase.Refresh();}string path=folder+"/"+name;
  var existing=AssetDatabase.LoadAssetAtPath<T>(path);if(existing){
   if(existing is Mesh mesh&&value is Mesh source){mesh.Clear();mesh.indexFormat=source.indexFormat;mesh.vertices=source.vertices;mesh.normals=source.normals;mesh.colors=source.colors;mesh.triangles=source.triangles;mesh.RecalculateBounds();}
   else if(existing is Texture2D tex&&value is Texture2D src){tex.Reinitialize(src.width,src.height,src.format,false);tex.SetPixels(src.GetPixels());tex.Apply();}
   else EditorUtility.CopySerialized(value,existing);EditorUtility.SetDirty(existing);UnityEngine.Object.DestroyImmediate(value);return existing;
  }AssetDatabase.CreateAsset(value,path);return value;
 }
 static Color Paint(float x,float z,float y){
  float inland=Sample(coast,x,z),trees=Sample(forest,x,z);float slope=new Vector2(Sample(heights,x+1,z)-Sample(heights,x-1,z),Sample(heights,x,z+1)-Sample(heights,x,z-1)).magnitude*.5f;
  Color grass=Color.Lerp(new Color(.57f,.59f,.25f),new Color(.26f,.39f,.15f),trees);
  Color color=Color.Lerp(new Color(.86f,.74f,.47f),grass,Mathf.SmoothStep(0,1,(inland-.6f)/2.4f));
  float rock=Mathf.Max(Mathf.SmoothStep(0,1,(slope-.3f)/.9f),Mathf.SmoothStep(0,1,(y-4)/6));
  color=Color.Lerp(color,new Color(.68f,.59f,.37f),rock);
  float dark=Mathf.SmoothStep(0,1,(y-15)/9)*Mathf.SmoothStep(0,1,(inland-18)/18);
  color=Color.Lerp(color,new Color(.48f,.36f,.18f),dark);
  return color*Mathf.Lerp(.94f,1.06f,Mathf.PerlinNoise(x*.9f+400,z*.9f+400));
 }
 static Mesh Chunk(int x0,int y0,int step){
  int xe=Math.Min(w-1,x0+64),ye=Math.Min(h-1,y0+64);var vertices=new List<Vector3>();var colors=new List<Color>();var normals=new List<Vector3>();var triangles=new List<int>();var lookup=new Dictionary<int,int>();
  Func<int,int,int> vertex=(ix,iy)=>{int key=iy*w+ix;if(lookup.TryGetValue(key,out int index))return index;index=vertices.Count;float x=(float)ix/(w-1)*width-width*.5f,z=height*.5f-(float)iy/(h-1)*height,y=heights[key];vertices.Add(new Vector3(x,y,z));colors.Add(Paint(x,z,y));normals.Add(new Vector3(Sample(heights,x-1,z)-Sample(heights,x+1,z),2,Sample(heights,x,z-1)-Sample(heights,x,z+1)).normalized);lookup[key]=index;return index;};
  for(int iy=y0;iy<ye;iy+=step)for(int ix=x0;ix<xe;ix+=step){int nx=Math.Min(xe,ix+step),ny=Math.Min(ye,iy+step);if(Mathf.Max(Mathf.Max(heights[iy*w+ix],heights[iy*w+nx]),Mathf.Max(heights[ny*w+ix],heights[ny*w+nx]))<-.8f)continue;int a=vertex(ix,iy),b=vertex(nx,iy),c=vertex(ix,ny),d=vertex(nx,ny);triangles.Add(a);triangles.Add(b);triangles.Add(c);triangles.Add(b);triangles.Add(d);triangles.Add(c);}
  if(triangles.Count==0)return null;var mesh=new Mesh{name=$"SEA terrain {x0},{y0} step {step}"};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetNormals(normals);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;
 }
 [MenuItem("Maritime Legend/Manual Sailing V2/Play Southeast Asia")]
 public static void Play(){if(EditorApplication.isPlayingOrWillChangePlaymode)return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene(ScenePath);EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);EditorApplication.EnterPlaymode();}
 [MenuItem("Maritime Legend/Manual Sailing V2/Create Southeast Asia")]
 public static void Create(){if(EditorApplication.isPlayingOrWillChangePlaymode)return;if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())Generate();}
 public static void Generate(){
  // Refresh shared prop LODs, corrected mountain colors and sky materials; preserve the original test scene as a separate asset.
  ManualSeaV2Study.Generate();
  var env=UnityEngine.Object.FindFirstObjectByType<ManualSeaV2Environment>();var root=env.transform;var camera=env.view;var orbit=camera.GetComponent<ManualSeaV2Camera>();var instances=env.GetComponent<ManualSeaV2Instances>();
  foreach(var group in UnityEngine.Object.FindObjectsByType<LODGroup>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(group.gameObject);
  using(var reader=new BinaryReader(File.OpenRead("Assets/Resources/Models/Environment/"+Feature+"/terrain.bytes"))){w=reader.ReadInt32();h=reader.ReadInt32();width=reader.ReadSingle();height=reader.ReadSingle();heights=new float[w*h];coast=new float[w*h];forest=new float[w*h];foreach(var array in new[]{heights,coast,forest})for(int i=0;i<array.Length;i++)array[i]=reader.ReadSingle();}
  int chunks=0,nearTriangles=0,farTriangles=0;
  for(int iy=0;iy<h-1;iy+=64)for(int ix=0;ix<w-1;ix+=64){var lods=new List<LOD>();var group=new GameObject($"SEA coast chunk {ix},{iy}");group.transform.SetParent(root,false);
   for(int lod=0;lod<2;lod++){var mesh=Chunk(ix,iy,lod==0?2:4);if(!mesh)continue;if(lod==0)nearTriangles+=mesh.triangles.Length/3;else farTriangles+=mesh.triangles.Length/3;mesh=Save(mesh,"Models/Environment",$"Terrain-{ix}-{iy}-L{lod}.asset");var obj=new GameObject("LOD "+lod,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(group.transform,false);obj.GetComponent<MeshFilter>().sharedMesh=mesh;obj.GetComponent<MeshRenderer>().sharedMaterial=instances.material;lods.Add(new LOD(lod==0?.12f:.004f,new[]{obj.GetComponent<Renderer>()}));}
   if(lods.Count==0){UnityEngine.Object.DestroyImmediate(group);continue;}var lodGroup=group.AddComponent<LODGroup>();lodGroup.SetLODs(lods.ToArray());lodGroup.RecalculateBounds();chunks++;
  }
  var depth=new Texture2D(w,h,TextureFormat.RFloat,false,true){wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear,name="SEA bathymetry"};var pixels=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)pixels[y*w+x]=new Color((heights[(h-1-y)*w+x]+32)/64,0,0,1);depth.SetPixels(pixels);depth.Apply();depth=Save(depth,"Textures","Bathymetry.asset");
  var oldWater=env.water;var water=new Material(oldWater){name="Southeast Asia azure sea"};water.SetTexture("_DepthMap",depth);water.SetVector("_MapBounds",new Vector4(-width/2,-height/2,width,height));water=Save(water,"Materials","Water.mat");foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())if(renderer.sharedMaterial==oldWater)renderer.sharedMaterial=water;env.water=water;
  var buckets=new Dictionary<Vector3Int,List<Matrix4x4>>();var random=new System.Random(261028);int props=0;
  for(float z=-height/2+2;z<height/2-2;z+=2.8f)for(float x=-width/2+2;x<width/2-2;x+=2.8f){float px=x+(float)random.NextDouble()*1.8f,pz=z+(float)random.NextDouble()*1.8f,inside=Sample(coast,px,pz),y=Sample(heights,px,pz),tree=Sample(forest,px,pz);if(inside<1||y>12)continue;bool rock=inside<3.5f;if(random.NextDouble()>(rock?.27:.15+tree*.75))continue;
   int variant=rock?2:random.Next(2);float size=rock?.35f+(float)random.NextDouble()*.6f:.60f+(float)random.NextDouble()*.65f;var key=new Vector3Int(Mathf.FloorToInt(px/40),Mathf.FloorToInt(pz/40),variant);if(!buckets.ContainsKey(key))buckets[key]=new List<Matrix4x4>();buckets[key].Add(Matrix4x4.TRS(new Vector3(px,y-.05f,pz),Quaternion.Euler(0,(float)random.NextDouble()*360,0),Vector3.one*size));props++;
  }
  var cells=new List<ManualSeaV2Instances.Cell>();foreach(var pair in buckets)for(int offset=0;offset<pair.Value.Count;offset+=1023){int count=Math.Min(1023,pair.Value.Count-offset);cells.Add(new ManualSeaV2Instances.Cell{variant=pair.Key.z,bounds=new Bounds(new Vector3(pair.Key.x*40+20,12,pair.Key.y*40+20),new Vector3(46,32,46)),transforms=pair.Value.GetRange(offset,count).ToArray()});}instances.cells=cells.ToArray();instances.lodDistance=150;
  orbit.focusBounds=new Vector4(-width/2+8,-height/2+8,width/2-8,height/2-8);orbit.homeFocus=new Vector3(-95,0,70);orbit.focus=orbit.homeFocus;orbit.homeDistance=120;orbit.distance=120;orbit.maximumDistance=120;orbit.ApplyPose();env.mapTitle="Southeast Asia V2 | 960 x 540";env.Apply();
  var benchmark=env.GetComponent<ManualSeaV2Benchmark>();benchmark.sampleFocuses=new[]{new Vector3(-95,0,70),new Vector3(-270,0,-15),new Vector3(210,0,130)};
  EditorSceneManager.SaveScene(camera.gameObject.scene,ScenePath);AssetDatabase.SaveAssets();
  CreateFleet(env,orbit);
  Capture(camera,env,instances,"south-china-sea");orbit.focus=new Vector3(-275,0,-15);orbit.distance=110;orbit.yaw=0;orbit.ApplyPose();Capture(camera,env,instances,"malacca");orbit.focus=new Vector3(210,0,130);orbit.distance=150;orbit.ApplyPose();Capture(camera,env,instances,"philippines");
  orbit.focus=orbit.homeFocus;orbit.distance=120;orbit.ApplyPose();env.mode=ManualSeaV2Environment.SkyMode.Night;env.Apply();Capture(camera,env,instances,"night-stars");env.mode=ManualSeaV2Environment.SkyMode.Sunny;env.Apply();
  orbit.distance=60;orbit.ApplyPose();Capture(camera,env,instances,"fleet-close");env.mode=ManualSeaV2Environment.SkyMode.Sunset;env.Apply();Capture(camera,env,instances,"sunset-close");env.mode=ManualSeaV2Environment.SkyMode.Sunny;env.Apply();orbit.distance=120;orbit.ApplyPose();
  camera.orthographic=true;camera.orthographicSize=300;camera.transform.position=new Vector3(0,700,0);camera.transform.rotation=Quaternion.Euler(90,0,0);Capture(camera,env,instances,"map-overview");orbit.ApplyPose();env.Apply();
  EditorSceneManager.SaveScene(camera.gameObject.scene,ScenePath);AssetDatabase.SaveAssets();File.WriteAllText(Review+"/geometry.txt",$"World {width}x{height}; chunks {chunks}; near terrain triangles {nearTriangles}; far terrain triangles {farTriangles}; shared props {props}; instance cells {cells.Count}\nLand and bathymetry derive from southeast_asia.png, east_asia.png is style reference only.\n");Debug.Log("SOUTHEAST_ASIA_V2_PASS");
 }
 static void Capture(Camera camera,ManualSeaV2Environment env,ManualSeaV2Instances instances,string name){var rt=new RenderTexture(1600,1000,24){antiAliasing=4};var previous=RenderTexture.active;camera.targetTexture=rt;camera.aspect=1.6f;env.water.SetFloat("_PreviewTime",12);env.water.SetVector("_ViewForward",camera.transform.forward);instances.Render();foreach(var ship in UnityEngine.Object.FindObjectsByType<ManualSeaV2Ship>(FindObjectsSortMode.None))ship.Refresh();camera.Render();RenderTexture.active=rt;var image=new Texture2D(1600,1000,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,1000),0,0);image.Apply();File.WriteAllBytes(Review+"/"+name+".png",image.EncodeToPNG());UnityEngine.Object.DestroyImmediate(image);RenderTexture.active=previous;camera.targetTexture=null;camera.ResetAspect();env.water.SetFloat("_PreviewTime",-1);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
 static void CreateFleet(ManualSeaV2Environment env,ManualSeaV2Camera orbit){
  var mesh=new Mesh{name="Ship billboard"};mesh.vertices=new[]{new Vector3(-5,-.6f,0),new Vector3(5,-.6f,0),new Vector3(-5,9.4f,0),new Vector3(5,9.4f,0)};mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one};mesh.triangles=new[]{0,2,1,1,2,3};mesh.RecalculateBounds();mesh=Save(mesh,"Models/Environment","ShipBillboard.asset");
  var material=Save(new Material(Shader.Find("Maritime/ManualSeaV2/Ship")),"Materials","Ships.mat");
  string[] types={"junk","square","lateen"},names={"E","SE","S","SW","W","NW","N","NE"};Vector3[] offsets={new Vector3(-10,0,0),new Vector3(13,0,7),new Vector3(-4,0,-18)};
  for(int i=0;i<3;i++){
   var pos=orbit.homeFocus+offsets[i];bool placed=Sample(coast,pos.x,pos.z)<-6;
   for(int attempt=0;!placed&&attempt<80;attempt++){float angle=attempt*2.399f,radius=3+attempt*.45f;pos=orbit.homeFocus+offsets[i]+new Vector3(Mathf.Cos(angle)*radius,0,Mathf.Sin(angle)*radius);placed=Sample(coast,pos.x,pos.z)<-6;}
   if(!placed)throw new Exception("No open water for preview fleet");
   var root=new GameObject("Preview fleet - "+types[i]);root.transform.SetParent(env.transform);root.transform.position=pos;
   var art=new GameObject("Existing game ship art",typeof(MeshFilter),typeof(MeshRenderer));art.transform.SetParent(root.transform,false);art.GetComponent<MeshFilter>().sharedMesh=mesh;art.GetComponent<MeshRenderer>().sharedMaterial=material;
   var ship=root.AddComponent<ManualSeaV2Ship>();ship.view=env.view;ship.environment=env;ship.billboard=art.transform;ship.art=art.GetComponent<Renderer>();ship.phase=i*2;ship.directions=new Texture2D[8];
   for(int d=0;d<8;d++){ship.directions[d]=Resources.Load<Texture2D>("Sprites/ManualSailing/"+types[i]+"/"+names[d]);if(!ship.directions[d])throw new Exception("Missing existing ship texture "+types[i]+"/"+names[d]);}ship.Refresh();
  }
 }
 public static void BuildWaterRevision(){
  EditorSceneManager.OpenScene(ScenePath);
  foreach(string path in new[]{"Assets/Resources/Materials/ManualSailingV2/Water.mat","Assets/Resources/Materials/SoutheastAsiaV2/Water.mat"}){
   var material=AssetDatabase.LoadAssetAtPath<Material>(path);material.SetFloat("_WaveLength",26);material.SetFloat("_WaveStrength",.30f);EditorUtility.SetDirty(material);
  }
  var env=UnityEngine.Object.FindFirstObjectByType<ManualSeaV2Environment>();var orbit=env.view.GetComponent<ManualSeaV2Camera>();var instances=env.GetComponent<ManualSeaV2Instances>();
  orbit.focus=orbit.homeFocus;orbit.distance=60;orbit.yaw=0;orbit.ApplyPose();env.mode=ManualSeaV2Environment.SkyMode.Sunny;env.Apply();Capture(env.view,env,instances,"fleet-close");
  orbit.yaw=-45;orbit.ApplyPose();env.Apply();Capture(env.view,env,instances,"water-soft-reflection");
  orbit.yaw=0;orbit.ApplyPose();env.mode=ManualSeaV2Environment.SkyMode.Sunset;env.Apply();Capture(env.view,env,instances,"sunset-close");
  orbit.distance=orbit.homeDistance;orbit.ApplyPose();env.mode=ManualSeaV2Environment.SkyMode.Sunny;env.Apply();EditorSceneManager.SaveScene(env.gameObject.scene,ScenePath);AssetDatabase.SaveAssets();
  if(ShaderUtil.ShaderHasError(Shader.Find("Maritime/ManualSeaV2/Water")))throw new Exception("Water shader compile failed");
  BuildSavedScene();
 }
 public static void BuildSavedScene(){PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;Directory.CreateDirectory("Builds/SoutheastAsiaV2");var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/SoutheastAsiaV2/SoutheastAsiaV2.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("SEA build failed");}
 public static void Build(){PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;Generate();Directory.CreateDirectory("Builds/SoutheastAsiaV2");var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/SoutheastAsiaV2/SoutheastAsiaV2.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("SEA build failed");}
}
}



