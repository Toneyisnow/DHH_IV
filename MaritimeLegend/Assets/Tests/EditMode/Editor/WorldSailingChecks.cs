#if UNITY_EDITOR || MANUAL_SAILING_CHECKS
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
public static class WorldSailingChecks {
 static int checks;
 static void Check(bool ok,string message){checks++;if(!ok)throw new Exception("WORLD: "+message);}
 static void Near(double a,double b,string message)=>Check(Math.Abs(a-b)<1e-6,message+" "+a+" != "+b);
 sealed class Field:IWorldTerrain {public bool known=true;public Func<double,double,double> sample=(x,y)=>80;public bool TryDistance(double x,double y,out double d){d=sample(x,y);return known;}}
 sealed class DiskField:IWorldTerrain {
  public readonly WorldMapManifest map;readonly Dictionary<int,WorldChunkData> chunks=new Dictionary<int,WorldChunkData>();
  public DiskField(WorldMapManifest map){this.map=map;}
  public WorldChunkData Data(int key){if(map.chunks[key].ocean)return null;if(!chunks.TryGetValue(key,out var d)){d=WorldChunkData.Parse(File.ReadAllBytes(Path.Combine(Application.dataPath,"Resources",map.chunks[key].key+".bytes")),key%72,key/72);chunks.Add(key,d);}return d;}
  public bool TryDistance(double x,double y,out double value){var d=Data(WorldMap.Key(x,y));value=d==null?80:d.Distance(x,y);return true;}
 }
 [UnityEditor.MenuItem("Maritime Legend/World Sailing/Run world checks")]
 public static void Run(){
  checks=0;Near(WorldMap.Wrap(-1),WorldMap.Width-1,"negative wrap");Near(WorldMap.Wrap(WorldMap.Width*5+3),3,"multiple laps");
  Near(WorldMap.DeltaX(WorldMap.Width-2,2),4,"east shortest path");Near(WorldMap.DeltaX(2,WorldMap.Width-2),-4,"west shortest path");Check(WorldMap.Row(WorldMap.Height)==31,"north edge belongs to final row");
  var field=new Field();var nav=new WorldNavigation(field);
  nav.ResolveMove(WorldMap.Width-.25,200,WorldMap.Width+.75,200,out var x,out var y);Near(x,.75,"east crossing");Near(nav.Travel,1,"no whole-world mileage");
  nav.ResolveMove(.25,200,-.75,200,out x,out y);Near(x,WorldMap.Width-.75,"west crossing");
  double total=0;x=12;y=100;
  for(int i=0;i<3456;i++){nav.ResolveMove(x,y,x+10,y,out x,out y);total+=nav.Travel;}Near(total,WorldMap.Width*3,"three complete laps");Near(x,12,"three laps return to origin");
  nav.ResolveMove(100,WorldMap.Height,101,WorldMap.Height+4,out x,out y);Near(x,101,"north tangent");Near(y,WorldMap.Height,"north air wall");Near(nav.Travel,1,"wall only counts tangent");Check(nav.LatitudeWall==1,"north contact");
  nav.ResolveMove(.2,0,-.8,-3,out x,out y);Near(x,WorldMap.Width-.8,"seam and south corner");Near(y,0,"south wall");Check(nav.LatitudeWall==-1,"south contact");
  nav.ResolveMove(x,0,x,2,out x,out y);Near(y,2,"leave wall immediately");
  field.known=false;nav.ResolveMove(100,100,110,100,out x,out y);Near(x,100,"unknown terrain blocks");Check(nav.Loading,"unknown loading status");field.known=true;
  field.sample=(xx,yy)=>Math.Abs(xx-100)-4;nav.ResolveMove(90,100,110,100,out x,out y);Check(x<=93.001&&nav.Contact,"sweep cannot tunnel through narrow land");
  var map=JsonUtility.FromJson<WorldMapManifest>(File.ReadAllText(Path.Combine(Application.dataPath,"Resources/Data/WorldMap/world.json")));map.Validate();var disk=new DiskField(map);
  foreach(var p in map.ports){disk.TryDistance(p.x,p.y,out var d);Check(d>=WorldMap.Clearance,"safe world berth "+p.name);}
  for(int cy=0;cy<32;cy++)for(int cx=0;cx<72;cx++){
   var a=disk.Data(cy*72+cx);var b=disk.Data(cy*72+(cx+1)%72);
   for(int i=0;i<=64;i++)Near(a==null?80:a.At(64,i),b==null?80:b.At(0,i),"horizontal shared edge");
   if(cy==31)continue;var c=disk.Data((cy+1)*72+cx);
   for(int i=0;i<=64;i++)Near(a==null?80:a.At(i,64),c==null?80:c.At(i,0),"vertical shared edge");
  }
  var catalog=CatalogLoader.Load();var s=GameService.New(catalog,0,0,"休闲",StateFactory.Appearance(catalog),"World checks",42);s.pendingEvents.Clear();var game=new GameService(catalog,s);game.Depart(1);
  s.sailing.manualMapId="southeast_asia_v2";s.seaX=.23;s.seaY=.45;s.sailing.manualFollowers=new[]{new ManualFollowerState{x=.2,y=.4},new ManualFollowerState{x=.19,y=.4}};
  var gold=s.gold;var days=s.sailing.calendar.elapsedGameDays;var food=s.ships[0].food;
  game.EnterWorldSailing(map);Check(game.WorldSailing,"world entry");Near(s.sailing.previousManualVoyage.x,.23,"legacy coordinates retained");Check(s.sailing.previousManualVoyage.followers.Length==2,"legacy followers retained");Near(s.gold,gold,"money preserved");Near(s.ships[0].food,food,"supplies preserved");Check(s.sailing.calendar.elapsedGameDays==days,"calendar preserved");
  double sx=s.seaX;game.EnterWorldSailing(map);Near(s.seaX,sx,"entry idempotent");
  field.sample=(xx,yy)=>80;game.SeaNavigation=new WorldNavigation(field);s.seaX=WorldMap.Width-.01;s.seaY=200;s.heading=0;s.wind=0;s.trim=.5;
  double before=s.sailing.lastDayDistance;game.Sail(.1,1);Check(s.seaX<10,"service wraps east");Check(s.sailing.lastDayDistance-before<.1,"experience uses legacy units");
  s.seaX=WorldMap.Width-2;s.seaY=200;s.heading=0;game.SteerToward(2,200,.1);Near(s.heading,0,"target across seam points east");
  s.seaX=200;s.seaY=WorldMap.Height;s.heading=-Math.PI/2;s.wind=-Math.PI/2;s.trim=.5;before=s.sailing.lastDayDistance;game.Sail(.1,1);Near(s.seaY,WorldMap.Height,"service wall");Near(s.sailing.lastDayDistance,before,"no wall experience");
  game.WorldStreamingBlocked=true;double calendar=s.sailing.calendar.secondsIntoDay;game.Workflow.Advance(2,false);Near(s.sailing.calendar.secondsIntoDay,calendar,"loading freezes calendar");
  game.Rescue();game.Workflow.Advance(2,false);Check(s.sailing.calendar.secondsIntoDay!=calendar,"rescue releases streaming clock gate");
  game.WorldStreamingBlocked=false;s.atSea=true;var discoveredPort=map.Port(6);s.seaX=discoveredPort.x;s.seaY=discoveredPort.y;s.discovered.Remove(6);game.Workflow.Advance(.01,false);Check(s.discovered.Contains(6),"world port discovery uses world units");
  var restored=JsonUtility.FromJson<GameState>(JsonUtility.ToJson(s));Check(restored.sailing.manualMapId==WorldMap.Id&&restored.sailing.worldCoordinateVersion==1,"world save roundtrip");Near(restored.sailing.previousManualVoyage.x,.23,"legacy snapshot roundtrip");
  double archivedX=s.seaX,archivedY=s.seaY;gold=s.gold;food=s.ships[0].food;
  Check(game.RestoreRegionalSailing(),"retire rejected world presentation");Check(!game.WorldSailing&&!s.sailing.preferWorldMap,"regional presentation restored");
  Near(s.seaX,.23,"restore regional x");Near(s.seaY,.45,"restore regional y");
  Near(s.sailing.suspendedWorldVoyage.x,archivedX,"archive world x");Near(s.sailing.suspendedWorldVoyage.y,archivedY,"archive world y");
  Near(s.gold,gold,"rollback preserves gold");Near(s.ships[0].food,food,"rollback preserves supplies");
  Check(!game.RestoreRegionalSailing(),"rollback idempotent");
  Check(new SailingState().preferWorldMap,"new games use streamed approved renderer");
  foreach(double heading in new[]{0,Math.PI/4,Math.PI/2,Math.PI,-Math.PI/2}){
   var a=JsonUtility.FromJson<GameState>(JsonUtility.ToJson(s));var b=JsonUtility.FromJson<GameState>(JsonUtility.ToJson(s));
   a.atSea=b.atSea=true;a.sailing.mode=b.sailing.mode="normal";a.sailing.manualMapId="southeast_asia_v2";b.sailing.manualMapId="regional_world_v2";
   a.seaX=.5;a.seaY=.5;b.seaX=8000;b.seaY=2500;a.heading=b.heading=heading;a.wind=b.wind=heading;a.trim=b.trim=.5;
   field.sample=(xx,yy)=>32;field.known=true;
   var original=new GameService(catalog,a);var streamed=new GameService(catalog,b){SeaNavigation=new MaritimeLegend.StreamedSeaNavigation(field)};
   double legacy=original.Sail(.25,1),current=streamed.Sail(.25,1);
   Check(Math.Abs((b.seaX-8000)/960-(a.seaX-.5))<1e-7,"streamed original east/west speed");
   Check(Math.Abs((2500-b.seaY)/540-(a.seaY-.5))<1e-7,"streamed original north/south speed");
   Check(Math.Abs(legacy-current)<1e-7,"streamed original speed display");
   double stoppedX=b.seaX,stoppedY=b.seaY;for(int i=0;i<100;i++)streamed.Sail(1.0/60,0);Near(b.seaX,stoppedX,"no stopped drift x");Near(b.seaY,stoppedY,"no stopped drift y");
  }
  var exactNav=new MaritimeLegend.StreamedSeaNavigation(field);field.known=false;exactNav.ResolveMove(8000,2500,8001,2500,out x,out y);Near(x,8000,"streamed unknown holds position");Check(exactNav.Loading,"streamed unknown requests wait");field.known=true;
  exactNav.ResolveMove(WorldMap.Width-.2,2560,WorldMap.Width+.2,2560,out x,out y);Check(x<.201&&x>.199,"original collision kernel across east seam");
  exactNav.ResolveMove(8000,5120,8001,5122,out x,out y);Near(y,5120,"original collision kernel polar clamp");Check(x>8000.99,"original collision kernel wall tangent");
  // Discovery uses half of fog vision by default, persists, and only queues a first-time notice.
  var discoveryState=GameService.New(catalog,0,0,"休闲",StateFactory.Appearance(catalog),"Discovery checks",73);
  var discoveryGame=new GameService(catalog,discoveryState);discoveryState.pendingEvents.Clear();discoveryState.atSea=true;
  discoveryGame.EnterWorldSailing(map);discoveryGame.WorldMapData=map;
  var hangzhou=map.Port(7);discoveryState.discovered.Remove(7);
  double originalFraction=catalog.balance.world.cityDiscoveryVisionFraction;
  catalog.balance.world.cityDiscoveryVisionFraction=.5;
  Near(discoveryGame.WorldCityDiscoveryRadius,discoveryGame.WorldVisionRadius*.5,"city visibility defaults to half fog vision");
  discoveryState.seaY=hangzhou.y;discoveryState.seaX=hangzhou.x+discoveryGame.WorldCityDiscoveryRadius*1.01;
  discoveryGame.DiscoverNearbyCities();Check(!discoveryState.discovered.Contains(7),"city hidden outside discovery radius");
  discoveryState.seaX=hangzhou.x+discoveryGame.WorldCityDiscoveryRadius*.99;
  discoveryGame.DiscoverNearbyCities();Check(discoveryState.discovered.Contains(7),"Hangzhou discovered inside radius");
  Check(discoveryGame.TryTakeCityDiscovery(out int foundCity)&&foundCity==7,"first discovery queues city notification");
  discoveryGame.DiscoverNearbyCities();Check(!discoveryGame.TryTakeCityDiscovery(out foundCity),"stationary ship does not repeat discovery");
  var discoveryReload=JsonUtility.FromJson<GameState>(JsonUtility.ToJson(discoveryState));
  var reloadedDiscoveryGame=new GameService(catalog,discoveryReload){WorldMapData=map};
  reloadedDiscoveryGame.DiscoverNearbyCities();Check(!reloadedDiscoveryGame.TryTakeCityDiscovery(out foundCity),"save reload does not repeat discovery");
  catalog.balance.world.cityDiscoveryVisionFraction=.25;
  Near(discoveryGame.WorldCityDiscoveryRadius,discoveryGame.WorldVisionRadius*.25,"discovery distance is configurable");
  catalog.balance.world.cityDiscoveryVisionFraction=originalFraction;
  Debug.Log("WORLD_SAILING_CHECKS_PASS "+checks+" assertions; all shared chunk edges, ports, cyclic movement, walls, migration and clock");
 }
}
}
#endif
