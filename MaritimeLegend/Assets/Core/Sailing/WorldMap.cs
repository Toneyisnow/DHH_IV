using System;
using System.Collections.Generic;
namespace MaritimeLegend.Domain {
// One coordinate contract for navigation, rendering, saves and picking. Y is north.
public static class WorldMap {
 public const string Id="world_v1";
 public const int Version=1, Columns=72, Rows=32, Cells=64;
 public const double Scale=32, Width=360*Scale, Height=160*Scale, ChunkSize=5*Scale, SampleStep=ChunkSize/Cells;
 public const double LegacySpeedScale=960, Clearance=3, PortRadius=24;
 public static double Wrap(double x)=>x-Math.Floor(x/Width)*Width;
 public static double ClampY(double y)=>Math.Max(0,Math.Min(Height,y));
 public static double DeltaX(double from,double to)=>Wrap(to-from+Width*.5)-Width*.5;
 public static double Distance(double ax,double ay,double bx,double by){double dx=DeltaX(ax,bx),dy=by-ay;return Math.Sqrt(dx*dx+dy*dy);}
 public static int Column(double x)=>(int)(Wrap(x)/ChunkSize);
 public static int Row(double y)=>Math.Min(Rows-1,(int)(ClampY(y)/ChunkSize));
 public static int Key(double x,double y)=>Row(y)*Columns+Column(x);
 public static int WrapColumn(int x)=>(x%Columns+Columns)%Columns;
 public static bool Finite(double x)=>!double.IsNaN(x)&&!double.IsInfinity(x);
 public static string Region(double x,double y){double lon=Wrap(x)/Scale-180,lat=y/Scale-80;
  if(lon>=95&&lon<145&&lat<25&&lat>-15)return "southeast_asia";
  if(lon>=95&&lat>=25)return "east_asia";
  if(lon>=25&&lon<95&&lat<30)return "indian_ocean";
  if(lon>=-8&&lon<40&&lat>=30&&lat<47)return "mediterranean_sea";
  if(lon>=-30&&lon<45&&lat>=47)return "north_sea";
  return lon<-30?"new_world":"africa";
 }
}
[Serializable] public sealed class WorldMapManifest {
 public int version,unitsPerDegree,chunkCells,columns,rows;
 public string worldId,source,oceanBits; [NonSerialized] public WorldChunkRecord[] chunks; public WorldPort[] ports;
 public void Validate(){
  if(version!=WorldMap.Version||worldId!=WorldMap.Id||unitsPerDegree!=WorldMap.Scale||chunkCells!=WorldMap.Cells||columns!=WorldMap.Columns||rows!=WorldMap.Rows)throw new InvalidOperationException("Unsupported world map manifest");
  if(chunks==null){var bits=Convert.FromBase64String(oceanBits??"");if(bits.Length!=columns*rows/8)throw new InvalidOperationException("Invalid ocean index");chunks=new WorldChunkRecord[columns*rows];for(int i=0;i<chunks.Length;i++){int x=i%columns,y=i/columns;bool ocean=(bits[i/8]&(1<<(i%8)))!=0;chunks[i]=new WorldChunkRecord{x=x,y=y,ocean=ocean,key=ocean?"":"Data/WorldMap/Chunks/"+x.ToString("00")+"_"+y.ToString("00")};}}
  if(chunks.Length!=columns*rows)throw new InvalidOperationException("Invalid world chunk count");
  for(int i=0;i<chunks.Length;i++)if(chunks[i]==null||chunks[i].x!=i%columns||chunks[i].y!=i/columns||(!chunks[i].ocean&&string.IsNullOrEmpty(chunks[i].key)))throw new InvalidOperationException("Invalid world chunk "+i);
  var ids=new HashSet<int>();if(ports==null||ports.Length==0)throw new InvalidOperationException("Missing world ports");
  foreach(var p in ports)if(!ids.Add(p.id)||!WorldMap.Finite(p.x)||!WorldMap.Finite(p.y)||p.x<0||p.x>=WorldMap.Width||p.y<0||p.y>WorldMap.Height)throw new InvalidOperationException("Invalid world port");
 }
 public WorldPort Port(int id){foreach(var p in ports)if(p.id==id)return p;throw new InvalidOperationException("No world berth for port "+id);}
}
[Serializable] public sealed class WorldChunkRecord {public int x,y;public bool ocean;public string key;}
[Serializable] public sealed class WorldPort {public int id;public string name;public double x,y,longitude,latitude;}
[Serializable] public sealed class WorldExploredChunk {public int key;public string bits;}
[Serializable] public sealed class PreviousManualVoyage {
 public string mapId,spawnPort;public double x,y;public float spawnX,spawnY;public ManualFollowerState[] followers;
}
public interface IWorldTerrain {bool TryDistance(double x,double y,out double distance);}
// Unknown navigation is blocked, including every intermediate sample of a swept move.
public sealed class WorldNavigation : ISeaNavigationPolicy {
 readonly IWorldTerrain terrain;
 public bool Loading {get;private set;} public bool Contact {get;private set;} public int LatitudeWall {get;private set;}
 public double Travel {get;private set;}
 public WorldNavigation(IWorldTerrain terrain){this.terrain=terrain;}
 public bool Safe(double x,double y)=>terrain.TryDistance(WorldMap.Wrap(x),WorldMap.ClampY(y),out var d)&&d>=WorldMap.Clearance;
 public void ResolveMove(double fromX,double fromY,double toX,double toY,out double x,out double y){
  Contact=Loading=false;LatitudeWall=0;Travel=0;x=fromX;y=WorldMap.ClampY(fromY);
  if(!WorldMap.Finite(fromX)||!WorldMap.Finite(fromY)||!WorldMap.Finite(toX)||!WorldMap.Finite(toY))throw new ArgumentException("Invalid world movement");
  double dx=toX-fromX,dy=toY-fromY;
  int steps=Math.Max(1,(int)Math.Ceiling(Math.Sqrt(dx*dx+dy*dy)/(WorldMap.SampleStep*.25)));
  // Long teleports must use an explicit destination loader, never millions of sweep steps.
  if(steps>16384)throw new ArgumentException("World movement requires bounded simulation steps");
  dx/=steps;dy/=steps;
  for(int i=0;i<steps;i++){
   double nx=x+dx,rawY=y+dy,ny=WorldMap.ClampY(rawY);
   if(ny!=rawY)LatitudeWall=rawY<0?-1:1;
   if(!terrain.TryDistance(WorldMap.Wrap(nx),ny,out double d)){Loading=true;break;}
   double ox=x,oy=y;
   if(d>=WorldMap.Clearance){x=nx;y=ny;}
   else {
    Contact=true;
    // Axis slide has its own swept, sub-cell samples and remains on known water.
    if(Safe(nx,y))x=nx;else if(Safe(x,ny))y=ny;
   }
   Travel+=Math.Sqrt((x-ox)*(x-ox)+(y-oy)*(y-oy));
  }
  x=WorldMap.Wrap(x);
 }
}
}
