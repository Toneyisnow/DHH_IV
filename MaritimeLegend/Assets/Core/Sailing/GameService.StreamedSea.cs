using System;
namespace MaritimeLegend.Domain {
public interface IStreamedSeaNavigation:ISeaNavigationPolicy {bool Loading {get;}}
public partial class GameService {
 public bool StreamedSailing=>S.sailing.manualMapId=="regional_world_v2";
 public void EnterStreamedSailing(WorldMapManifest map){
  if(StreamedSailing){WorldMapData=map;return;}
  if(WorldSailing){S.sailing.suspendedWorldVoyage=new PreviousManualVoyage{mapId=S.sailing.manualMapId,x=S.seaX,y=S.seaY,spawnX=S.sailing.manualSpawnX,spawnY=S.sailing.manualSpawnY,spawnPort=S.sailing.manualSpawnPort,followers=S.sailing.manualFollowers};S.sailing.manualMapId="";}
  EnterWorldSailing(map);S.sailing.manualMapId="regional_world_v2";S.sailing.worldCoordinateVersion=2;S.sailing.manualZoom=10;
 }
 double SailStreamed(double dt,double throttle){
  if(WorldStreamingBlocked||!(SeaNavigation is IStreamedSeaNavigation nav)){S.sailing.speed=0;return 0;}
  double speed=C.balance.navigation.windSpeed*4*SkillResolver.Linear(S.skillLevels[C.balance.skills.navigationSkill],C.skills[C.balance.skills.navigationSkill].maxLevel,C.balance.navigation.minSailSpeed,C.balance.navigation.maxSailSpeed)*SailingFormulas.SailPower(throttle,S.heading,S.trim,S.wind);
  double x=S.seaX,y=S.seaY;nav.ResolveMove(x,y,x+Math.Cos(S.heading)*speed*960*dt,y-Math.Sin(S.heading)*speed*540*dt,out var nx,out var ny);
  S.seaX=nx;S.seaY=ny;double dx=WorldMap.DeltaX(x,nx)/960,dy=(ny-y)/540,d=Math.Sqrt(dx*dx+dy*dy);S.sailing.lastDayDistance+=d;S.sailing.speed=dt>0?d/dt:0;
  if(nav.Loading)WorldStreamingBlocked=true;SetSeaRegion(WorldMap.Region(nx,ny));return S.sailing.speed;
 }
}
}
