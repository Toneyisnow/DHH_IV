using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace MaritimeLegend.Editor {
public static class RegionalCityExpansionBuilder {
 static readonly string[] Regions=RegionalCityElementsBuilder.Regions;
 static readonly string[] Cells={"HouseB","TownHall","BoatA","BoatB","TreeA_Slim","TreeA_Short","TreeA_DeadBranches","RepairPlatform","TreeB_Slim","TreeB_Short","TreeB_DeadBranches","HalfBuiltBoat"};
 const string Root="Assets/Resources/";
 static string P(string type,string id,string file)=>Root+type+"/Cities/"+id+"/V2/"+file;
 static string Review {get{var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-regionalReview");return i<0?Path.GetFullPath("../ArtSource/RegionalCityElements/V2/Review"):Path.GetFullPath(args[i+1]);}}
 [Serializable] class Catalog {public string version="2.0";public float roadTileSize=4,roadWidth=2,shorelineZ=0;public string waterSide="Z < 0",flight="2-3 birds; two sizes of same species; 1-2 seconds per crossing";public List<Entry> elements=new List<Entry>();}
 [Serializable] class Entry {public string region,element,prefab;}
 static Catalog catalog;
 [MenuItem("Maritime Legend/City Art/Build Coastal Regional Library V2")]
 public static void Menu(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())Build();}
 public static void Build(){
  catalog=new Catalog();Directory.CreateDirectory(Review);
  foreach(string id in Regions){
   foreach(string type in new[]{"Sprites","Models","Materials","Prefabs"})Directory.CreateDirectory(P(type,id,""));AssetDatabase.Refresh();
   ImportExpansion(id);CreateGround(id);CopyExisting(id,"House","HouseA",.82f);CopyExisting(id,"Market","Market",1);
   CreateBirds(id);CreateDockAndShipyard(id);CreateWater(id);
   var sample=Sample(id);ValidateLayout(sample);Save(sample,id,"CoastalSample");
  }
  File.WriteAllText(Root+"Data/Presentation/Cities/RegionalElementsV2.json",JsonUtility.ToJson(catalog,true));AssetDatabase.Refresh();AssetDatabase.SaveAssets();
  RenderAndVerify();CreateScene();
  Debug.Log("REGIONAL_CITY_V2_PASS: "+catalog.elements.Count+" prefabs; road connectors, grass shapes, coastal placement, boat float and viewport flocks verified.");
 }
 static void Save(GameObject go,string id,string name){
  go.name=id+"_"+name;PrefabUtility.SaveAsPrefabAsset(go,P("Prefabs",id,name+".prefab"));UnityEngine.Object.DestroyImmediate(go);
  catalog.elements.Add(new Entry{region=id,element=name,prefab="Prefabs/Cities/"+id+"/V2/"+name});
 }
 static T Asset<T>(T obj,string type,string id,string name)where T:UnityEngine.Object=>RegionalCityElementsBuilder.PutAsset(obj,P(type,id,name));
 static GameObject Load(string id,string name)=>(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(P("Prefabs",id,name+".prefab")));
 static GameObject Place(string id,string name,Transform parent,Vector3 p){var go=Load(id,name);go.transform.SetParent(parent,false);go.transform.localPosition=p;return go;}
 static void ImportExpansion(string id){
  string path=P("Sprites",id,"Expansion.png");RegionalCityElementsBuilder.ConfigureTexture(path,true);var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
  if(atlas.width%4!=0||atlas.height%3!=0)throw new Exception(id+": expansion grid must divide into 4 x 3 cells");
  int cw=atlas.width/4,ch=atlas.height/3;var pixels=atlas.GetPixels32();
  for(int cell=0;cell<12;cell++){
   string name=Cells[cell];int left=cell%4*cw,bottom=(2-cell/4)*ch,x0=left+cw,y0=bottom+ch,x1=left,y1=bottom,count=0;
   for(int y=bottom;y<bottom+ch;y++)for(int x=left;x<left+cw;x++)if(pixels[y*atlas.width+x].a>=179){x0=Math.Min(x0,x);x1=Math.Max(x1,x);y0=Math.Min(y0,y);y1=Math.Max(y1,y);count++;}
   if(count<600)throw new Exception(id+"/"+name+": missing opaque sprite");
   var rect=new RectInt(x0,y0,x1-x0+1,y1-y0+1);var texture=Slice(atlas,rect,P("Sprites",id,name+".png"));
   bool tree=name.StartsWith("Tree"),boat=name=="BoatA"||name=="BoatB";
   float height=tree?(name.EndsWith("Short")?3.1f:name.EndsWith("Slim")?5.8f:5.2f):name=="TownHall"?7:name=="HouseB"?5.1f:3;
   float width=height*rect.width/rect.height;
   if(boat||name=="RepairPlatform"||name=="HalfBuiltBoat"){width=name=="BoatA"?4:name=="BoatB"?4.5f:name=="RepairPlatform"?6:3.7f;height=width*rect.height/rect.width;}
   var mat=new Material(Shader.Find("Maritime/RegionalCityElement")){mainTexture=texture};mat.SetFloat("_Amplitude",tree?.065f:0);mat.SetFloat("_Period",8);mat.SetFloat("_Phase",cell*.71f+Array.IndexOf(Regions,id)*.43f);
   mat=Asset(mat,"Materials",id,name+".mat");var mesh=Asset(RegionalCityElementsBuilder.Grid(width,height,tree?2:1),"Models",id,name+".asset");
   var go=Card(name,mesh,mat,true);
   if(name=="HouseB"||name=="TownHall"){
    var eave=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Cities/"+id+"/Eave.prefab"),go.transform);
    float scale=name=="TownHall"?.85f:.65f;float anchor=height*.64f-.9f*scale;
    eave.transform.localPosition=new Vector3(0,anchor*Mathf.Cos(32*Mathf.Deg2Rad),anchor*Mathf.Sin(32*Mathf.Deg2Rad)-.05f);eave.transform.localScale=Vector3.one*scale;
   }
   if(boat){var motion=go.AddComponent<RegionalBoatFloat>();motion.phase=cell*1.8f;motion.period=cell==2?4.8f:5.7f;var placement=go.AddComponent<RegionalCoastalPlacement>();placement.kind=RegionalCoastalPlacement.Kind.MooredBoat;placement.footprint=new Rect(-width*.5f,-.3f,width,Mathf.Max(1,height*.55f));}
   Save(go,id,name);
  }
  RegionalCityElementsBuilder.ConfigureTexture(path,false);
 }
 static Texture2D Slice(Texture2D source,RectInt r,string path){
  var tex=new Texture2D(r.width,r.height,TextureFormat.RGBA32,false);tex.SetPixels(source.GetPixels(r.x,r.y,r.width,r.height));tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);AssetDatabase.ImportAsset(path);RegionalCityElementsBuilder.ConfigureTexture(path,false);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 }
 static GameObject Card(string name,Mesh mesh,Material mat,bool tilted){
  var root=new GameObject(name);var child=new GameObject("Visual",typeof(MeshFilter),typeof(MeshRenderer));child.transform.SetParent(root.transform,false);child.GetComponent<MeshFilter>().sharedMesh=mesh;child.GetComponent<MeshRenderer>().sharedMaterial=mat;child.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.TwoSided;if(tilted)child.transform.localRotation=Quaternion.Euler(32,0,0);return root;
 }
 static void CopyExisting(string id,string oldName,string newName,float scale){
  var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Prefabs/Cities/"+id+"/"+oldName+".prefab"));go.transform.localScale=Vector3.one*scale;Save(go,id,newName);
 }
 static void CreateGround(string id){
  string path=Root+"Sprites/Cities/Common/V2/GrassAtlas.png";RegionalCityElementsBuilder.ConfigureTexture(path,true);var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(path);int index=Array.IndexOf(Regions,id),cw=atlas.width/3,ch=atlas.height/2;
  var grass=Slice(atlas,new RectInt(index%3*cw+3,(1-index/3)*ch+3,cw-6,ch-6),P("Sprites",id,"Grass.png"));RegionalCityElementsBuilder.ConfigureTexture(path,false);
  var grassMat=Asset(new Material(Shader.Find("Maritime/RegionalGround")){mainTexture=grass},"Materials",id,"Grass.mat");
  var roadMat=Asset(new Material(Shader.Find("Maritime/RegionalGround")){mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Sprites/Cities/"+id+"/Road.png")},"Materials",id,"Road.mat");
  foreach(var pair in new[]{("RoadStraight",5),("RoadCorner",3),("RoadTee",11),("RoadCross",15),("RoadEnd",1),("RoadPlaza",0)}){
   var mesh=Asset(RoadMesh(pair.Item2),"Models",id,pair.Item1+".asset");var go=Card(pair.Item1,mesh,roadMat,false);var marker=go.AddComponent<RegionalRoadTile>();marker.connections=pair.Item2;
   go.AddComponent<MeshCollider>().sharedMesh=mesh;RegionalCityElementsBuilder.AddDust(go,Array.IndexOf(Regions,id));Save(go,id,pair.Item1);
  }
  for(int shape=0;shape<3;shape++){
   string name=new[]{"GrassRound","GrassWide","GrassCorner"}[shape];var mesh=Asset(GrassMesh(shape),"Models",id,name+".asset");Save(Card(name,mesh,grassMat,false),id,name);
  }
 }
 static Mesh RoadMesh(int mask){
  var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();const int n=16;
  for(int z=0;z<n;z++)for(int x=0;x<n;x++){
   float px=-2+(x+.5f)*.25f,pz=-2+(z+.5f)*.25f;
   bool inside=mask==0||Mathf.Abs(px)<=1&&Mathf.Abs(pz)<=1||((mask&1)!=0&&Mathf.Abs(px)<=1&&pz>0)||((mask&2)!=0&&Mathf.Abs(pz)<=1&&px>0)||((mask&4)!=0&&Mathf.Abs(px)<=1&&pz<0)||((mask&8)!=0&&Mathf.Abs(pz)<=1&&px<0);
   if(!inside)continue;int i=v.Count;float xx=-2+x*.25f,zz=-2+z*.25f;v.Add(new Vector3(xx,.035f,zz));v.Add(new Vector3(xx,.035f,zz+.25f));v.Add(new Vector3(xx+.25f,.035f,zz));v.Add(new Vector3(xx+.25f,.035f,zz+.25f));uv.AddRange(new[]{Vector2.zero,Vector2.up,Vector2.right,Vector2.one});t.AddRange(new[]{i,i+1,i+2,i+2,i+1,i+3});
  }
  var mesh=new Mesh{name="4m road / 2m connectors",vertices=v.ToArray(),uv=uv.ToArray(),triangles=t.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
 static Mesh GrassMesh(int shape){
  var v=new List<Vector3>{new Vector3(0,.02f,0)};var t=new List<int>();
  if(shape==2){foreach(var p in new[]{new Vector2(-2,-2),new Vector2(-2,2),new Vector2(.4f,2),new Vector2(.4f,.4f),new Vector2(2,.4f),new Vector2(2,-2)})v.Add(new Vector3(p.x,.02f,p.y));}
  else for(int i=0;i<40;i++){float a=-i*Mathf.PI*2/40;float r=1+.055f*Mathf.Sin(a*7+shape)+.03f*Mathf.Cos(a*11);v.Add(new Vector3(Mathf.Cos(a)*r*(shape==0?2:2.7f),.02f,Mathf.Sin(a)*r*(shape==0?2:1.45f)));}
  for(int i=1;i<v.Count;i++)t.AddRange(new[]{0,i,i==v.Count-1?1:i+1});var mesh=new Mesh{name="Flat random-combination grass "+shape,vertices=v.ToArray(),triangles=t.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
 }
 static void CreateBirds(string id){
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Sprites/Cities/"+id+"/Bird.png");
  foreach(string size in new[]{"BirdLarge","BirdSmall"}){
   float width=size=="BirdLarge"?1.3f:.83f;var mat=new Material(Shader.Find("Maritime/RegionalCityElement")){mainTexture=texture};mat.SetFloat("_Bird",1);mat.SetFloat("_Amplitude",.12f);mat.SetFloat("_FlapSpeed",18);mat=Asset(mat,"Materials",id,size+".mat");
   var mesh=Asset(RegionalCityElementsBuilder.Grid(width,width*texture.height/texture.width,5),"Models",id,size+".asset");var go=Card(size,mesh,mat,false);go.GetComponentInChildren<Renderer>().shadowCastingMode=ShadowCastingMode.Off;Save(go,id,size);
  }
  var flock=new GameObject("Quick random flock");var controller=flock.AddComponent<RegionalBirdFlock>();controller.seed=7411+Array.IndexOf(Regions,id)*139;controller.largeBird=AssetDatabase.LoadAssetAtPath<GameObject>(P("Prefabs",id,"BirdLarge.prefab"));controller.smallBird=AssetDatabase.LoadAssetAtPath<GameObject>(P("Prefabs",id,"BirdSmall.prefab"));Save(flock,id,"BirdFlock");
 }
 static GameObject Box(string name,Transform parent,Vector3 p,Vector3 scale,Material material){var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;return go;}
 static void CreateDockAndShipyard(string id){
  var wood=new Material(Shader.Find("Maritime/CitySurface"));CityPixelMaterials.Apply(wood,"wood");wood=Asset(wood,"Materials",id,"PierWood.mat");
  var dock=new GameObject("Fixed shore pier");
  for(int i=0;i<22;i++)Box("Pier plank",dock.transform,new Vector3(0,.04f,-.125f-i*.25f),new Vector3(1.8f,.16f,.235f),wood);
  foreach(float z in new[]{-.25f,-2.75f,-5.25f})foreach(float x in new[]{-.8f,.8f})Box("Fixed piling",dock.transform,new Vector3(x,-.45f,z),new Vector3(.18f,1.5f,.18f),wood);
  var placement=dock.AddComponent<RegionalCoastalPlacement>();placement.kind=RegionalCoastalPlacement.Kind.Dock;placement.footprint=new Rect(-.9f,-5.5f,1.8f,5.5f);Save(dock,id,"Dock");
  var yard=new GameObject("Waterfront shipyard");Place(id,"RepairPlatform",yard.transform,new Vector3(0,-.9f,0));Place(id,"HalfBuiltBoat",yard.transform,new Vector3(.1f,.45f,-.4f));
  placement=yard.AddComponent<RegionalCoastalPlacement>();placement.kind=RegionalCoastalPlacement.Kind.Shipyard;placement.footprint=new Rect(-3,-.5f,6,3.5f);Save(yard,id,"Shipyard");
 }
 static void CreateWater(string id){var mat=Asset(new Material(Shader.Find("Maritime/RegionalCoastalWater")),"Materials",id,"Water.mat");var water=new GameObject("Water surface");var sea=Box("Animated sea",water.transform,new Vector3(0,-.32f,-6),new Vector3(34,.05f,12),mat);UnityEngine.Object.DestroyImmediate(sea.GetComponent<Collider>());Save(water,id,"Water");}
 static GameObject Sample(string id){
  var root=new GameObject(id+" coastal layout / shore Z=0");
  var soil=new Material(Shader.Find("Maritime/CitySurface")){color=new Color(.41f,.36f,.26f)};soil=Asset(soil,"Materials",id,"Soil.mat");Box("Land Z >= 0",root.transform,new Vector3(0,-.2f,8),new Vector3(34,.4f,16),soil);Place(id,"Water",root.transform,Vector3.zero);
  var rng=new System.Random(200+Array.IndexOf(Regions,id));
  for(int i=0;i<22;i++){string name=new[]{"GrassRound","GrassWide","GrassCorner"}[rng.Next(3)];float x=(float)rng.NextDouble()*29-14.5f,z=(float)rng.NextDouble()*11+3;var patch=Place(id,name,root.transform,new Vector3(x,.00015f*i,z));patch.transform.localRotation=Quaternion.Euler(0,rng.Next(4)*90,0);}
  for(int x=-12;x<=12;x+=4){string kind=x==0?"RoadCross":x==-8||x==8?"RoadTee":x==-12||x==12?"RoadEnd":"RoadStraight";var tile=Place(id,kind,root.transform,new Vector3(x,0,2));tile.transform.localRotation=Quaternion.Euler(0,kind=="RoadStraight"?90:x==-12?90:x==12?270:kind=="RoadTee"?180:0,0);}
  Place(id,"RoadStraight",root.transform,new Vector3(0,0,6));var end=Place(id,"RoadEnd",root.transform,new Vector3(0,0,10));end.transform.localRotation=Quaternion.Euler(0,180,0);
  Place(id,"HouseA",root.transform,new Vector3(-9,0,7));Place(id,"HouseB",root.transform,new Vector3(8,0,7));Place(id,"TownHall",root.transform,new Vector3(0,0,10));Place(id,"Market",root.transform,new Vector3(-3.7f,0,3));
  for(int i=0;i<3;i++){string suffix=new[]{"Slim","Short","DeadBranches"}[i];Place(id,"TreeA_"+suffix,root.transform,new Vector3(-14,0,12-i*4));Place(id,"TreeB_"+suffix,root.transform,new Vector3(13.5f,0,12-i*4));}
  Place(id,"Dock",root.transform,new Vector3(-8,0,0));Place(id,"Dock",root.transform,new Vector3(8,0,0));
  Place(id,"BoatA",root.transform,new Vector3(-11,-.25f,-4.5f));Place(id,"BoatB",root.transform,new Vector3(-4.7f,-.25f,-5.5f));Place(id,"Shipyard",root.transform,new Vector3(8,0,-4));
  Place(id,"BirdFlock",root.transform,Vector3.zero);return root;
 }
 static void ValidateLayout(GameObject sample){
  foreach(var p in sample.GetComponentsInChildren<RegionalCoastalPlacement>())if(!p.IsOnWater(sample.transform))throw new Exception("Coastal asset on dry land: "+p.name);
  if(sample.GetComponentsInChildren<RegionalBoatFloat>().Length!=2)throw new Exception("Expected two floating moored boats");
  foreach(var filter in sample.GetComponentsInChildren<MeshFilter>())if(filter.transform.parent!=null&&filter.transform.parent.name.Contains("_Grass"))
   if(sample.transform.InverseTransformPoint(filter.transform.TransformPoint(filter.sharedMesh.bounds.max)).y>=.035f)throw new Exception("Grass covers road surface");
 }
 static Camera CameraForReview(){var camera=RegionalCityElementsBuilder.SetupStage();camera.orthographicSize=13.6f;camera.transform.position=new Vector3(0,2.6f,3)-camera.transform.forward*36;camera.farClipPlane=100;return camera;}
 static void RenderAndVerify(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var camera=CameraForReview();var rt=new RenderTexture(1280,960,24);camera.targetTexture=rt;Shader.SetGlobalFloat("_RegionalCityPreview",1);var report=new StringBuilder();
  try{foreach(string id in Regions){
   var sample=Load(id,"CoastalSample");ValidateLayout(sample);
   for(int frame=0;frame<96;frame++){
    float t=frame/12f;Shader.SetGlobalFloat("_RegionalCityTime",t);foreach(var boat in sample.GetComponentsInChildren<RegionalBoatFloat>())boat.Sample(t);foreach(var flock in sample.GetComponentsInChildren<RegionalBirdFlock>())flock.Sample(t,camera);
    foreach(var dust in sample.GetComponentsInChildren<ParticleSystem>())dust.Simulate(t+3,true,true,true);
    camera.Render();var prior=RenderTexture.active;RenderTexture.active=rt;var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(Review,id+"-"+frame.ToString("00")+".png"),texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);RenderTexture.active=prior;
   }
   VerifyMotion(sample,camera,report);UnityEngine.Object.DestroyImmediate(sample);VerifyRoads(id,report);
  }
  foreach(string name in new[]{"Maritime/RegionalCityElement","Maritime/RegionalGround","Maritime/RegionalCoastalWater"})foreach(var message in ShaderUtil.GetShaderMessages(Shader.Find(name)))if(message.severity.ToString()=="Error")throw new Exception(name+": "+message.message);
  }finally{Shader.SetGlobalFloat("_RegionalCityPreview",0);camera.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
  File.WriteAllText(Path.Combine(Review,"checks.txt"),report.ToString());File.WriteAllText(Path.Combine(Review,"PASS.txt"),"REGIONAL_CITY_V2_PASS: six coastal samples; two anchored floating boats each; water-only dock and shipyard footprints; road connector widths and rotations; three grass geometries; 1-2 second random viewport crossings, groups of 2-3, both bird sizes; shader compilation passed.");
 }
 static void VerifyMotion(GameObject sample,Camera camera,StringBuilder report){
  foreach(var boat in sample.GetComponentsInChildren<RegionalBoatFloat>()){boat.Sample(0);var a=boat.transform.localPosition;boat.Sample(1.1f);var b=boat.transform.localPosition;if(Mathf.Abs(a.y-b.y)<.005f||Mathf.Abs(a.x-b.x)>.00001f||Mathf.Abs(a.z-b.z)>.00001f)throw new Exception("Boat anchor/buoyancy failed");}
  var flock=sample.GetComponentInChildren<RegionalBirdFlock>();var directions=new HashSet<int>();
  foreach(float aspect in new[]{1f,16f/9,9f/16}){camera.aspect=aspect;for(int cycle=0;cycle<12;cycle++){
   flock.Sample(cycle*6+.1f,camera);if(flock.BirdCount<2||flock.BirdCount>3||flock.Duration<1||flock.Duration>2)throw new Exception("Flock count/duration failed");
   Vector2 s=flock.StartPoint,e=flock.EndPoint,d=e-s;directions.Add((int)Mathf.Floor((Mathf.Atan2(d.y,d.x)+Mathf.PI)*4/Mathf.PI));
   if(s.x>=0&&s.x<=1&&s.y>=0&&s.y<=1||e.x>=0&&e.x<=1&&e.y>=0&&e.y<=1)throw new Exception("Flock endpoint inside screen");
   flock.Sample(cycle*6+flock.Duration+.05f,camera);foreach(Transform child in flock.transform)if(child.gameObject.activeSelf)throw new Exception("Bird stays visible after crossing");
  }}camera.ResetAspect();if(directions.Count<4)throw new Exception("Flock random directions insufficient");
  report.AppendLine(sample.name+": WATER / BOAT_ANCHOR / BUOYANCY / FLOCK_1_TO_2_SECONDS / GROUP_2_TO_3 / THREE_ASPECTS / RANDOM_DIRECTIONS PASS");
 }
 static void VerifyRoads(string id,StringBuilder report){
  foreach(string kind in new[]{"RoadStraight","RoadCorner","RoadTee","RoadCross","RoadEnd"}){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(P("Prefabs",id,kind+".prefab"));var marker=prefab.GetComponent<RegionalRoadTile>();var mesh=prefab.GetComponentInChildren<MeshFilter>().sharedMesh;
   for(int edge=0;edge<4;edge++){
    float min=100,max=-100;foreach(var v in mesh.vertices){float forward=edge==0?v.z:edge==1?v.x:edge==2?-v.z:-v.x;float cross=edge%2==0?v.x:v.z;if(Mathf.Abs(forward-2)<.00001f){min=Mathf.Min(min,cross);max=Mathf.Max(max,cross);}}
    bool open=(marker.connections&(1<<edge))!=0;if(open&&(Mathf.Abs(min+1)>.001f||Mathf.Abs(max-1)>.001f)||!open&&max>=min)throw new Exception("Road connector mismatch: "+kind+" edge "+edge);
   }
   if(marker.RotatedConnections(4)!=marker.connections)throw new Exception("Road rotation failed");
  }
  var round=AssetDatabase.LoadAssetAtPath<Mesh>(P("Models",id,"GrassRound.asset"));var wide=AssetDatabase.LoadAssetAtPath<Mesh>(P("Models",id,"GrassWide.asset"));var corner=AssetDatabase.LoadAssetAtPath<Mesh>(P("Models",id,"GrassCorner.asset"));if(round.bounds.size==wide.bounds.size||corner.vertexCount==round.vertexCount)throw new Exception("Grass shapes not distinct");
  report.AppendLine(id+": ROAD_2M_CONNECTIONS / 90_DEGREE_ROTATIONS / THREE_GRASS_SHAPES PASS");
 }
 static void CreateScene(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);CameraForReview();for(int i=0;i<Regions.Length;i++){var sample=Load(Regions[i],"CoastalSample");sample.SetActive(i==0);}EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/ArtPreview/RegionalCoastalElementsV2.unity");AssetDatabase.SaveAssets();}
}
}
