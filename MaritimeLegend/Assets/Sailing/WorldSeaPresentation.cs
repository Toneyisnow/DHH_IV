using System;
using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {public partial class MaritimeGame {
 StreamedSeaRuntime worldSea;GameState worldSeaOwner;
 bool IsWorldSea=>s!=null&&s.sailing.manualMapId=="regional_world_v2";
 void EnterWorldSea(){
  Save(true);if(worldSea==null)worldSea=new StreamedSeaRuntime(transform);
  g.EnterStreamedSailing(worldSea.Map);worldSeaOwner=null;EnsureWorldSea();
  Notify("已进入世界航海，船队位于出发港外海；原区域位置已保留，物资与进度不变。",8);Save(true);
 }
 void EnsureWorldSea(){
  if(worldSea==null)worldSea=new StreamedSeaRuntime(transform);
  if(!g.StreamedSailing){g.EnterStreamedSailing(worldSea.Map);worldSeaOwner=null;}
  g.WorldMapData=worldSea.Map;g.SeaNavigation=worldSea.Navigation;
  worldSea.SightRadius=(float)g.WorldVisionRadius;
  var settings=SailingScene.Active;if(settings)worldSea.ConfigureCamera(settings.minimumZoom,settings.maximumZoom,settings.pitch);
  if(indianOcean!=null)indianOcean.SetActive(false);
  if(worldSeaOwner!=s){
   if(s.sailing.worldCoordinateVersion!=2||!WorldMap.Finite(s.seaX)||!WorldMap.Finite(s.seaY)||s.seaX<0||s.seaX>=WorldMap.Width||s.seaY<0||s.seaY>WorldMap.Height)throw new InvalidOperationException("Invalid or unsupported world sailing coordinates; save retained");
   if(s.sailing.manualFollowers!=null){if(s.sailing.manualFollowers.Length!=2)throw new InvalidOperationException("Invalid world fleet");foreach(var f in s.sailing.manualFollowers)if(f==null||!WorldMap.Finite(f.x)||!WorldMap.Finite(f.y)||f.x<0||f.x>=WorldMap.Width||f.y<0||f.y>WorldMap.Height)throw new InvalidOperationException("Invalid saved world follower");}
   worldSeaOwner=s;seaStopped=true;seaHasTarget=false;seaTarget=SeaPosition;worldSea.SetView(s.sailing.manualYaw,s.sailing.manualZoom);
  }
  worldSea.Update(s);g.WorldStreamingBlocked=!worldSea.NavigationReady;seaCamera=SeaPosition;
 }
 void TickWorldSea(double seconds){
  worldSea.AdvanceWeather(s.sailing,s.wind,seconds,c.balance.navigation.windSpeed*4*c.balance.navigation.minSailSpeed*.02);
  worldSea.turnRate=c.balance.sailing.turnDegreesPerSecond;
  if(!LiveGui.PointerOverControl&&!OverSailingInstruments(LiveGui.Mouse)){worldSea.Orbit(Input.GetMouseButton(1)?Input.GetAxis("Mouse X"):0,Input.mouseScrollDelta.y);s.sailing.manualYaw=worldSea.Yaw;s.sailing.manualZoom=worldSea.Zoom;}
  worldSea.Update(s);g.WorldStreamingBlocked=!worldSea.NavigationReady;if(g.WorldStreamingBlocked){s.sailing.speed=0;return;}
  for(double remaining=seconds;remaining>1e-9;){double step=Math.Min(remaining,1.0/60);remaining-=step;
   double targetX=WorldMap.DeltaX(s.seaX,seaTarget.x)/960,targetY=(seaTarget.y-s.seaY)/540,distance=Math.Sqrt(targetX*targetX+targetY*targetY);
   if(seaHasTarget&&!seaStopped){if(distance<Math.Max(.0005,s.sailing.speed*step*1.5)){seaHasTarget=false;}else g.SteerToward(seaTarget.x,seaTarget.y,step);}
   double oldX=s.seaX,oldY=s.seaY;
   double speed=g.Sail(step,seaStopped?0:throttle);
   double dx=WorldMap.DeltaX(oldX,s.seaX)/960*CoastNavigation.Width,dy=(s.seaY-oldY)/540*CoastNavigation.Height;
   worldSea.StepFollowers(s,step,Math.Sqrt(dx*dx+dy*dy)/step);
   double afterX=WorldMap.DeltaX(s.seaX,seaTarget.x)/960,afterY=(seaTarget.y-s.seaY)/540;
   if(seaHasTarget&&(Math.Sqrt(afterX*afterX+afterY*afterY)<Math.Max(.0005,speed*step*1.5)||targetX*afterX+targetY*afterY<=0))seaHasTarget=false;
   if(g.WorldStreamingBlocked)break;
  }
  worldSea.Center(s.seaX,s.seaY);seaCamera=SeaPosition;
 }
 void WorldSeaBackdrop(){
  worldSea.Render(s,throttle);GUI.DrawTexture(new Rect(0,0,1920,1080),worldSea.Image,ScaleMode.StretchToFill);
  if(seaHasTarget){var p=worldSea.Project(seaTarget);if(p.x>20&&p.x<1650&&p.y>190&&p.y<930){
   for(int k=0;k<24;k++){float a=k*Mathf.PI/12,b=(k+1)*Mathf.PI/12;Line(p+new Vector2(Mathf.Cos(a)*16,Mathf.Sin(a)*8),p+new Vector2(Mathf.Cos(b)*16,Mathf.Sin(b)*8),gold,2);}Label(p.x+20,p.y-20,150,35,"航行目标",small);
  }}
 }
 void WorldSailingView(){
  var v=s.sailing;var sh=s.ships[0];var nav=worldSea.Navigation;
  Fill(new Rect(32,28,530,100),new Color(.025f,.09f,.13f,.8f));Label(54,38,500,40,"世界地图 · 手动航海",heading);
  Label(54,84,500,30,GameCalendar.Date(v.calendar.elapsedGameDays)+" · "+(s.seaX/32-180).ToString("0.00")+"° 经度 / "+(s.seaY/32-80).ToString("0.00")+"° 纬度",small);
  if(Btn(590,32,170,54,"Sunny"))worldSea.SetSkyMode(ManualSeaV2Environment.SkyMode.Sunny);
  if(Btn(775,32,170,54,"Sunset"))worldSea.SetSkyMode(ManualSeaV2Environment.SkyMode.Sunset);
  if(Btn(960,32,170,54,"Night"))worldSea.SetSkyMode(ManualSeaV2Environment.SkyMode.Night);
  if(Btn(1680,32,208,54,"暂停 / ESC"))paused=true;
  if(Btn(1430,100,458,50,"返回出发水域 · "+v.manualSpawnPort)){seaTarget=new Vector2(v.manualSpawnX,v.manualSpawnY);seaHasTarget=true;seaStopped=false;}
  string status=g.WorldStreamingBlocked?"海域加载中":nav.LatitudeWall!=0?(nav.LatitudeWall>0?"已到达北纬80°航行边界":"已到达南纬80°航行边界"):seaStopped?"已停船":nav.Contact?(nav.Sliding?"沿岸慢行":"海岸阻挡"):"航行中";
  Fill(new Rect(30,958,1860,94),new Color(.025f,.09f,.13f,.88f));
  Label(52,971,1020,34,status+" · 航速 "+v.speed.ToString("0.0000")+" · 帆 "+(throttle*100).ToString("0")+"% ×"+SailingFormulas.SailPower(throttle,s.heading,s.trim,s.wind).ToString("0.00")+" · 船体 "+sh.hull.ToString("0")+" · 粮 "+sh.food.ToString("0.0")+" · 水 "+sh.water.ToString("0.0"),small);
  Label(52,1011,1050,32,"左键航行 · 右键水平环绕 · 滚轮缩放 · 空格停船 · W/S 收放帆 · A/D 调帆",small);
  if(Btn(1080,976,190,56,seaStopped?"继续航行":"停船"))seaStopped=!seaStopped;
  WorldPort nearest=null;double best=WorldMap.PortRadius;foreach(var p in worldSea.Map.ports){double d=WorldMap.Distance(s.seaX,s.seaY,p.x,p.y);if(d<=best){best=d;nearest=p;}}
  if(Btn(1290,976,280,56,nearest==null?"靠近港口后入港":"入港 · "+nearest.name,nearest!=null&&!g.WorldStreamingBlocked)){
   if(nearest.id==s.seaFrom){s.city=s.seaFrom;s.atSea=false;v.speed=0;g.SetSeaRegion(c.sailingDefaults.portRegions[s.city]);Change("城市");}
   else{s.seaTo=nearest.id;Notify(g.Arrive());if(!s.atSea)Change("城市");}
  }
  if(Btn(1590,976,270,56,"紧急返港救援")){Notify(g.Rescue(),10);Change("城市");}
  Label(40,914,1500,34,worldSea.Diagnostic+" · 东西相通 / 南北纬80°止航 · 出发港 "+v.manualSpawnPort,small);
  SailingInstruments();SailingCompass();SailingPortTooltip();
 }
}}

