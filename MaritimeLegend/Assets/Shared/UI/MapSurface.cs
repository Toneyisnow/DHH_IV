using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class MapSurface {
 public float Zoom=1; public Vector2 Pan; Vector2 previous;Texture2D texture;
 public Vector2 Point(double x,double y)=>new Vector2(65+(float)x*1300*Zoom+Pan.x,150+(float)y*650*Zoom+Pan.y);
 public int Draw(MaritimeGame ui,int selected,string title){
  ui.Panel(new Rect(30,130,1405,755));if(texture==null)texture=Resources.Load<Texture2D>("Sprites/map");if(texture!=null)LiveGui.DrawTexture(new Rect(40,140,1385,735),texture,ScaleMode.ScaleAndCrop);ui.Fill(new Rect(40,140,1385,735),new Color(.015f,.12f,.16f,.65f));ui.Label(65,148,1260,60,title);
  var mouse=LiveGui.Mouse;if(new Rect(40,140,1385,735).Contains(mouse)){Zoom=Mathf.Clamp(Zoom+Input.mouseScrollDelta.y*.05f,.7f,1.4f);if(Input.GetMouseButton(1))Pan+=mouse-previous;}previous=mouse;
  foreach(var r in ui.s.routes){var ids=r.Split('>');var a=ui.c.ports[int.Parse(ids[0])];var b=ui.c.ports[int.Parse(ids[1])];ui.Line(Point(a.x,a.y),Point(b.x,b.y),new Color(.75f,.65f,.38f));}
  foreach(var p in ui.c.ports){var v=Point(p.x,p.y);bool known=ui.s.discovered.Contains(p.id);ui.Fill(new Rect(v.x,v.y,p.id==ui.s.home?18:12,p.id==ui.s.home?18:12),known?Color.yellow:Color.gray);if(ui.Btn(v.x-50,v.y+18,155,44,p.name+(p.id==selected?" ◇":"")))selected=p.id;}
  ui.Label(65,815,1250,50,"金色：已探索  灰色：待探索  细线：已发现航路  ◇：选中港口 · 滚轮缩放 / 右键拖动");return selected;
 }
}
}
