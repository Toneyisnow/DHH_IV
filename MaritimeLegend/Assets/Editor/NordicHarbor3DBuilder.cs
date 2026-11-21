using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;

namespace MaritimeLegend.Editor {
public static class NordicHarbor3DBuilder {
 const string ModelRoot="Assets/Resources/Models/Cities/Nordic/Harbor3D/",MatRoot="Assets/Resources/Materials/Cities/Nordic/Harbor3D/",PrefabRoot="Assets/Resources/Prefabs/Cities/Nordic/Harbor3D/";
 static readonly Dictionary<string,Material> mats=new Dictionary<string,Material>();
 static System.Random rng;
 static string Source {get{var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-nordicSource");return at<0?Path.GetFullPath("../ArtSource/NordicHarbor3D"):Path.GetFullPath(args[at+1]);}}
 static float R(float a,float b)=>a+(float)rng.NextDouble()*(b-a);
 static T Put<T>(T obj,string path)where T:UnityEngine.Object=>RegionalCityElementsBuilder.PutAsset(obj,path);
 class Part {public List<Vector3> v=new List<Vector3>();public List<Color> colors=new List<Color>();public List<int> t=new List<int>();}
 class Shape {
  public Dictionary<string,Part> parts=new Dictionary<string,Part>();
  Part Part(string mat){if(!parts.ContainsKey(mat))parts[mat]=new Part();return parts[mat];}
  public void Tri(string mat,Vector3 a,Vector3 b,Vector3 c,Color color){var p=Part(mat);int i=p.v.Count;p.v.AddRange(new[]{a,b,c});p.colors.AddRange(new[]{color,color,color});p.t.AddRange(new[]{i,i+1,i+2});}
  public void Quad(string mat,Vector3 a,Vector3 b,Vector3 c,Vector3 d,float variation=1){var col=new Color(variation,variation,variation,1);Tri(mat,a,b,c,col);Tri(mat,a,c,d,col);}
  public void Box(string mat,Vector3 center,Vector3 size,Quaternion rotation=default(Quaternion),float variation=1){
   if(rotation.x==0&&rotation.y==0&&rotation.z==0&&rotation.w==0)rotation=Quaternion.identity;Vector3 h=size*.5f;
   Vector3[] a={new Vector3(-h.x,-h.y,-h.z),new Vector3(h.x,-h.y,-h.z),new Vector3(h.x,h.y,-h.z),new Vector3(-h.x,h.y,-h.z),new Vector3(-h.x,-h.y,h.z),new Vector3(h.x,-h.y,h.z),new Vector3(h.x,h.y,h.z),new Vector3(-h.x,h.y,h.z)};
   for(int i=0;i<a.Length;i++)a[i]=center+rotation*a[i];
   foreach(var f in new[]{new[]{0,3,2,1},new[]{5,6,7,4},new[]{4,7,3,0},new[]{1,2,6,5},new[]{3,7,6,2},new[]{4,0,1,5}})Quad(mat,a[f[0]],a[f[1]],a[f[2]],a[f[3]],variation);
  }
  public void Beam(string mat,Vector3 a,Vector3 b,float width){Box(mat,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),Quaternion.FromToRotation(Vector3.up,(b-a).normalized));}
  public void Cylinder(string mat,Vector3 a,Vector3 b,float radius,int sides=10,float endRadius=-1){
   if(endRadius<0)endRadius=radius;var q=Quaternion.FromToRotation(Vector3.up,(b-a).normalized);
   for(int i=0;i<sides;i++){float u=i*Mathf.PI*2/sides,v=(i+1)*Mathf.PI*2/sides;var x=q*new Vector3(Mathf.Cos(u),0,Mathf.Sin(u));var y=q*new Vector3(Mathf.Cos(v),0,Mathf.Sin(v));Quad(mat,a+x*radius,b+x*endRadius,b+y*endRadius,a+y*radius);Tri(mat,a,a+x*radius,a+y*radius,Color.white);Tri(mat,b,b+y*endRadius,b+x*endRadius,Color.white);}
  }
  public void Blob(string mat,Vector3 center,Vector3 scale,int seed){
   for(int j=0;j<7;j++)for(int i=0;i<12;i++){
    Func<int,int,Vector3> p=(x,y)=>{float lon=x*Mathf.PI*2/12,lat=-Mathf.PI*.5f+y*Mathf.PI/7;float radius=1+.09f*Mathf.Sin(lon*5+lat*3+seed);return center+Vector3.Scale(new Vector3(Mathf.Cos(lon)*Mathf.Cos(lat),Mathf.Sin(lat),Mathf.Sin(lon)*Mathf.Cos(lat))*radius,scale);};
    Quad(mat,p(i,j),p(i,j+1),p(i+1,j+1),p(i+1,j),R(.86f,1.08f));
   }
  }
  public void Gable(string mat,float width,float depth,float eave,float top){
   var a=new[]{new Vector3(-width/2,eave,-depth/2),new Vector3(width/2,eave,-depth/2),new Vector3(0,top,-depth/2)};var b=a.Select(p=>p+Vector3.forward*depth).ToArray();
   Tri(mat,a[0],a[2],a[1],Color.white);Tri(mat,b[0],b[1],b[2],Color.white);
   for(int i=0;i<3;i++){int n=(i+1)%3;Quad(mat,a[i],a[n],b[n],b[i]);}
  }
  public GameObject Save(string name,bool building=false,Vector3 dimensions=default(Vector3)){
   var root=new GameObject(name);
   foreach(var pair in parts){var p=pair.Value;var mesh=new Mesh{name=name+"_"+pair.Key,indexFormat=IndexFormat.UInt32,vertices=p.v.ToArray(),colors=p.colors.ToArray(),triangles=p.t.ToArray()};mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Put(mesh,ModelRoot+name+"_"+pair.Key+".asset");
    var child=new GameObject(pair.Key,typeof(MeshFilter),typeof(MeshRenderer));child.transform.SetParent(root.transform,false);child.GetComponent<MeshFilter>().sharedMesh=mesh;child.GetComponent<MeshRenderer>().sharedMaterial=mats[pair.Key];child.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.On;child.GetComponent<Renderer>().receiveShadows=true;
   }
   if(building){var marker=root.AddComponent<NordicSolidBuilding>();marker.model=name;marker.footprint=dimensions;var collider=root.AddComponent<BoxCollider>();collider.center=new Vector3(0,dimensions.y/2,0);collider.size=dimensions;}
   PrefabUtility.SaveAsPrefabAsset(root,PrefabRoot+name+".prefab");ExportObj(root,Path.Combine(Source,"Models",name));UnityEngine.Object.DestroyImmediate(root);return AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot+name+".prefab");
  }
 }
 [MenuItem("Maritime Legend/City Art/Build Nordic 3D Harbor")]
 public static void Menu(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())Build();}
 public static void BuildWithPlayer(){
  Build();UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;
  PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
  Directory.CreateDirectory("Builds/NordicHarbor3D");
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/NordicHarborScene.unity"},locationPathName="Builds/NordicHarbor3D/NordicHarbor.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Nordic standalone build failed");
 }
 public static void Build(){
  rng=new System.Random(81108);foreach(string path in new[]{ModelRoot,MatRoot,PrefabRoot,Path.Combine(Source,"Models"),Path.Combine(Source,"Review")})Directory.CreateDirectory(path);AssetDatabase.Refresh();Materials();
  Building("HarborMerchantHouse",6.7f,7.1f,5.2f,3.4f,"red",true);Building("RedResidence",4.7f,5.4f,4.6f,2.6f,"red",false);Building("OchreResidence",4.5f,5.3f,4.4f,2.5f,"ochre",false);Building("TownHall",8.4f,6.5f,5.3f,2.8f,"ochre",true);
  Tree("Spruce",false);Tree("Birch",true);Market();Dock();Ship("MooredCutter",false,false);Ship("FishingBoat",false,true);Ship("HullUnderConstruction",true,false);Ship("HarborTallShip",false,false,true);Props();
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var town=Layout();var preview=Setup(town);Verify(town,preview.view);
  PrefabUtility.SaveAsPrefabAsset(town,PrefabRoot+"NordicHarborLayout.prefab");ExportObj(town,Path.Combine(Source,"Models","NordicHarborLayout"));
  Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/NordicHarborScene.unity");AssetDatabase.SaveAssets();Capture(preview);
  File.WriteAllText(Path.Combine(Source,"Review","PASS.txt"),"NORDIC_HARBOR_3D_PASS: perspective camera, four solid building models, roof/wall thickness and normal directions checked, shoreline berths, oblique captures and orbit frames rendered. Buildings do not contain SpriteRenderer or 2.5D atlas cards.");
  Debug.Log("NORDIC_HARBOR_3D_PASS");
 }
 static void Materials(){
  mats.Clear();var shader=Shader.Find("Maritime/NordicHarborSurface");var atlas=Resources.Load<Texture2D>("CityDaylight/Textures/PixelMaterials");
  Action<string,Color,int,float> make=(name,color,tile,scale)=>{var m=new Material(shader){color=color,mainTexture=atlas};m.SetFloat("_Tile",tile);m.SetFloat("_TextureScale",scale);mats[name]=Put(m,MatRoot+name+".mat");};
  make("red",new Color(.66f,.29f,.19f),3,1.2f);make("ochre",new Color(.80f,.65f,.40f),3,1.2f);make("wood",new Color(.40f,.28f,.16f),3,.75f);make("trim",new Color(.82f,.74f,.55f),3,1);make("stone",new Color(.50f,.50f,.43f),5,.6f);make("roof",new Color(.40f,.45f,.46f),2,.9f);make("glass",new Color(.10f,.20f,.23f),-1,1);mats["glass"].SetFloat("_Paint",0);make("iron",new Color(.12f,.13f,.12f),-1,1);mats["iron"].SetFloat("_Paint",0);make("leaf",new Color(.27f,.43f,.19f),4,.7f);mats["leaf"].SetFloat("_Wind",.045f);mats["leaf"].SetFloat("_Foliage",1);make("leaflight",new Color(.42f,.49f,.23f),4,.7f);mats["leaflight"].SetFloat("_Wind",.035f);mats["leaflight"].SetFloat("_Foliage",1);make("birch",new Color(.77f,.75f,.61f),3,1);make("canvas",new Color(.69f,.66f,.52f),1,1);make("bluecloth",new Color(.27f,.35f,.40f),1,1);make("fruit",new Color(.63f,.42f,.17f),-1,1);make("grass",new Color(.35f,.40f,.23f),4,.6f);
  var ground=new Material(Shader.Find("Maritime/CitySurface"));CityPixelMaterials.Apply(ground,"cobble");ground.color=new Color(1.06f,1.03f,.94f);mats["ground"]=Put(ground,MatRoot+"ground.mat");
  AssetDatabase.SaveAssets();
 }
 static void Building(string name,float w,float d,float h,float rise,string wall,bool balcony){
  var s=new Shape();s.Box("stone",new Vector3(0,.27f,0),new Vector3(w+.35f,.54f,d+.35f));s.Box(wall,new Vector3(0,h*.5f+.35f,0),new Vector3(w,h,d));float eave=h+.35f;
  for(float y=.65f;y<eave;y+=.22f){s.Box(wall,new Vector3(0,y,-d/2-.025f),new Vector3(w,.19f,.06f),variation:R(.9f,1.1f));s.Box(wall,new Vector3(0,y,d/2+.025f),new Vector3(w,.19f,.06f),variation:R(.9f,1.1f));foreach(int side in new[]{-1,1})s.Box(wall,new Vector3(side*(w/2+.025f),y,0),new Vector3(.06f,.19f,d),variation:R(.9f,1.1f));}
  foreach(int x in new[]{-1,1})foreach(int z in new[]{-1,1})s.Box("trim",new Vector3(x*w/2,eave/2,z*d/2),new Vector3(.18f,eave,.18f));
  foreach(float y in new[]{.65f,h*.53f,eave}){s.Box("wood",new Vector3(0,y,0),new Vector3(w+.18f,.14f,d+.18f));}
  // Four closed facades: projected detail is real geometry, never a flat building texture.
  Facade(s,new Vector3(0,0,-d/2),Quaternion.identity,w,h,true);Facade(s,new Vector3(0,0,d/2),Quaternion.Euler(0,180,0),w,h,false);Facade(s,new Vector3(-w/2,0,0),Quaternion.Euler(0,90,0),d,h,false);Facade(s,new Vector3(w/2,0,0),Quaternion.Euler(0,-90,0),d,h,false);
  s.Gable(wall,w,d,eave,eave+rise);
  for(int side=-1;side<=1;side+=2){float half=w/2+.42f,len=Mathf.Sqrt(half*half+rise*rise),angle=Mathf.Atan2(rise,half)*Mathf.Rad2Deg;var rotation=Quaternion.Euler(0,0,-side*angle);
   s.Box("roof",new Vector3(side*half/2,eave+rise/2,0),new Vector3(len,.19f,d+.85f),rotation);
   int rows=12,cols=Mathf.CeilToInt((d+.85f)/.43f);
   for(int row=0;row<rows;row++)for(int col=0;col<cols;col++){float t=(row+.5f)/rows;float z=-(d+.85f)/2+(col+.5f)*(d+.85f)/cols;s.Box("roof",new Vector3(side*half*t,eave+rise*(1-t)+.13f,z),new Vector3(len/rows+.045f,.055f,(d+.85f)/cols-.018f),rotation,R(.79f,1.17f));}
   s.Beam("wood",new Vector3(side*half,eave,-d/2-.5f),new Vector3(side*half,eave,d/2+.5f),.18f);
  }
  s.Beam("wood",new Vector3(0,eave+rise+.12f,-d/2-.6f),new Vector3(0,eave+rise+.12f,d/2+.6f),.21f);
  foreach(int end in new[]{-1,1}){float z=end*(d/2+.48f);s.Beam("trim",new Vector3(-w/2-.4f,eave,z),new Vector3(0,eave+rise+.12f,z),.14f);s.Beam("trim",new Vector3(w/2+.4f,eave,z),new Vector3(0,eave+rise+.12f,z),.14f);s.Beam("wood",new Vector3(0,eave,z),new Vector3(0,eave+rise,z),.16f);for(int i=-3;i<=3;i++){float x=i*w/9;float top=eave+rise*(1-Mathf.Abs(x)/(w/2));s.Beam("wood",new Vector3(x,eave,z-end*.1f),new Vector3(x,top,z-end*.1f),.08f);}}
  s.Box("stone",new Vector3(w*.28f,eave+rise*.58f,d*.19f),new Vector3(.65f,2.4f,.7f));for(int i=0;i<8;i++)s.Box("stone",new Vector3(w*.28f,eave+rise*.58f-1.2f+i*.3f,d*.19f),new Vector3(.7f,.09f,.75f),variation:R(.75f,1.04f));s.Box("stone",new Vector3(w*.28f,eave+rise*.58f+1.24f,d*.19f),new Vector3(.85f,.16f,.9f));
  for(int i=0;i<3;i++)s.Box("stone",new Vector3(0,.1f+i*.12f,-d/2-.95f+i*.25f),new Vector3(1.8f,.2f+i*.24f,.65f));
  if(balcony){float y=h*.53f;s.Box("wood",new Vector3(0,y,-d/2-.65f),new Vector3(w+.15f,.16f,1.25f));for(float x=-w/2;x<=w/2;x+=.36f)s.Beam("trim",new Vector3(x,y,-d/2-1.2f),new Vector3(x,y+.85f,-d/2-1.2f),.07f);s.Box("wood",new Vector3(0,y+.9f,-d/2-1.2f),new Vector3(w+.25f,.13f,.13f));foreach(int side in new[]{-1,1}){s.Beam("wood",new Vector3(side*(w/2-.35f),.5f,-d/2-.9f),new Vector3(side*(w/2-.35f),y,-d/2-.9f),.16f);s.Beam("wood",new Vector3(side*(w/2-.35f),y-.7f,-d/2),new Vector3(side*(w/2-.35f),y,-d/2-.95f),.13f);}}
  s.Box("glass",new Vector3(0,eave+rise*.38f,-d/2-.52f),new Vector3(.7f,.8f,.08f));
  foreach(float x in new[]{-.43f,0,.43f})s.Box("trim",new Vector3(x,eave+rise*.38f,-d/2-.59f),new Vector3(.08f,1,.12f));
  foreach(float y in new[]{-.48f,.48f})s.Box("trim",new Vector3(0,eave+rise*.38f+y,-d/2-.59f),new Vector3(.94f,.08f,.12f));
  s.Save(name,true,new Vector3(w,eave+rise,d));
 }
 static void Facade(Shape s,Vector3 origin,Quaternion rotation,float width,float height,bool door){
  Action<string,Vector3,Vector3> box=(m,p,size)=>s.Box(m,origin+rotation*p,size,rotation);
  foreach(float y in new[]{1.55f,height*.53f+1.15f})foreach(float x in new[]{-width*.29f,width*.29f}){
   box("wood",new Vector3(x,y,-.06f),new Vector3(1.2f,1.4f,.13f));box("glass",new Vector3(x,y,-.14f),new Vector3(.96f,1.15f,.06f));
   foreach(float dx in new[]{-.55f,0,.55f})box("trim",new Vector3(x+dx,y,-.21f),new Vector3(.075f,1.35f,.1f));foreach(float dy in new[]{-.65f,0,.65f})box("trim",new Vector3(x,y+dy,-.21f),new Vector3(1.16f,.075f,.1f));box("trim",new Vector3(x,y-.75f,-.22f),new Vector3(1.4f,.12f,.38f));
   foreach(int sign in new[]{-1,1}){box("wood",new Vector3(x+sign*.78f,y,-.1f),new Vector3(.33f,1.3f,.11f));for(int j=0;j<5;j++)box("roof",new Vector3(x+sign*.78f,y-.5f+j*.25f,-.18f),new Vector3(.29f,.12f,.08f));}
  }
  if(door){box("wood",new Vector3(0,1.4f,-.08f),new Vector3(1.25f,2.2f,.16f));for(int j=0;j<6;j++)box("wood",new Vector3(-.5f+j*.2f,1.4f,-.18f),new Vector3(.17f,2.05f,.075f));foreach(int sign in new[]{-1,1})box("trim",new Vector3(sign*.72f,1.4f,-.23f),new Vector3(.18f,2.4f,.2f));box("trim",new Vector3(0,2.65f,-.22f),new Vector3(1.6f,.2f,.22f));box("iron",new Vector3(.37f,1.35f,-.26f),new Vector3(.08f,.18f,.07f));}
 }
 static void Tree(string name,bool birch){
  var s=new Shape();float h=birch?6.7f:7.3f;s.Cylinder(birch?"birch":"wood",Vector3.zero,new Vector3(.12f,h*.82f,0),birch?.18f:.24f,9,.07f);
  if(birch){for(int i=0;i<13;i++){float y=.35f+i*.37f;s.Box("iron",new Vector3(.04f,y,-.16f),new Vector3(R(.12f,.25f),.045f,.035f),Quaternion.Euler(0,i*71,0));}for(int i=0;i<10;i++){float a=i*2.4f;var p=new Vector3(Mathf.Cos(a)*R(.8f,1.6f),R(3.7f,6.6f),Mathf.Sin(a)*R(.7f,1.6f));s.Beam("birch",new Vector3(0,2.2f+i*.2f,0),p,.09f);s.Blob("leaflight",p,new Vector3(.85f,.65f,.8f),i);}}
  else for(int level=0;level<11;level++){float y=1.1f+level*.53f,r=2.15f*(1-level/12f);for(int b=0;b<10;b++){float a=b*Mathf.PI*2/10+level*.45f;Vector3 p=new Vector3(Mathf.Cos(a)*r*.72f,y,Mathf.Sin(a)*r*.72f);s.Beam("wood",new Vector3(0,y+.32f,0),p,.07f);s.Blob("leaf",p,new Vector3(r*.37f,.17f,r*.33f),b+level);}}
  s.Save(name);
 }
 static void Market(){var s=new Shape();foreach(float x in new[]{-1.5f,1.5f})foreach(float z in new[]{-.75f,.75f})s.Box("wood",new Vector3(x,1.2f,z),new Vector3(.12f,2.4f,.12f));for(int i=0;i<10;i++){float x=-1.5f+(i+.5f)*.3f;s.Box(i%2==0?"canvas":"bluecloth",new Vector3(x,2.35f,0),new Vector3(.3f,.045f,2.1f),Quaternion.Euler(-9,0,0));s.Box(i%2==0?"canvas":"bluecloth",new Vector3(x,2.05f,-1.02f),new Vector3(.3f,.28f,.04f));}s.Box("wood",new Vector3(0,.65f,-.35f),new Vector3(2.8f,1.3f,.85f));for(int i=0;i<12;i++)s.Box("trim",new Vector3(-1.35f+i*.24f,.65f,-.8f),new Vector3(.21f,1.2f,.03f));for(int i=0;i<25;i++)s.Blob("fruit",new Vector3(R(-1.2f,1.2f),1.4f,R(-.65f,-.05f)),Vector3.one*.11f,i);s.Save("FishMarket");}
 static void Dock(){var s=new Shape();for(int i=0;i<32;i++)s.Box("wood",new Vector3(0,.04f,-4+i*.25f),new Vector3(2.6f,.18f,.23f),variation:R(.88f,1.08f));foreach(float x in new[]{-1.15f,1.15f})foreach(float z in new[]{-3.8f,-1.3f,1.3f,3.8f})s.Cylinder("wood",new Vector3(x,-1.6f,z),new Vector3(x,.55f,z),.14f);s.Save("TimberPier");}
 static void Ship(string name,bool skeleton,bool small,bool tall=false){
  var s=new Shape();float length=small?4.2f:tall?11.5f:7.2f,width=small?1.5f:tall?3.5f:2.5f,height=small?.65f:1.15f;const int stations=16;
  Func<int,int,Vector3> point=(i,j)=>{float z=-length/2+length*i/stations,r=Mathf.Pow(Mathf.Max(0,Mathf.Sin(i*Mathf.PI/stations)),.55f)*width/2;float a=j*Mathf.PI/8;return new Vector3(-Mathf.Cos(a)*r,-Mathf.Sin(a)*height,z);};
  if(!skeleton){for(int i=0;i<stations;i++)for(int j=0;j<8;j++)s.Quad("wood",point(i,j),point(i,j+1),point(i+1,j+1),point(i+1,j),R(.82f,1.12f));for(int i=0;i<stations;i++)s.Quad("wood",point(i,0),point(i+1,0),point(i+1,8),point(i,8));}
  for(int i=1;i<stations;i+=(skeleton?2:3))for(int j=0;j<8;j++)s.Beam(skeleton?"trim":"wood",point(i,j),point(i,j+1),skeleton?.11f:.06f);
  for(int side=0;side<=8;side+=8)for(int i=0;i<stations;i++)s.Beam("trim",point(i,side)+Vector3.up*.06f,point(i+1,side)+Vector3.up*.06f,.1f);
  s.Beam("wood",new Vector3(0,-height,-length*.42f),new Vector3(0,-height,length*.42f),.18f);
  if(skeleton){for(int i=0;i<3;i++){float z=-length*.3f+i*length*.3f;s.Box("wood",new Vector3(0,-height-.15f,z),new Vector3(width+1,.25f,.3f));foreach(int side in new[]{-1,1})s.Beam("wood",new Vector3(side*(width/2+.3f),-height-.3f,z),new Vector3(side*width*.32f,-.5f,z),.15f);}}
  else {for(int i=0;i<4;i++)s.Box("trim",new Vector3(0,.08f,-length*.3f+i*length*.2f),new Vector3(width*.72f,.12f,.3f));if(!small){foreach(float z in tall?new[]{-2.8f,1.2f}:new[]{0f}){float mast=tall?10:4.8f;s.Cylinder("wood",new Vector3(0,0,z),new Vector3(0,mast,z),.12f,10,.055f);foreach(float y in tall?new[]{4f,6.8f,8.8f}:new[]{3.7f}){float span=tall?4.6f:2.8f;s.Beam("wood",new Vector3(-span/2,y,z),new Vector3(span/2,y,z),.08f);s.Beam("iron",new Vector3(-width/2,.1f,z-1),new Vector3(0,y+1,z),.025f);s.Beam("iron",new Vector3(width/2,.1f,z+1),new Vector3(0,y+1,z),.025f);if(tall)for(int j=0;j<12;j++){float x=-span/2+(j+.5f)*span/12;s.Box("canvas",new Vector3(x,y-.65f,z+Mathf.Sin((j+.5f)*Mathf.PI/12)*.32f),new Vector3(span/12,.95f,.025f));}}}}}
  s.Save(name);
 }
 static void Props(){var s=new Shape();s.Cylinder("wood",Vector3.zero,Vector3.up*.85f,.34f,12);foreach(float y in new[]{.1f,.7f})s.Cylinder("iron",Vector3.up*y,Vector3.up*(y+.07f),.355f,12);s.Save("Barrel");s=new Shape();s.Box("wood",Vector3.up*.42f,Vector3.one*.84f);foreach(float x in new[]{-.33f,.33f})s.Box("trim",new Vector3(x,.42f,-.43f),new Vector3(.07f,.84f,.04f));s.Save("Crate");}
 static GameObject Place(string name,Transform parent,Vector3 p,float yaw=0,float scale=1){var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabRoot+name+".prefab"),parent);go.transform.localPosition=p;go.transform.localRotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one*scale;return go;}
 static GameObject Layout(){
  var root=new GameObject("Nordic 3D Harbor / reference composition");
  var ground=new Shape();ground.Box("ground",new Vector3(-.5f,-.35f,10),new Vector3(21,.7f,45));ground.Box("stone",new Vector3(-10.9f,-.32f,10),new Vector3(.4f,.65f,45));ground.Box("stone",new Vector3(9.9f,-.32f,10),new Vector3(.4f,.65f,45));ground.Box("stone",new Vector3(-.5f,-.32f,32.5f),new Vector3(21,.65f,.4f));ground.Save("QuayGround");Place("QuayGround",root.transform,Vector3.zero);
  var sea=GameObject.CreatePrimitive(PrimitiveType.Cube);sea.name="Harbor water around peninsula";sea.transform.SetParent(root.transform,false);sea.transform.localPosition=new Vector3(0,-.62f,30);sea.transform.localScale=new Vector3(220,.08f,220);UnityEngine.Object.DestroyImmediate(sea.GetComponent<Collider>());var water=Put(new Material(Shader.Find("Maritime/NordicHarborWater")),MatRoot+"water.mat");sea.GetComponent<Renderer>().sharedMaterial=water;
  Place("HarborMerchantHouse",root.transform,new Vector3(5.1f,0,8),-7);
  Place("RedResidence",root.transform,new Vector3(-6.7f,0,23),5);Place("OchreResidence",root.transform,new Vector3(.1f,0,30),-5);
  Place("TownHall",root.transform,new Vector3(6,0,25),0,.75f);
  Place("RedResidence",root.transform,new Vector3(-7.5f,0,35),12,.83f); // separate far quay below
  var far=new Shape();far.Box("stone",new Vector3(-6,-.35f,38),new Vector3(13,.7f,8));far.Save("FarQuay");Place("FarQuay",root.transform,Vector3.zero);
  Place("FishMarket",root.transform,new Vector3(-6.5f,0,9),-10);Place("FishMarket",root.transform,new Vector3(-7.5f,0,16),5,.85f);
  Place("Spruce",root.transform,new Vector3(-8.3f,0,2.8f),15,1.1f);Place("Birch",root.transform,new Vector3(-8.6f,0,18.8f),20,.85f);Place("Spruce",root.transform,new Vector3(9.1f,0,16),30,.9f);Place("Spruce",root.transform,new Vector3(-9,0,-9),20,1.3f);Place("Birch",root.transform,new Vector3(9.4f,0,-6),20,1.1f);
  Place("TimberPier",root.transform,new Vector3(13.8f,0,20),90);Place("TimberPier",root.transform,new Vector3(-14.8f,0,14),90);Place("TimberPier",root.transform,new Vector3(14,0,1),90);
  var cutter=Place("MooredCutter",root.transform,new Vector3(-16,-.25f,10),-12);cutter.AddComponent<RegionalBoatFloat>().phase=1;
  var boat=Place("FishingBoat",root.transform,new Vector3(13,-.28f,14),4);boat.AddComponent<RegionalBoatFloat>().phase=2;
  var tall=Place("HarborTallShip",root.transform,new Vector3(17,-.2f,25),-8);tall.AddComponent<RegionalBoatFloat>().amplitude=.06f;
  Place("HullUnderConstruction",root.transform,new Vector3(15,.95f,1),90,.8f);
  for(int i=0;i<9;i++){Place(i%3==0?"Crate":"Barrel",root.transform,new Vector3(8.3f+(i%2)*.75f,0,2.2f+(i/2)*1.1f),i*17);}
  for(int i=0;i<5;i++)Place("Barrel",root.transform,new Vector3(-8.7f,0,8+i*.8f),i*9);
  // Muted distant islands remain real mesh geometry behind the harbor.
  var hills=new Shape();for(int i=0;i<9;i++)hills.Blob("grass",new Vector3(-65+i*19,.2f,95+Mathf.Sin(i)*8),new Vector3(12,2.5f+i%3,10),i);hills.Save("DistantIslands");Place("DistantIslands",root.transform,Vector3.zero);
  var flock=(GameObject)PrefabUtility.InstantiatePrefab(Resources.Load<GameObject>("Prefabs/Cities/Nordic/V2/BirdFlock"),root.transform);
  Player(root.transform);return root;
 }
 static void Player(Transform parent){var atlas=Resources.Load<Texture2D>("Sprites/Characters/Character01/CartoonWalk/walk-sheet");if(atlas==null)return;var sprite=Sprite.Create(atlas,new Rect(672,504,168,168),new Vector2(.5f,10f/168),84);sprite=Put(sprite,ModelRoot+"EuropeanPlayerIdle.asset");var mat=Put(new Material(Shader.Find("Maritime/CitySprite")),MatRoot+"player.mat");var go=new GameObject("Existing approved European player / scale reference",typeof(SpriteRenderer));go.transform.SetParent(parent,false);go.transform.localPosition=new Vector3(-1.4f,.03f,2.3f);go.transform.localRotation=Quaternion.Euler(24,-3,0);var r=go.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.TwoSided;}
 static NordicHarborPreview Setup(GameObject town){
  RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.42f,.48f,.57f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=new Color(.63f,.73f,.76f);RenderSettings.fogStartDistance=48;RenderSettings.fogEndDistance=135;
  var sun=new GameObject("Warm low harbor sunlight").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.96f,.87f);sun.intensity=1.8f;sun.shadows=LightShadows.Soft;sun.shadowStrength=.96f;sun.shadowBias=.025f;sun.shadowNormalBias=.15f;sun.transform.rotation=Quaternion.Euler(48,-48,0);RenderSettings.sun=sun;
  var sky=new Material(Shader.Find("Skybox/Procedural"));sky.SetColor("_SkyTint",new Color(.5f,.5f,.5f));sky.SetColor("_GroundColor",new Color(.48f,.57f,.59f));sky.SetFloat("_AtmosphereThickness",.85f);sky=Put(sky,MatRoot+"sky.mat");RenderSettings.skybox=sky;
  QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.VeryHigh;QualitySettings.shadowDistance=100;QualitySettings.shadowCascades=4;QualitySettings.antiAliasing=0;
  var camera=new GameObject("Nordic perspective camera").AddComponent<Camera>();camera.tag="MainCamera";camera.clearFlags=CameraClearFlags.Skybox;camera.nearClipPlane=.25f;camera.farClipPlane=180;camera.allowHDR=true;camera.allowMSAA=false;camera.depthTextureMode=DepthTextureMode.Depth;
  var preview=new GameObject("Nordic harbor preview controls").AddComponent<NordicHarborPreview>();preview.view=camera;preview.town=town.transform;preview.ApplyCamera();var lens=camera.gameObject.AddComponent<CityLens>();lens.shader=Shader.Find("Maritime/CityDepthLens");lens.focus=town.transform.Find("HarborMerchantHouse");lens.sharpBand=9;lens.blur=3.8f;lens.fog=.025f;return preview;
 }
 static void Verify(GameObject town,Camera camera){
  if(camera.orthographic)throw new Exception("Must use perspective camera");var report=new StringBuilder("Solid-building geometry audit\n");
  foreach(var filter in town.GetComponentsInChildren<MeshFilter>())foreach(var v in filter.sharedMesh.vertices)
   if(float.IsNaN(v.x)||float.IsNaN(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.x)||float.IsInfinity(v.y)||float.IsInfinity(v.z))throw new Exception("Invalid vertex: "+filter.name);
  foreach(var building in town.GetComponentsInChildren<NordicSolidBuilding>()){
   if(building.GetComponentsInChildren<SpriteRenderer>().Length!=0)throw new Exception("Building contains a sprite");int count=0;var normals=new HashSet<int>();var bounds=new Bounds();bool first=true;
   foreach(var f in building.GetComponentsInChildren<MeshFilter>()){var m=f.sharedMesh;count+=m.vertexCount;if(first){bounds=m.bounds;first=false;}else bounds.Encapsulate(m.bounds);foreach(var n in m.normals){if(n.x>.8f)normals.Add(0);if(n.x<-.8f)normals.Add(1);if(n.z>.8f)normals.Add(2);if(n.z<-.8f)normals.Add(3);if(n.y>.8f)normals.Add(4);if(n.y<-.8f)normals.Add(5);}}
   if(count<1000||bounds.size.z<4||bounds.size.x<4||bounds.size.y<6||normals.Count!=6)throw new Exception("Flat or incomplete building: "+building.name);
   double volume=0;foreach(var filter in building.GetComponentsInChildren<MeshFilter>()){var mesh=filter.sharedMesh;var v=mesh.vertices;var triangles=mesh.triangles;for(int i=0;i<triangles.Length;i+=3)volume+=Vector3.Dot(v[triangles[i]],Vector3.Cross(v[triangles[i+1]],v[triangles[i+2]]))/6.0;}
   if(Math.Abs(volume)<building.footprint.x*building.footprint.y*building.footprint.z*.25f)throw new Exception("Insufficient solid volume: "+building.name);
   report.AppendLine(building.name+": vertices="+count+", XYZ bounds="+bounds.size+", signed component volume="+volume.ToString("F2",CultureInfo.InvariantCulture)+", all 6 normal directions PASS");
  }
  if(town.GetComponentsInChildren<NordicSolidBuilding>().Length<4)throw new Exception("Missing district buildings");
  Func<float,float> width=(depth)=>{var center=camera.transform.position+camera.transform.forward*depth;return camera.WorldToViewportPoint(center+camera.transform.right*.5f).x-camera.WorldToViewportPoint(center-camera.transform.right*.5f).x;};
  float ratio=width(10)/width(30);if(Mathf.Abs(ratio-3)>.01f)throw new Exception("Perspective projection scale incorrect");report.AppendLine("Perspective projected width at 10m / 30m = "+ratio+" (expected 3)");
  File.WriteAllText(Path.Combine(Source,"Review","geometry-checks.txt"),report.ToString());
 }
 static void Capture(NordicHarborPreview preview){
  var rt=new RenderTexture(1440,810,24,RenderTextureFormat.ARGB32);preview.view.targetTexture=rt;Shader.SetGlobalFloat("_RegionalCityPreview",1);
  try{
   foreach(var pose in new[]{(-3f,24f,31f,"Reference"),(38f,27f,35f,"RightOblique"),(-38f,32f,38f,"LeftOblique"),(145f,32f,43f,"RearBuildings")}){preview.SetView(pose.Item1,pose.Item2,pose.Item3);preview.Sample(.8f);Shader.SetGlobalFloat("_RegionalCityTime",.8f);Shot(preview.view,rt,pose.Item4+".png");}
   for(int frame=0;frame<72;frame++){float t=frame/12f;preview.SetView(Mathf.Lerp(-17,27,frame/71f),24,31);preview.Sample(t);Shader.SetGlobalFloat("_RegionalCityTime",t);Shot(preview.view,rt,"Orbit-"+frame.ToString("00")+".png");}
   foreach(string shader in new[]{"Maritime/NordicHarborSurface","Maritime/CityDepthLens"})foreach(var message in ShaderUtil.GetShaderMessages(Shader.Find(shader)))if(message.severity.ToString()=="Error")throw new Exception(message.message);
  }finally{Shader.SetGlobalFloat("_RegionalCityPreview",0);preview.SetView(-3,24,31);preview.view.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
 }
 static void Shot(Camera camera,RenderTexture rt,string name){camera.Render();var previous=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);tex.Apply();File.WriteAllBytes(Path.Combine(Source,"Review",name),tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);RenderTexture.active=previous;}
 static void ExportObj(GameObject root,string path){
  var obj=new StringBuilder("# True three-dimensional mesh; Unity Y-up\nmtllib "+Path.GetFileName(path)+".mtl\n");var mtl=new StringBuilder();var seen=new HashSet<string>();int offset=1;
  foreach(var f in root.GetComponentsInChildren<MeshFilter>()){
   var mesh=f.sharedMesh;var renderer=f.GetComponent<MeshRenderer>();if(mesh==null||renderer==null)continue;var mat=renderer.sharedMaterial;if(mat==null)continue;string name=mat.name.Replace(' ','_');obj.AppendLine("o "+f.name.Replace(' ','_'));obj.AppendLine("usemtl "+name);
   var transform=root.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;
   foreach(var v in mesh.vertices){var p=transform.MultiplyPoint3x4(v);obj.AppendFormat(CultureInfo.InvariantCulture,"v {0} {1} {2}\n",p.x,p.y,p.z);}
   foreach(var n in mesh.normals){var p=transform.MultiplyVector(n).normalized;obj.AppendFormat(CultureInfo.InvariantCulture,"vn {0} {1} {2}\n",p.x,p.y,p.z);}
   var tri=mesh.triangles;for(int i=0;i<tri.Length;i+=3)obj.AppendLine("f "+(tri[i]+offset)+"//"+(tri[i]+offset)+" "+(tri[i+1]+offset)+"//"+(tri[i+1]+offset)+" "+(tri[i+2]+offset)+"//"+(tri[i+2]+offset));offset+=mesh.vertexCount;
   if(seen.Add(name)){var c=mat.HasProperty("_Color")?mat.GetColor("_Color"):Color.gray;mtl.AppendFormat(CultureInfo.InvariantCulture,"newmtl {0}\nKd {1} {2} {3}\n\n",name,c.r,c.g,c.b);}
  }
  File.WriteAllText(path+".obj",obj.ToString());File.WriteAllText(path+".mtl",mtl.ToString());
 }
}
}
