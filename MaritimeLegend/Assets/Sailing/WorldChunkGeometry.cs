using System;
using System.Collections.Generic;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// CPU-only job: no GameObjects, Meshes or Unity engine API calls on the worker.
public sealed class WorldChunkGeometry {
 public Vector3[] vertices,normals;public Color[] colors;public int[] triangles;
 struct Vertex {public Vector3 p;public float d;public Vertex(Vector3 p,float d){this.p=p;this.d=d;}}
 public static WorldChunkGeometry Build(WorldChunkData data,int stride){
  var v=new List<Vector3>();var ns=new List<Vector3>();var cs=new List<Color>();var ids=new List<int>();
  double latitude=(data.Y*5-80)+2.5;
  Func<int,int,Vertex> point=(x,y)=>{
   float d=data.At(x,y);double gx=(data.X*WorldMap.Cells+x)*WorldMap.SampleStep,gy=(data.Y*WorldMap.Cells+y)*WorldMap.SampleStep;
   double wave=.5+.25*Math.Sin(gx/WorldMap.Width*Math.PI*2*37+gy*.031)+.25*Math.Cos(gx/WorldMap.Width*Math.PI*2*61-gy*.019);
   float h=(float)(Math.Min(Math.Max(0,-d)*.65,26)*(.35+wave*.9));
   return new Vertex(new Vector3((float)(x*WorldMap.SampleStep),h,(float)(y*WorldMap.SampleStep)),d);
  };
  Action<Vertex,Vertex,Vertex> tri=(a,b,c)=>{
   var input=new[]{a,b,c};var poly=new List<Vertex>(4);
   for(int i=0;i<3;i++){var p=input[i];var q=input[(i+1)%3];if(p.d<=0)poly.Add(p);
    if((p.d<=0)!=(q.d<=0)){float t=p.d/(p.d-q.d);var pos=p.p+(q.p-p.p)*t;pos.y=0;poly.Add(new Vertex(pos,0));}}
   for(int i=1;i<poly.Count-1;i++){
    var aa=poly[0];var bb=poly[i];var cc=poly[i+1];var n=Vector3.Cross(bb.p-aa.p,cc.p-aa.p).normalized;
    foreach(var p in new[]{aa,bb,cc}){
     ids.Add(v.Count);v.Add(p.p);ns.Add(n);
     float inland=Math.Min(1,Math.Max(0,-p.d)/12),high=Math.Min(1,p.p.y/25);
     var sand=new Color(.77f,.69f,.43f);var green=Math.Abs(latitude)<30?new Color(.24f,.43f,.22f):new Color(.36f,.46f,.27f);
     var col=Color.Lerp(sand,green,inland);col=Color.Lerp(col,new Color(.49f,.45f,.33f),high*.55f);
     if(Math.Abs(latitude)>60)col=Color.Lerp(col,new Color(.84f,.87f,.84f),(float)Math.Min(1,(Math.Abs(latitude)-60)/17));
     cs.Add(col);
    }
   }
  };
  for(int y=0;y<WorldMap.Cells;y+=stride)for(int x=0;x<WorldMap.Cells;x+=stride){var a=point(x,y);var b=point(x+stride,y);var c=point(x,y+stride);var d=point(x+stride,y+stride);tri(a,c,b);tri(b,c,d);}
  // Skirts hide LOD T junctions. Collision always uses the original full-resolution field.
  for(int edge=0;edge<4;edge++)for(int t=0;t<WorldMap.Cells;t+=stride){
   int x=edge==0?0:edge==1?WorldMap.Cells:t,y=edge==2?0:edge==3?WorldMap.Cells:t;
   var a=point(x,y);var b=point(edge<2?x:x+stride,edge<2?y+stride:y);
   if(a.d>0&&b.d>0)continue;
   var aa=a;var bb=b;aa.p.y=-3;bb.p.y=-3;tri(a,aa,b);tri(b,aa,bb);
  }
  return new WorldChunkGeometry{vertices=v.ToArray(),normals=ns.ToArray(),colors=cs.ToArray(),triangles=ids.ToArray()};
 }
 public Mesh Upload(string name){var m=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};m.vertices=vertices;m.normals=normals;m.colors=colors;m.triangles=triangles;m.RecalculateBounds();m.UploadMeshData(true);return m;}
}
}
