using System;
using System.IO;
using UnityEngine;
namespace MaritimeLegend {
public static class SeaReliefChecks {
 static void Require(bool value,string message){if(!value)throw new Exception("SEA RELIEF CHECK: "+message);}
 public static void Run(){
  foreach(var p in new[]{new Vector2(300,300),new Vector2(1500,800),new Vector2(960,540)})Require(Vector2.Distance(p,SeaProjection.Project(SeaProjection.Unproject(p,Vector2.one*.5f),Vector2.one*.5f))<.1f,"projection round trip");
  byte[] bytes;using(var stream=new MemoryStream()){using(var writer=new BinaryWriter(stream)){writer.Write(1672);writer.Write(941);for(int y=0;y<941;y++)for(int x=0;x<1672;x++)writer.Write(800f-x);}bytes=stream.ToArray();}
  var coast=new CoastNavigation(bytes);
  var head=coast.MovePixels(new Vector2(770,400),new Vector2(100,0));Require(head.x<=786.01f&&head.x>=785.9f,"head-on stops at clearance");Require(!coast.Sliding,"head-on must not slide");
  var slide=coast.MovePixels(new Vector2(786,400),new Vector2(15,30));Require(slide.x<=786.01f&&slide.y>401&&slide.y<413,"slow tangential slide");Require(coast.Sliding,"slide status");
  var fast=coast.MovePixels(new Vector2(100,400),new Vector2(1400,0));Require(fast.x<=786.01f,"sweep does not tunnel");
  var away=coast.MovePixels(head,new Vector2(-20,0));Require(away.x<head.x-19,"escape from coast");
  var real=new CoastNavigation(Resources.Load<TextAsset>("Models/ManualSailing/coast-distance").bytes);int sweeps=0;
  for(int y=25;y<916;y+=31)for(int x=25;x<1647;x+=31){var p=new Vector2(x,y);if(real.Distance(p)<15)continue;var d=new Vector2(Mathf.Sin(x*.7f)*90,Mathf.Cos(y*.7f)*90);var q=real.MovePixels(p,d);Require(real.Distance(q)>=13.98f,"real map clearance at "+p);sweeps++;}
  Require(sweeps>300,"real coastal samples");
  using(var reader=new BinaryReader(new MemoryStream(Resources.Load<TextAsset>("Models/ManualSailing/relief").bytes))){int w=reader.ReadInt32(),h=reader.ReadInt32();float peak=0;int shore=0;for(int y=0;y<h;y++)for(int x=0;x<w;x++){float elevation=reader.ReadSingle();peak=Mathf.Max(peak,elevation);float d=real.Distance(new Vector2(x*1672f/(w-1),y*941f/(h-1)));if(d<-.5f&&d> -2f&&x>3&&y>3&&x<w-4&&y<h-4){Require(elevation<.15f,"shore rises above sea level");shore++;}}Require(peak>2&&peak<10&&shore>100,"low relief and shore samples");}
  using(var reader=new BinaryReader(new MemoryStream(Resources.Load<TextAsset>("Models/ManualSailing/bathymetry").bytes))){int w=reader.ReadInt32(),h=reader.ReadInt32();double near=0,deep=0;int nc=0,dc=0;for(int y=0;y<h;y++)for(int x=0;x<w;x++){float depth=reader.ReadSingle();Require(depth>=0&&!float.IsNaN(depth),"valid depth");if(x%7!=0||y%7!=0)continue;float d=real.Distance(new Vector2(x,y));if(d>1&&d<10){near+=depth;nc++;}if(d>100){deep+=depth;dc++;}}Require(nc>0&&dc>0&&deep/dc>near/nc+100,"offshore depth contrast");}
  Debug.Log("SEA_STYLE_PASS: zero-height shoreline, bounded mountain relief, shallow/deep bathymetry");
  float min=100,max=-100;for(int i=0;i<=100;i++){float v=SeaBobbing.Offset(80,i*.05f,0);min=Mathf.Min(min,v);max=Mathf.Max(max,v);}Require(Mathf.Abs(max-min-16)<.03f,"one-fifth bob excursion");
  Debug.Log("SEA_RELIEF_GEOMETRY_PASS: actual map sweeps="+sweeps+", head-on stop, oblique slide, escape, tunneling, projection, 20% bob");
 }
}
}
