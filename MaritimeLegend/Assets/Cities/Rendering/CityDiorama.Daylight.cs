using UnityEngine;

namespace MaritimeLegend {
public sealed partial class CityDiorama {
 CityDaylightKit kit; Vector3 cameraVelocity; Transform walkerQuad;
 public const float MapWidth=108, MapDepth=72;
 public float CameraDistance {get;private set;}
 void BuildDaylightTown() {
  kit=new CityDaylightKit(transform,definition.surfaceShader);
  if(venice) {
   foreach(int side in new[]{-1,1})Box("Canal district",new Vector3(side*28,-.45f,12),new Vector3(52,.9f,MapDepth),stone,true);
   for(int z=-22;z<48;z+=2)foreach(int side in new[]{-1,1})Box("Canal coping",new Vector3(side*2.15f,.12f,z),new Vector3(.3f,.24f,1.9f),wall,false);
   foreach(float z in new[]{1f,29f})kit.Place("Bridge","Stone arch bridge",new Vector3(0,0,z),90,1,true);
  } else {
   Box("Northern city foundation",new Vector3(0,-.45f,20.5f),new Vector3(MapWidth,.9f,55),stone,true);
   Box("Southern waterfront",new Vector3(0,-.45f,-18),new Vector3(MapWidth,.9f,12),stone,true);
   foreach(float x in new[]{-32f,0f,32f})kit.Place("Bridge","Canal footbridge",new Vector3(x,0,-9),0,1,true);
   foreach(float z in new[]{-12.1f,-6.9f})for(int x=-53;x<54;x+=2) {
    if(Mathf.Abs(x)<2||Mathf.Abs(x-32)<2||Mathf.Abs(x+32)<2)continue;
    Box("Canal embankment",new Vector3(x,.08f,z),new Vector3(1.95f,.16f,.25f),wall,false);
   }
   Boat(new Vector3(23,-.42f,-9));
   Box("Guild terrace",new Vector3(0,.9f,9),new Vector3(22,1.8f,8),wall,true);
   Box("Terrace paving",new Vector3(0,1.81f,9),new Vector3(22,.04f,8),stone,true);
   // Closed sides force navigation to use the broad stairway.
   obstacles.Add(new Rect(-11,4.6f,8.9f,.5f));obstacles.Add(new Rect(2.1f,4.6f,8.9f,.5f));
   for(int i=0;i<12;i++)Box("Broad stone step",new Vector3(0,(i+1)*.075f,1.9f+i*.3f),new Vector3(4,(i+1)*.15f,.34f),stone,true);
  }
  AddBuilding("MarketA","市场A",new Vector3(-7,HeightAt(-7,9),9),6,6.2f,"市场A");
  AddBuilding("MarketB","市场B",new Vector3(7,HeightAt(7,9),9),6,6.2f,"市场B");
  AddBuilding("TownHall","市政厅",new Vector3(-9,0,26),8,5,"市政厅");
  AddBuilding("Tavern","酒馆",new Vector3(12,0,-3),6,4,"酒馆");
  AddBuilding("Shipyard","船坞",new Vector3(-17,0,-20.5f),7,5,"船坞");
  Box("Shipyard waterfront deck",new Vector3(-17,-.12f,-25),new Vector3(11,.24f,5),wood,true);
  for(int i=0;i<8;i++)Box("Dock piling",new Vector3(-22+i*1.4f,-.7f,-27),new Vector3(.2f,1.9f,.2f),wood,false);
  Boat(new Vector3(-17,-.42f,-29));Mast(new Vector3(-17,-.42f,-29));
  Landmarks["产业园"]=new Vector3(20,0,23);Landmarks["码头"]=new Vector3(-17,0,-25);
  // Open streets continue for roughly three camera widths and two depths.
  foreach(float z in new[]{-18f,18f,38f})foreach(float x in new[]{-43f,-29f,29f,43f}) {
   kit.Place("House","Harbor townhouse",new Vector3(x,0,z),0,1.05f);
   obstacles.Add(new Rect(x-3.1f,z-2.7f,6.2f,5.4f));
  }
  foreach(float z in new[]{40f})foreach(float x in new[]{-12f,12f}) {
   kit.Place("House","Courtyard house",new Vector3(x,0,z));obstacles.Add(new Rect(x-3,z-2.5f,6,5));
  }
  foreach(float z in new[]{-16f,0f,20f,39f})foreach(float x in new[]{-36f,-21f,21f,36f}) {
   kit.Place("Tree","Olive tree",new Vector3(x,0,z),x+z,1.05f);obstacles.Add(new Rect(x-.7f,z-.7f,1.4f,1.4f));
   Box("Tree planter",new Vector3(x,.17f,z),new Vector3(1.6f,.34f,1.6f),wall,false);
  }
  kit.Place("Tree","Foreground olive",new Vector3(-9,0,-5),25,1.15f);obstacles.Add(new Rect(-9.7f,-5.7f,1.4f,1.4f));
  kit.Place("Tree","Market olive",new Vector3(16,0,13),60,1.15f);obstacles.Add(new Rect(15.3f,12.3f,1.4f,1.4f));
  for(int i=0;i<7;i++){Crates(new Vector3(-22+i*1.1f,0,-18));Barrel(new Vector3(9+i*.5f,0,-6),.25f);}
  foreach(var p in new[]{new Vector3(-4,1.83f,6.8f),new Vector3(9,0,-5.2f),new Vector3(-12,0,23.3f)})Lantern(p);
  var grass=Surface(new Color(.32f,.40f,.19f));
  var gardenStone=Surface(new Color(.64f,.63f,.51f));
  foreach(float z in new[]{18f,38f})foreach(float x in new[]{-36f,36f}) {
   Box("Courtyard garden",new Vector3(x,.025f,z),new Vector3(5,.05f,10),grass,false);
   foreach(int side in new[]{-1,1})Box("Garden border",new Vector3(x+side*2.5f,.11f,z),new Vector3(.18f,.22f,10),gardenStone,false);
   Box("Garden bench",new Vector3(x, .55f,z-3),new Vector3(2,.18f,.55f),wood,false);
   foreach(float dx in new[]{-.75f,.75f})Box("Bench leg",new Vector3(x+dx,.27f,z-3),new Vector3(.15f,.54f,.4f),metal,false);
   obstacles.Add(new Rect(x-1.2f,z-3.5f,2.4f,1));
  }
  Box("Civic fountain basin",new Vector3(12,.25f,25),new Vector3(4,.5f,4),gardenStone,false);
  Box("Fountain pool",new Vector3(12,.51f,25),new Vector3(3.6f,.02f,3.6f),Surface(new Color(.18f,.43f,.44f)),false);
  Box("Fountain column",new Vector3(12,1.1f,25),new Vector3(.65f,1.6f,.65f),gardenStone,false);
  Box("Fountain bowl",new Vector3(12,1.9f,25),new Vector3(1.6f,.2f,1.6f),gardenStone,false);
  obstacles.Add(new Rect(9.6f,22.6f,4.8f,4.8f));
  var hills=Surface(new Color(.31f,.42f,.36f));
  for(int i=0;i<9;i++) {
   var hill=GameObject.CreatePrimitive(PrimitiveType.Sphere);hill.name="Distant coastal hill";hill.layer=ArtLayer;hill.transform.SetParent(transform);hill.transform.position=new Vector3(-100+i*25,1,78+(i%2)*9);hill.transform.localScale=new Vector3(42,12+i%3*5,32);hill.GetComponent<Renderer>().sharedMaterial=hills;Destroy(hill.GetComponent<Collider>());
  }
 }
 void AddBuilding(string model,string name,Vector3 p,float width,float depth,string destination) {
  kit.Place(model,name,p);float front=p.z-depth/2;
  // Include awning supports and counters in the footprint, with walking clearance.
  obstacles.Add(new Rect(p.x-width/2-.4f,front-.4f,width+.8f,depth+.8f));
  Landmarks[destination]=new Vector3(p.x,p.y,front-1.05f);
 }
 void FollowCamera(float dt,bool immediate=false) {
  Vector3 target=Player.position+new Vector3(0,.7f,3.5f);
  CameraDistance=Mathf.Lerp(31,26,Mathf.InverseLerp(-20,44,Player.position.z));
  Vector3 desired=target-View.transform.forward*CameraDistance;
  View.transform.position=immediate?desired:Vector3.SmoothDamp(View.transform.position,desired,ref cameraVelocity,.24f,Mathf.Infinity,dt);
  if(walkerQuad!=null)walkerQuad.rotation=View.transform.rotation;
 }
 void KeyboardWalk(float dt) {
  float x=Input.GetAxisRaw("Horizontal"),z=Input.GetAxisRaw("Vertical");
  if(Mathf.Abs(x)+Mathf.Abs(z)<.1f)return;
  Vector3 delta=new Vector3(x,0,z).normalized*.85f;
  var next=grid.Snap(Player.position+delta);
  if(Vector3.Distance(next,Player.position+delta)<.7f){var localPath=grid.Path(Player.position,next);if(localPath.Count<=4){path=localPath;arrival="";}}
 }
}
}
