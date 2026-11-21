using System;
namespace MaritimeLegend.Domain {
public partial class GameService {
 public bool WorldSailing=>S.sailing.manualMapId==WorldMap.Id||StreamedSailing;
 // Retire the rejected presentation without interpreting world metres as regional UVs.
 public bool RestoreRegionalSailing(){
  var v=S.sailing;v.preferWorldMap=false;if(!WorldSailing)return false;
  v.suspendedWorldVoyage=new PreviousManualVoyage{mapId=v.manualMapId,x=S.seaX,y=S.seaY,spawnX=v.manualSpawnX,spawnY=v.manualSpawnY,spawnPort=v.manualSpawnPort,followers=v.manualFollowers};
  var old=v.previousManualVoyage;
  v.manualMapId=old!=null&&old.mapId=="southeast_asia_v2"?old.mapId:"";
  if(v.manualMapId!=""){S.seaX=old.x;S.seaY=old.y;v.manualSpawnX=old.spawnX;v.manualSpawnY=old.spawnY;v.manualSpawnPort=old.spawnPort;v.manualFollowers=old.followers;}
  else {v.manualFollowers=null;v.manualSpawnPort="";}
  v.manualYaw=0;v.manualZoom=10;v.speed=0;WorldStreamingBlocked=false;SeaNavigation=null;
  return true;
 }
 public bool WorldStreamingBlocked {get;set;}
 public WorldMapManifest WorldMapData {get;set;}
 public double WorldVisionRadius=>Math.Max(12,Math.Min(480,ManualVisionRadius*WorldMap.LegacySpeedScale));
 bool PortInManualVision(int id,double legacyVision){
  if(WorldSailing){if(WorldMapData==null)return false;var p=WorldMapData.Port(id);return WorldMap.Distance(S.seaX,S.seaY,p.x,p.y)<WorldVisionRadius;}
  if(S.sailing.manualMapId=="indian_ocean"||S.sailing.manualMapId=="southeast_asia_v2")return false;
  var old=C.ports[id];return Math.Sqrt(Math.Pow(old.x-S.seaX,2)+Math.Pow(old.y-S.seaY,2))<legacyVision;
 }
 public void EnterWorldSailing(WorldMapManifest map){
  map.Validate();WorldMapData=map;
  if(WorldSailing){if(S.sailing.worldCoordinateVersion!=WorldMap.Version)throw new InvalidOperationException("Unsupported world sailing save");return;}
  if(DialogueOpen)throw new InvalidOperationException("Finish the current dialogue before changing map");
  var port=map.Port(S.seaFrom);var v=S.sailing;
  if(!string.IsNullOrEmpty(v.manualMapId))v.previousManualVoyage=new PreviousManualVoyage{mapId=v.manualMapId,x=S.seaX,y=S.seaY,spawnX=v.manualSpawnX,spawnY=v.manualSpawnY,spawnPort=v.manualSpawnPort,followers=v.manualFollowers};
  v.previousManualMapId=v.manualMapId;v.previousManualX=S.seaX;v.previousManualY=S.seaY;
  v.manualMapId=WorldMap.Id;v.worldCoordinateVersion=WorldMap.Version;v.preferWorldMap=true;
  v.manualSpawnPort=port.name;v.manualSpawnX=(float)port.x;v.manualSpawnY=(float)port.y;
  S.seaX=port.x;S.seaY=port.y;v.manualFollowers=null;v.manualYaw=0;v.manualZoom=85;v.speed=0;
  SetSeaRegion(WorldMap.Region(port.x,port.y));
 }
 double SailWorld(double dt,double throttle){
  if(StreamedSailing)return SailStreamed(dt,throttle);
  if(WorldStreamingBlocked||!(SeaNavigation is WorldNavigation nav)){S.sailing.speed=0;return 0;}
  double speed=C.balance.navigation.windSpeed*4*SkillResolver.Linear(S.skillLevels[C.balance.skills.navigationSkill],C.skills[C.balance.skills.navigationSkill].maxLevel,C.balance.navigation.minSailSpeed,C.balance.navigation.maxSailSpeed)*SailingFormulas.SailPower(throttle,S.heading,S.trim,S.wind)*WorldMap.LegacySpeedScale;
  double total=0;
  for(double left=dt;left>1e-9;){double step=Math.Min(left,1.0/60);left-=step;
   nav.ResolveMove(S.seaX,S.seaY,S.seaX+Math.Cos(S.heading)*speed*step,S.seaY-Math.Sin(S.heading)*speed*step,out double x,out double y);
   S.seaX=x;S.seaY=y;total+=nav.Travel;if(nav.Loading){WorldStreamingBlocked=true;break;}
  }
  S.sailing.speed=dt>0?total/dt:0;
  // Keep daily experience thresholds in the existing economy's distance unit.
  S.sailing.lastDayDistance+=total/WorldMap.LegacySpeedScale;
  SetSeaRegion(WorldMap.Region(S.seaX,S.seaY));return S.sailing.speed;
 }
}
}
