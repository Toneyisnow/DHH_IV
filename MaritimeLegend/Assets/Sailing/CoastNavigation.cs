using System;
using System.IO;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class CoastNavigation : ISeaNavigationPolicy {
 public const float Width=1672,Height=941,Clearance=14,SlideFactor=.22f;
 public Vector2 Origin;public bool Contact {get;private set;} public bool Sliding {get;private set;}
 readonly int width,height;readonly float[] distances;readonly float clearanceLimit;
 public CoastNavigation(byte[] bytes,float clearance=Clearance){clearanceLimit=clearance;using(var reader=new BinaryReader(new MemoryStream(bytes))){width=reader.ReadInt32();height=reader.ReadInt32();distances=new float[width*height];for(int i=0;i<distances.Length;i++)distances[i]=reader.ReadSingle();}}
 public Vector2 Pixels(Vector2 uv)=>new Vector2(uv.x*Width,uv.y*Height);
 public Vector2 UV(Vector2 p)=>new Vector2(p.x/Width,p.y/Height);
 public float Distance(Vector2 p){
  float edge=Mathf.Min(p.x,p.y,Width-1-p.x,Height-1-p.y);if(edge<0)return edge;
  int x=Mathf.Clamp((int)p.x,0,width-2),y=Mathf.Clamp((int)p.y,0,height-2);float tx=p.x-x,ty=p.y-y;
  return Mathf.Min(edge,Mathf.Lerp(Mathf.Lerp(distances[y*width+x],distances[y*width+x+1],tx),Mathf.Lerp(distances[(y+1)*width+x],distances[(y+1)*width+x+1],tx),ty));
 }
 Vector2 Normal(Vector2 p){var n=new Vector2(Distance(p+Vector2.right)-Distance(p-Vector2.right),Distance(p+Vector2.up)-Distance(p-Vector2.up));return n.sqrMagnitude>1e-6f?n.normalized:Vector2.zero;}
 public Vector2 MovePixels(Vector2 start,Vector2 delta){
  Contact=Sliding=false;Vector2 p=start;int steps=Mathf.Max(1,Mathf.CeilToInt(delta.magnitude/.75f));Vector2 step=delta/steps;
  for(int i=0;i<steps;i++){
   float clearance=Distance(p);var next=p+step;
   if(Distance(next)>=clearanceLimit){p=next;continue;}
   Contact=true;
   // A legacy save already inside the new mask can escape outwards, never further inland.
   if(clearance<clearanceLimit-.02f){if(Distance(next)>clearance+.001f)p=next;continue;}
   float lo=0,hi=1;for(int j=0;j<9;j++){float t=(lo+hi)*.5f;if(Distance(p+step*t)>=clearanceLimit)lo=t;else hi=t;}
   p+=step*lo;var n=Normal(p);var remaining=step*(1-lo);var tangent=(remaining-n*Vector2.Dot(remaining,n))*SlideFactor;
   if(tangent.sqrMagnitude>1e-8f){var candidate=p+tangent+n*.025f;if(Distance(candidate)>=clearanceLimit){p=candidate;Sliding=true;}}
  }
  return p;
 }
 public void ResolveMove(double fromX,double fromY,double toX,double toY,out double x,out double y){
  if(fromX==toX&&fromY==toY){Contact=Sliding=false;x=fromX;y=fromY;return;}
  var from=Pixels(new Vector2((float)fromX,(float)fromY)-Origin);var to=Pixels(new Vector2((float)toX,(float)toY)-Origin);
  var result=UV(MovePixels(from,to-from))+Origin;x=result.x;y=result.y;
 }
}
}
