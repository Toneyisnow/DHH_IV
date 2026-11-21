using System;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class StreamedSeaNavigation:IStreamedSeaNavigation {
 readonly IWorldTerrain terrain;readonly CoastNavigation coast;double originX,originY;
 public bool Loading {get;private set;}public bool Contact=>coast.Contact;public bool Sliding=>coast.Sliding;public int LatitudeWall {get;private set;}
 public StreamedSeaNavigation(IWorldTerrain terrain){this.terrain=terrain;coast=new CoastNavigation(p=>{if(!terrain.TryDistance(WorldMap.Wrap(originX+p.x*960.0/CoastNavigation.Width),WorldMap.ClampY(originY-p.y*540.0/CoastNavigation.Height),out var distance)){Loading=true;return -1000;}return (float)distance*CoastNavigation.Width/960;},9);}
 public bool Safe(double x,double y)=>terrain.TryDistance(WorldMap.Wrap(x),WorldMap.ClampY(y),out var d)&&d>=9*960/CoastNavigation.Width;
 public void ResolveMove(double fromX,double fromY,double toX,double toY,out double x,out double y){
  Loading=false;LatitudeWall=toY<0?-1:toY>WorldMap.Height?1:0;x=fromX;y=fromY;
  if(!WorldMap.Finite(fromX)||!WorldMap.Finite(fromY)||!WorldMap.Finite(toX)||!WorldMap.Finite(toY))throw new ArgumentException("Invalid sailing coordinates");
  if(fromX==toX&&fromY==WorldMap.ClampY(toY))return;
  if(!terrain.TryDistance(fromX,fromY,out _)){Loading=true;return;}
  originX=fromX;originY=fromY;var from=Vector2.zero;
  var to=new Vector2((float)((toX-fromX)/960*CoastNavigation.Width),(float)((fromY-WorldMap.ClampY(toY))/540*CoastNavigation.Height));
  if((to-from).magnitude>1000)throw new ArgumentException("Use explicit loading for a teleport");
  var result=coast.MovePixels(from,to-from);if(Loading)return;
  x=WorldMap.Wrap(fromX+result.x*960.0/CoastNavigation.Width);y=WorldMap.ClampY(fromY-result.y*540.0/CoastNavigation.Height);
 }
}
}
