using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
public partial class MaritimeGame {
 ManualSeaV2Runtime indianOcean; CoastNavigation seaCoast, followerCoast; float seaAnimationClock;
 GameState seaOwner; int seaDeparture=-1; Vector2 seaCamera,seaTarget; bool seaHasTarget,seaStopped=true;
 internal Vector2 SeaCamera=>seaCamera;
 internal bool HasManualSeaTarget=>seaHasTarget;
 internal Vector2 SeaPosition=>new Vector2((float)s.seaX,(float)s.seaY);
 void OnDestroy(){DisposeLanguages();if(indianOcean!=null)indianOcean.Dispose();}
 internal void EnsureManualSea() {
  if(indianOcean==null){
   indianOcean=new ManualSeaV2Runtime(transform);
   var coastBytes=Resources.Load<TextAsset>("Environment/SoutheastAsiaV2/coast-distance").bytes;
   seaCoast=new CoastNavigation(coastBytes,9);followerCoast=new CoastNavigation(coastBytes,9);
  }

  var cameraSettings=SailingScene.Active;if(cameraSettings)indianOcean.ConfigureCamera(cameraSettings.minimumZoom,cameraSettings.maximumZoom,cameraSettings.pitch);
  if(seaOwner!=s||seaDeparture!=s.seaFrom){
   if(s.sailing.manualMapId!="southeast_asia_v2"){
    if(!string.IsNullOrEmpty(s.sailing.manualMapId)){
     s.sailing.previousManualMapId=s.sailing.manualMapId;s.sailing.previousManualX=s.seaX;s.sailing.previousManualY=s.seaY;
     Notify("手动航海已更新为东南亚 V2，船队移至安全出发水域；航程与物资保留。");
    }
    var spawn=new Vector2(.5f-95f/960,.5f-70f/540);
    for(int attempt=0;seaCoast.Distance(seaCoast.Pixels(spawn))<18&&attempt<400;attempt++){
     float a=attempt*2.399f,r=2+attempt*.2f;spawn=new Vector2(.5f+(-95+Mathf.Cos(a)*r)/960,.5f-(70+Mathf.Sin(a)*r)/540);
    }
    if(seaCoast.Distance(seaCoast.Pixels(spawn))<18)throw new System.InvalidOperationException("No safe Southeast Asia anchorage");
    s.sailing.manualMapId="southeast_asia_v2";s.sailing.manualSpawnPort="南海锚地";s.sailing.manualSpawnX=spawn.x;s.sailing.manualSpawnY=spawn.y;
    s.sailing.manualYaw=0;s.sailing.manualZoom=120;s.seaX=spawn.x;s.seaY=spawn.y;s.heading=0;
    s.sailing.manualFollowers=null;g.SetSeaRegion("southeast_asia");
   }
   instrumentWheelAngle=0;seaOwner=s;seaDeparture=s.seaFrom;seaCamera=SeaPosition;seaTarget=SeaPosition;seaHasTarget=false;seaStopped=true;seaCoast.Origin=Vector2.zero;
   indianOcean.SetView(s.sailing.manualYaw,s.sailing.manualZoom);s.sailing.manualZoom=indianOcean.Zoom;indianOcean.Center(SeaPosition);
  }
  if(s.sailing.manualFollowers==null||s.sailing.manualFollowers.Length!=2)
  {
   var forward=SeaInstrumentMath.WorldDirection(s.heading);s.sailing.manualFollowers=SeaFleetFollowing.Spawn(SeaPosition,Mathf.Atan2(forward.y,forward.x));
   Vector2 leader=seaCoast.Pixels(SeaPosition);
   foreach(var follower in s.sailing.manualFollowers){
    Vector2 candidate=followerCoast.MovePixels(leader,-forward*24);
    for(int k=0;Vector2.Distance(candidate,leader)<18&&k<12;k++){
     float angle=k*Mathf.PI/6;candidate=followerCoast.MovePixels(leader,new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*24);
    }
    var uv=followerCoast.UV(candidate);follower.x=uv.x;follower.y=uv.y;leader=candidate;
   }
  }
  indianOcean.SightRadius=Mathf.Clamp((float)g.ManualVisionRadius*960,12,480);
  g.SeaNavigation=seaCoast;
 }
 internal bool SetManualSeaTarget(Vector2 pointer) {
  EnsureManualSea();if(pointer.y<190||pointer.y>930||pointer.x<0||pointer.x>1920||OverSailingInstruments(pointer))return false;
  if(!indianOcean.TryUnproject(pointer,out seaTarget))return false;seaTarget.x=Mathf.Clamp(seaTarget.x,.001f,.999f);seaTarget.y=Mathf.Clamp(seaTarget.y,.001f,.999f);
  seaHasTarget=true;seaStopped=false;return true;
 }
 internal void TickManualSea(double seconds) {
  EnsureManualSea();seaAnimationClock+=(float)seconds;
  indianOcean.AdvanceWeather(s.sailing,s.wind,seconds,c.balance.navigation.windSpeed*4*c.balance.navigation.minSailSpeed*.02);
  if(!LiveGui.PointerOverControl&&!OverSailingInstruments(LiveGui.Mouse)){indianOcean.Orbit(Input.GetMouseButton(1)?Input.GetAxis("Mouse X"):0,Input.mouseScrollDelta.y);s.sailing.manualYaw=indianOcean.Yaw;s.sailing.manualZoom=indianOcean.Zoom;}
  // Simulate the whole chain in bounded steps, independently of GUI repaint frequency.
  for(double remaining=seconds;remaining>1e-9;) {
   double step=System.Math.Min(remaining,1.0/60);remaining-=step;
   if(seaHasTarget&&!seaStopped){var distance=Vector2.Distance(SeaPosition,seaTarget);if(distance<Mathf.Max(.0005f,(float)(s.sailing.speed*step*1.5)))seaHasTarget=false;else g.SteerToward(seaTarget.x,seaTarget.y,step);}
   var before=SeaPosition;double speed=g.Sail(step,seaStopped?0:throttle);
   float pixelsPerSecond=seaCoast.Pixels(SeaPosition-before).magnitude/(float)step;
   SeaFleetFollowing.Step(s.sailing.manualFollowers,SeaPosition,pixelsPerSecond,step,c.balance.sailing.turnDegreesPerSecond,followerCoast,18,24);
   if(seaHasTarget&&(Vector2.Distance(SeaPosition,seaTarget)<Mathf.Max(.0005f,(float)(speed*step*1.5))||Vector2.Dot(seaTarget-before,seaTarget-SeaPosition)<=0))seaHasTarget=false;
  }
  indianOcean.Follow(SeaPosition);indianOcean.Reveal(s.sailing,SeaPosition);seaCamera=SeaPosition;
 }
 void ManualSeaBackdrop() {
  EnsureManualSea();indianOcean.Render(s,throttle);
  GUI.DrawTexture(new Rect(0,0,1920,1080),indianOcean.Image,ScaleMode.StretchToFill);
  if(seaHasTarget){var p=indianOcean.Project(seaTarget);if(p.y>190&&p.y<930&&p.x>20&&p.x<1900){
   float radius=18+Mathf.Sin(Time.time*3)*3;for(int k=0;k<24;k++){float a=k*Mathf.PI/12,b=(k+1)*Mathf.PI/12;Line(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.5f)*radius,p+new Vector2(Mathf.Cos(b),Mathf.Sin(b)*.5f)*radius,gold,2);}Label(p.x+24,p.y-20,140,35,"航行目标",small);}}
 }

 internal Vector2 ProjectManualSea(Vector2 point)=>indianOcean.Project(point);
 internal float ManualSeaZoom=>indianOcean.Zoom;
 internal float ManualSeaPitch=>indianOcean.Pitch;
 internal Vector2 UnprojectManualSea(Vector2 point)=>indianOcean.Unproject(point);
 internal void SetIndianReview(Vector2 center,float yaw,float zoom){EnsureManualSea();s.seaX=center.x;s.seaY=center.y;s.sailing.manualFollowers=SeaFleetFollowing.Spawn(center,0);seaStopped=true;seaHasTarget=false;indianOcean.SetView(yaw,zoom);indianOcean.Center(center);}
 internal void CaptureIndianMap(string path)=>indianOcean.Capture(path);
}
}
