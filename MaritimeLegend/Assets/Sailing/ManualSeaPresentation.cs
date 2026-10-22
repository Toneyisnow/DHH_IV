using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
public partial class MaritimeGame {
 Texture2D seaMap; Material seaMaterial; readonly Texture2D[,] fleetArt=new Texture2D[3,8];
 Texture2D seaCity,seaCity2; GameState seaOwner; int seaDeparture=-1; Vector2 seaCamera,seaOrigin,seaTarget; bool seaHasTarget,seaStopped=true;
 static readonly string[] SeaDirections={"E","SE","S","SW","W","NW","N","NE"};
 internal Vector2 SeaCamera=>seaCamera;
 internal Vector2 SeaPosition=>new Vector2((float)s.seaX,(float)s.seaY);
 internal void EnsureManualSea() {
  if(seaMaterial==null){seaMap=Resources.Load<Texture2D>("Sprites/ManualSailing/southeast_asia");seaMaterial=new Material(Resources.Load<Shader>("Shaders/ManualSea"));
   string[] types={"square","junk","lateen"};for(int t=0;t<3;t++)for(int d=0;d<8;d++)fleetArt[t,d]=Resources.Load<Texture2D>("Sprites/ManualSailing/"+types[t]+"/"+SeaDirections[d]);
   seaCity=Resources.Load<Texture2D>("Sprites/ManualSailing/chinese_pagoda_port");seaCity2=Resources.Load<Texture2D>("Sprites/ManualSailing/japanese_pagoda_port");}
  if(seaOwner!=s||seaDeparture!=s.seaFrom){seaOwner=s;seaDeparture=s.seaFrom;seaOrigin=new Vector2((float)c.ports[s.seaFrom].x,(float)c.ports[s.seaFrom].y)-new Vector2(.38f,.32f);seaCamera=SeaPosition;seaTarget=SeaPosition;seaHasTarget=false;seaStopped=true;}
 }
 internal bool SetManualSeaTarget(Vector2 pointer) {
  EnsureManualSea();if(pointer.y<190||pointer.y>930||pointer.x<0||pointer.x>1920)return false;
  var target=SeaProjection.Unproject(pointer,seaCamera);
  target.x=Mathf.Clamp(target.x,(float)c.balance.navigation.mapMinX,(float)c.balance.navigation.mapMaxX);
  target.y=Mathf.Clamp(target.y,(float)c.balance.navigation.mapMinY,(float)c.balance.navigation.mapMaxY);
  seaTarget=target;seaHasTarget=true;seaStopped=false;return true;
 }
 internal void TickManualSea(double seconds) {
  EnsureManualSea();
  if(seaHasTarget&&!seaStopped){var distance=Vector2.Distance(SeaPosition,seaTarget);if(distance<.0005f){seaHasTarget=false;seaStopped=true;}else g.SteerToward(seaTarget.x,seaTarget.y,seconds);}
  double speed=g.Sail(seconds,seaStopped?0:throttle);
  if(seaHasTarget&&Vector2.Distance(SeaPosition,seaTarget)<Mathf.Max(.0005f,(float)(speed*seconds*1.5))){seaHasTarget=false;seaStopped=true;}
  seaCamera=SeaProjection.Follow(SeaPosition,seaCamera);
 }
 void ManualSeaBackdrop() {
  EnsureManualSea();seaMaterial.SetVector("_Center",seaCamera-seaOrigin);seaMaterial.SetVector("_Projection",new Vector4(SeaProjection.MapWidth,SeaProjection.Depth,0,0));
  GUI.DrawTexture(new Rect(0,0,1920,1080),seaMap,ScaleMode.StretchToFill,seaMaterial);
  // Coastal scenery belongs to the Southeast Asia art, not to legacy world ports.
  DrawSeaCity(new Vector2(.326f,.30f),seaCity,"海岸聚落");DrawSeaCity(new Vector2(.44f,.43f),seaCity2,"岛屿港湾");
  if(seaHasTarget){var p=SeaProjection.Project(seaTarget,seaCamera);if(p.y>190&&p.y<930&&p.x>20&&p.x<1900){
   float radius=18+Mathf.Sin(Time.time*3)*3;for(int k=0;k<24;k++){float a=k*Mathf.PI/12,b=(k+1)*Mathf.PI/12;Line(p+new Vector2(Mathf.Cos(a),Mathf.Sin(a)*.5f)*radius,p+new Vector2(Mathf.Cos(b),Mathf.Sin(b)*.5f)*radius,gold,2);}Label(p.x+24,p.y-20,140,35,"航行目标",small);}}
  int direction=((int)Mathf.Round((float)s.heading*Mathf.Rad2Deg/45)+8)%8;
  Vector2 forward=new Vector2(Mathf.Cos((float)s.heading),Mathf.Sin((float)s.heading));Vector2 side=new Vector2(-forward.y,forward.x);
  var anchor=SeaProjection.Project(SeaPosition,seaCamera);var offsets=new[]{new Vector2(-90,-10),new Vector2(-30,32),new Vector2(30,-32),new Vector2(90,10)};var points=new Vector2[4];for(int i=0;i<4;i++)points[i]=anchor+offsets[i];
  int[] order={0,1,2,3};System.Array.Sort(order,(a,b)=>points[a].y.CompareTo(points[b].y));
  foreach(int i in order){var p=points[i];float scale=Mathf.Clamp((p.y-162)/378,.65f,1.3f);float size=(i==3?72:80)*Mathf.Min(scale,1.1f);
   Vector2 tail=new Vector2(-forward.x,-forward.y*.707f).normalized;
   for(int w=0;w<3;w++){var v=p+tail*(18+w*10);Line(v-new Vector2(10+w*4,0),v+new Vector2(10+w*4,0),new Color(.7f,.94f,1,.25f-w*.06f),2);}
   GUI.color=i==3?new Color(.83f,.91f,1):Color.white;GUI.DrawTexture(new Rect(p.x-size*.55f,p.y-size*.88f+Mathf.Sin(Time.time*1.8f+i)*1.5f,size*1.1f,size),fleetArt[i%3,direction],ScaleMode.ScaleToFit);GUI.color=Color.white;
  }
 }
 void DrawSeaCity(Vector2 mapPoint,Texture2D art,string title){var p=SeaProjection.Project(mapPoint+seaOrigin,seaCamera);if(p.x< -100||p.x>2020||p.y<200||p.y>1000)return;float size=Mathf.Clamp((p.y-162)/378, .2f,1.5f)*150;GUI.DrawTexture(new Rect(p.x-size/2,p.y-size,size,size),art,ScaleMode.ScaleToFit);Label(p.x-80,p.y,180,32,title,small);}
}
}
