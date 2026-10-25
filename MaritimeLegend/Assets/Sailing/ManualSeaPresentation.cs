using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
public partial class MaritimeGame {
 IndianOceanRenderer indianOcean; CoastNavigation seaCoast, followerCoast; float seaAnimationClock; Material seaMaterial;
 readonly Texture2D[,] fleetArt=new Texture2D[3,8];
 GameState seaOwner; int seaDeparture=-1; Vector2 seaCamera,seaTarget; bool seaHasTarget,seaStopped=true;
 static readonly string[] SeaDirections={"E","SE","S","SW","W","NW","N","NE"};
 internal Vector2 SeaCamera=>seaCamera;
 internal bool HasManualSeaTarget=>seaHasTarget;
 internal Vector2 SeaPosition=>new Vector2((float)s.seaX,(float)s.seaY);
 void OnDestroy(){DisposeLanguages();if(indianOcean!=null)indianOcean.Dispose();if(seaMaterial!=null)Destroy(seaMaterial);}
 internal void EnsureManualSea() {
  if(indianOcean==null){
   indianOcean=new IndianOceanRenderer(transform);seaMaterial=new Material(Resources.Load<Shader>("Shaders/SeaAtmosphere"));
   var coastBytes=Resources.Load<TextAsset>("Environment/IndianOcean/coast-distance").bytes;
   seaCoast=new CoastNavigation(coastBytes);followerCoast=new CoastNavigation(coastBytes);
   string[] types={"square","junk","lateen"};for(int t=0;t<3;t++)for(int d=0;d<8;d++)fleetArt[t,d]=Resources.Load<Texture2D>("Sprites/ManualSailing/"+types[t]+"/"+SeaDirections[d]);
  }
  var cameraSettings=SailingScene.Active;if(cameraSettings)indianOcean.ConfigureCamera(cameraSettings.minimumZoom,cameraSettings.maximumZoom,cameraSettings.pitch);
  if(seaOwner!=s||seaDeparture!=s.seaFrom){
   if(s.sailing.manualMapId!="indian_ocean"){
    var port=indianOcean.Data.ports[Random.Range(0,indianOcean.Data.ports.Length)];
    s.sailing.manualMapId="indian_ocean";s.sailing.manualSpawnPort=port.name;s.sailing.manualSpawnX=port.x;s.sailing.manualSpawnY=port.y;
    s.sailing.manualYaw=0;s.sailing.manualZoom=70;s.seaX=port.x;s.seaY=port.y;
    s.sailing.manualFollowers=null;
    s.heading=Mathf.Atan2(port.y+port.landZ/941,port.x-port.landX/1672);g.SetSeaRegion("indian_ocean");
   }
   instrumentWheelAngle=0;seaOwner=s;seaDeparture=s.seaFrom;seaCamera=SeaPosition;seaTarget=SeaPosition;seaHasTarget=false;seaStopped=true;seaCoast.Origin=Vector2.zero;
   indianOcean.SetView(s.sailing.manualYaw,s.sailing.manualZoom);indianOcean.Center(SeaPosition);
  }
  if(s.sailing.manualFollowers==null||s.sailing.manualFollowers.Length!=2)
   s.sailing.manualFollowers=SeaFleetFollowing.Spawn(SeaPosition,Mathf.Atan2(SeaInstrumentMath.WorldDirection(s.heading).y,SeaInstrumentMath.WorldDirection(s.heading).x));
  g.SeaNavigation=seaCoast;
 }
 internal bool SetManualSeaTarget(Vector2 pointer) {
  EnsureManualSea();if(pointer.y<190||pointer.y>930||pointer.x<0||pointer.x>1920||OverSailingInstruments(pointer))return false;
  seaTarget=indianOcean.Unproject(pointer);seaTarget.x=Mathf.Clamp(seaTarget.x,.001f,.999f);seaTarget.y=Mathf.Clamp(seaTarget.y,.001f,.999f);
  seaHasTarget=true;seaStopped=false;return true;
 }
 internal void TickManualSea(double seconds) {
  EnsureManualSea();seaAnimationClock+=(float)seconds;
  if(!LiveGui.PointerOverControl&&!OverSailingInstruments(LiveGui.Mouse)){indianOcean.Orbit(Input.GetMouseButton(1)?Input.GetAxis("Mouse X"):0,Input.mouseScrollDelta.y);s.sailing.manualYaw=indianOcean.Yaw;s.sailing.manualZoom=indianOcean.Zoom;}
  // Simulate the whole chain in bounded steps, independently of GUI repaint frequency.
  for(double remaining=seconds;remaining>1e-9;) {
   double step=System.Math.Min(remaining,1.0/60);remaining-=step;
   if(seaHasTarget&&!seaStopped){var distance=Vector2.Distance(SeaPosition,seaTarget);if(distance<Mathf.Max(.0005f,(float)(s.sailing.speed*step*1.5)))seaHasTarget=false;else g.SteerToward(seaTarget.x,seaTarget.y,step);}
   var before=SeaPosition;double speed=g.Sail(step,seaStopped?0:throttle);
   float pixelsPerSecond=seaCoast.Pixels(SeaPosition-before).magnitude/(float)step;
   SeaFleetFollowing.Step(s.sailing.manualFollowers,SeaPosition,pixelsPerSecond,step,c.balance.sailing.turnDegreesPerSecond,followerCoast);
   if(seaHasTarget&&(Vector2.Distance(SeaPosition,seaTarget)<Mathf.Max(.0005f,(float)(speed*step*1.5))||Vector2.Dot(seaTarget-before,seaTarget-SeaPosition)<=0))seaHasTarget=false;
  }
  indianOcean.Follow(SeaPosition);seaCamera=SeaPosition;
 }
 void ManualSeaBackdrop() {
  EnsureManualSea();indianOcean.Render(SeaPosition);
  GUI.DrawTexture(new Rect(0,0,1920,1080),indianOcean.Image,ScaleMode.StretchToFill,seaMaterial);
  foreach(var port in indianOcean.Data.ports){var p=indianOcean.Project(new Vector2(port.landX/1672,-port.landZ/941));if(p.x<20||p.x>1900||p.y<195||p.y>900)continue;
   float r=Mathf.Clamp(520/indianOcean.Zoom,3,9);Fill(new Rect(p.x-r,p.y-r,r*2,r*2),gold);Label(p.x+12,p.y-17,220,35,port.name,small);
  }
  if(seaHasTarget){var p=indianOcean.Project(seaTarget);if(p.y>190&&p.y<930&&p.x>20&&p.x<1900){
   float radius=18+Mathf.Sin(Time.time*3)*3;for(int k=0;k<24;k++){float a=k*Mathf.PI/12,b=(k+1)*Mathf.PI/12;Line(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.5f)*radius,p+new Vector2(Mathf.Cos(b),Mathf.Sin(b)*.5f)*radius,gold,2);}Label(p.x+24,p.y-20,140,35,"航行目标",small);}}
  var positions=new[]{SeaPosition,new Vector2((float)s.sailing.manualFollowers[0].x,(float)s.sailing.manualFollowers[0].y),new Vector2((float)s.sailing.manualFollowers[1].x,(float)s.sailing.manualFollowers[1].y)};
  var points=new Vector2[3];for(int i=0;i<3;i++)points[i]=indianOcean.Project(positions[i]);
  int[] order={0,1,2};System.Array.Sort(order,(a,b)=>points[a].y.CompareTo(points[b].y));
  foreach(int i in order){
   var forward=i==0?SeaInstrumentMath.WorldDirection(s.heading):new Vector2((float)System.Math.Cos(s.sailing.manualFollowers[i-1].heading),(float)System.Math.Sin(s.sailing.manualFollowers[i-1].heading));
   var uvDirection=new Vector2(forward.x/1672,forward.y/941);
   var screenDirection=(indianOcean.Project(positions[i]+uvDirection)-points[i]).normalized;
   int direction=((int)Mathf.Round(Mathf.Atan2(screenDirection.y,screenDirection.x)*Mathf.Rad2Deg/45)+8)%8;
   var p=points[i];float size=Mathf.Clamp(4400/indianOcean.Zoom,18,90);var tail=-screenDirection;
   for(int w=0;w<3;w++){var v=p+tail*(size*.25f+w*size*.12f);var cross=new Vector2(-tail.y,tail.x)*(size*.12f+w*size*.04f);Line(v-cross,v+cross,new Color(.7f,.94f,1,.25f-w*.06f),2);}
   GUI.DrawTexture(new Rect(p.x-size*.55f,p.y-size*.88f+SeaBobbing.Offset(size,seaAnimationClock,i),size*1.1f,size),fleetArt[i,direction],ScaleMode.ScaleToFit);
  }
 }
 internal Vector2 ProjectManualSea(Vector2 point)=>indianOcean.Project(point);
 internal float ManualSeaZoom=>indianOcean.Zoom;
 internal float ManualSeaPitch=>indianOcean.Pitch;
 internal Vector2 UnprojectManualSea(Vector2 point)=>indianOcean.Unproject(point);
 internal void SetIndianReview(Vector2 center,float yaw,float zoom){EnsureManualSea();s.seaX=center.x;s.seaY=center.y;s.sailing.manualFollowers=SeaFleetFollowing.Spawn(center,0);seaStopped=true;seaHasTarget=false;indianOcean.SetView(yaw,zoom);indianOcean.Center(center);}
 internal void CaptureIndianMap(string path)=>indianOcean.Capture(path);
}
}
