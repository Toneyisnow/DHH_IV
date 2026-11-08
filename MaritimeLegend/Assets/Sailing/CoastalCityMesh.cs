using System;
using System.Collections.Generic;
using UnityEngine;
namespace MaritimeLegend {
 // Editable layout assets describe solid buildings. Each foundation corner samples the terrain.
 public static class CoastalCityMesh {
  [Serializable] public class Layout {public Building[] buildings;public float[] wall,roof;public int palms=4;}
  [Serializable] public class Building {public float x,z,w,d,h;public int tiers=1;public bool tower,stilts;}
  public static Mesh Build(ManualSeaPort port,Func<float,float,float> ground){
   var asset=Resources.Load<TextAsset>("Models/ManualSailing/Ports3D/"+port.id);
   if(!asset)throw new InvalidOperationException("Missing 3D city layout "+port.id);
   var layout=JsonUtility.FromJson<Layout>(asset.text);var v=new List<Vector3>();var triangles=new List<int>();var colors=new List<Color>();
   float width=port.height*port.aspect,depth=width*.72f;
   var sea=ManualSeaV2Runtime.World(port.Spawn)-port.Position;sea.y=0;sea.Normalize();
   var right=Vector3.Cross(Vector3.up,sea);var center=port.Position+Vector3.up*port.height*.5f;
   Vector3 World(float x,float z,float y)=>new Vector3(port.x,0,port.z)+right*x+sea*z+Vector3.up*y;
   Color wall=new Color(layout.wall[0],layout.wall[1],layout.wall[2]);Color roof=new Color(layout.roof[0],layout.roof[1],layout.roof[2]);
   Color stone=new Color(.55f,.50f,.34f),wood=new Color(.32f,.19f,.09f),gold=new Color(.80f,.62f,.28f);
   void Face(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color color){int n=v.Count;v.Add(a-center);v.Add(b-center);v.Add(c-center);v.Add(d-center);for(int i=0;i<4;i++)colors.Add(color);triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
   void Box(float x,float z,float w,float d,float bottom,float top,Color color,bool conform=false){
    var a=World(x-w/2,z-d/2,bottom);var b=World(x+w/2,z-d/2,bottom);var c=World(x+w/2,z+d/2,bottom);var e=World(x-w/2,z+d/2,bottom);
    if(conform){a.y=ground(a.x,a.z)-.16f;b.y=ground(b.x,b.z)-.16f;c.y=ground(c.x,c.z)-.16f;e.y=ground(e.x,e.z)-.16f;}
    var at=new Vector3(a.x,top,a.z);var bt=new Vector3(b.x,top,b.z);var ct=new Vector3(c.x,top,c.z);var et=new Vector3(e.x,top,e.z);
    Face(a,at,bt,b,color);Face(b,bt,ct,c,color*.92f);Face(c,ct,et,e,color);Face(e,et,at,a,color*.92f);Face(at,et,ct,bt,color*1.08f);Face(e,a,b,c,color);
   }
   float High(float x,float z,float w,float d){float h=.15f;for(int iz=0;iz<3;iz++)for(int ix=0;ix<3;ix++){var p=World(x+(ix-1)*w/2,z+(iz-1)*d/2,0);h=Mathf.Max(h,ground(p.x,p.z));}return h;}
   float Low(float x,float z,float w,float d){float h=float.MaxValue;for(int iz=0;iz<3;iz++)for(int ix=0;ix<3;ix++){var p=World(x+(ix-1)*w/2,z+(iz-1)*d/2,0);h=Mathf.Min(h,ground(p.x,p.z));}return h;}
   void Roof(float x,float z,float w,float d,float y,float h,Color color){
    var a=World(x-w/2,z-d/2,y);var b=World(x+w/2,z-d/2,y);var c=World(x+w/2,z+d/2,y);var e=World(x-w/2,z+d/2,y);
    var l=World(x-w*.27f,z,y+h);var r=World(x+w*.27f,z,y+h);
    Face(a,l,r,b,color);Face(b,r,r,c,color*.82f);Face(c,r,l,e,color*.9f);Face(e,l,l,a,color*.82f);Face(e,a,b,c,wood);
    Box(x,z,w*.58f,.10f,y+h,y+h+.1f,gold);
    // Raised tile courses on both roof slopes, with real eaves and ridge caps.
    for(int row=1;row<6;row++){float f=row/6f;float rw=Mathf.Lerp(w,w*.54f,f);for(int side=-1;side<=1;side+=2)Box(x,z+side*d*.5f*(1-f),rw,.045f,y+h*f+.012f,y+h*f+.045f,color*.80f);}
    Box(x,z-d*.5f,w,.10f,y-.12f,y+.02f,gold*.78f);Box(x,z+d*.5f,w,.10f,y-.12f,y+.02f,gold*.78f);
   }
   var occupied=new List<Rect>();
   foreach(var b in layout.buildings){
    float x=b.x*width,z=b.z*depth,w=b.w*width*.82f,d=b.d*depth*.82f,h=b.h*port.height;
    // Search within the original icon footprint for a terrace. Avoid placing a house
    // across a cliff just because the old flat icon happened to cover that cliff.
    float desiredX=x,desiredZ=z,best=float.MaxValue;
    for(int iz=0;iz<=16;iz++)for(int ix=0;ix<=18;ix++){
     float cx=Mathf.Lerp(-width*.46f+w*.55f,width*.46f-w*.55f,ix/18f),cz=Mathf.Lerp(-depth*.32f+d*.55f,depth*.52f-d*.55f,iz/16f);
     var footprint=new Rect(cx-w*.56f,cz-d*.56f,w*1.12f,d*1.12f);bool overlaps=false;foreach(var previous in occupied)if(previous.Overlaps(footprint)){overlaps=true;break;}if(overlaps)continue;
     float hi=High(cx,cz,w*1.1f,d*1.1f),lo=Low(cx,cz,w*1.1f,d*1.1f),slope=hi-Mathf.Max(0,lo);
     float score=(cx-desiredX)*(cx-desiredX)+(cz-desiredZ)*(cz-desiredZ)+slope*slope*14+Mathf.Max(0,hi-port.y-3)*100+(lo<0?2:0);
     if(score<best){best=score;x=cx;z=cz;}
    }
    occupied.Add(new Rect(x-w*.56f,z-d*.56f,w*1.12f,d*1.12f));
    float floor=High(x,z,w*1.1f,d*1.1f)+.35f;
    bool stilts=b.stilts||Low(x,z,w,d)<.05f;
    if(stilts){floor=Mathf.Max(floor,1.1f);for(int ix=-1;ix<=1;ix+=2)for(int iz=-1;iz<=1;iz+=2)Box(x+ix*w*.43f,z+iz*d*.43f,.19f,.19f,-1,floor,wood,true);Box(x,z,w*1.08f,d*1.08f,floor-.17f,floor,wood);}
    else Box(x,z,w*1.1f,d*1.1f,0,floor,stone,true);
    Box(x,z,w,d,floor,floor+h,wall);
    Box(x,z,w*1.025f,d*1.025f,floor+h*.10f,floor+h*.17f,gold*.77f);
    Box(x,z,w*1.025f,d*1.025f,floor+h*.90f,floor+h*.97f,wood);
    for(int sx=-1;sx<=1;sx+=2)for(int sz=-1;sz<=1;sz+=2)Box(x+sx*w*.49f,z+sz*d*.49f,.10f,.10f,floor,floor+h,wood);
    // Doors and recessed dark windows on all four facades remain visible when orbiting.
    Box(x,z+d/2+.025f,w*.19f,.06f,floor,floor+h*.62f,wood);
    for(int side=-1;side<=1;side+=2){
     for(int k=-1;k<=1;k+=2){float wx=x+k*w*.29f,wz=z+side*(d/2+.025f);Box(wx,wz,w*.13f,.055f,floor+h*.44f,floor+h*.74f,wood);Box(wx,wz,w*.025f,.07f,floor+h*.44f,floor+h*.74f,gold);Box(wx,wz,w*.16f,.08f,floor+h*.42f,floor+h*.45f,gold);}
     Box(x+side*(w/2+.025f),z,.055f,d*.25f,floor+h*.42f,floor+h*.73f,wood);
    }
    float roofY=floor+h;
    for(int t=0;t<b.tiers;t++){
     float scale=Mathf.Pow(.70f,t),rh=port.height*(b.tower?.13f:.085f)*scale;
     Roof(x,z,w*1.22f*scale,d*1.24f*scale,roofY,rh,roof);roofY+=rh*.78f;
     if(t+1<b.tiers){Box(x,z,w*scale*.56f,d*scale*.56f,roofY,roofY+.28f,wall);roofY+=.28f;}
    }
    // Short terraced access steps follow the sampled beach rather than floating.
    for(int step=0;step<4;step++){float zz=z+d*.57f+step*.25f;float low=High(x,zz,w*.28f,.28f);if(low<floor)Box(x,zz,w*.28f,.3f,0,Mathf.Max(low+.08f,floor-step*.18f),stone,true);}
   }
   // Pier extends toward the actual offshore berth. Posts end in the seabed at each corner.
   float pierStart=depth*.23f,pierEnd=depth*.65f;float deck=High(0,pierStart,1.25f,.5f)+.28f;
   for(float z=pierStart;z<pierEnd;z+=.7f){Box(0,z,1.25f,.66f,deck-.15f,deck,wood);for(int side=-1;side<=1;side+=2)Box(side*.52f,z,.14f,.14f,-2,deck+.30f,wood,true);}
   // Solid low-poly palms: trunk and eight tapered fronds, not camera-facing foliage.
   for(int i=0;i<layout.palms;i++){
    float x=(i%2==0?-1:1)*width*(.36f+(i%3)*.025f),z=(-.28f+i*.13f)*depth;var p=World(x,z,0);float y=ground(p.x,p.z),h=port.height*.32f;if(y<.1f||y>port.y+6)continue;
    Box(x,z,.16f,.16f,y,y+h,wood);
    for(int k=0;k<8;k++){float a=k*Mathf.PI/4;var start=World(x,z,y+h);var dir=(right*Mathf.Cos(a)+sea*Mathf.Sin(a))*1.65f;var cross=Vector3.Cross(Vector3.up,dir).normalized*.30f;var mid=start+dir*.5f+Vector3.up*.25f;var tip=start+dir-Vector3.up*.5f;Face(start,mid-cross,tip,mid+cross,new Color(.28f,.42f,.16f));Face(start,mid+cross,tip,mid-cross,new Color(.35f,.49f,.19f));}
   }
   var mesh=new Mesh{name="Solid coastal city - "+port.id};mesh.SetVertices(v);mesh.SetTriangles(triangles,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
  }
 }
}
