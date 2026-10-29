using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
 // The wind vector points toward its destination, as in GameService.Sail.
 public static class SeaInstrumentMath {
  public static Vector2 WorldDirection(double heading)=>new Vector2(Mathf.Cos((float)heading)*CoastNavigation.Width,Mathf.Sin((float)heading)*CoastNavigation.Height).normalized;
  public static Vector2 SailNormal(double heading,double trim){var forward=WorldDirection(heading);float a=(float)SailingFormulas.SailDegrees(trim)*Mathf.Deg2Rad;return new Vector2(forward.x*Mathf.Cos(a)-forward.y*Mathf.Sin(a),forward.x*Mathf.Sin(a)+forward.y*Mathf.Cos(a));}
  public static float WindFill(double heading,double trim,double wind)=>(float)SailingFormulas.WindFill(heading,trim,wind);
  public static int FillState(float fill)=>fill<.15f?0:fill<.45f?1:fill<.78f?2:3;
  public static float WheelStep(float angle,double axis,double seconds)=>Mathf.Repeat(angle+(float)(axis*seconds)*600,360);
 }
 public partial class MaritimeGame {
  static readonly Rect InstrumentPanelRect=new Rect(1684,208,216,420);
  static readonly string[] SailStateNames={"没风","小顺风","中顺风","大顺风"};
  Texture2D instrumentFrame,instrumentShip,instrumentHelm;readonly Texture2D[] instrumentSails=new Texture2D[4];
  GUIStyle instrumentCaption;float instrumentWheelAngle;
  internal int InstrumentSailState=>SeaInstrumentMath.FillState(throttle*SeaInstrumentMath.WindFill(s.heading,s.trim,s.wind));
  internal float InstrumentWheelAngle=>instrumentWheelAngle;
  internal bool OverSailingInstruments(Vector2 pointer)=>InstrumentPanelRect.Contains(pointer);
  internal void ApplyManualSailInput(double axis,double seconds,bool reset){
   g.AdjustSail(axis,seconds,reset);
   instrumentWheelAngle=reset?0:SeaInstrumentMath.WheelStep(instrumentWheelAngle,axis,seconds);
  }
  Vector2 InstrumentScreenDirection(Vector2 direction){var uv=new Vector2(direction.x/CoastNavigation.Width,direction.y/CoastNavigation.Height);return (indianOcean.Project(SeaPosition+uv*10)-indianOcean.Project(SeaPosition)).normalized;}
  internal float InstrumentShipAngle {get{var d=InstrumentScreenDirection(SeaInstrumentMath.WorldDirection(s.heading));return Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg+90;}}
  void InstrumentSprite(Texture2D art,Vector2 center,float size,float angle){var matrix=GUI.matrix;LiveGuiUtility.RotateAroundPivot(angle,center);GUI.DrawTexture(new Rect(center.x-size/2,center.y-size/2,size,size),art);GUI.matrix=matrix;}
  void SailingInstruments(){
   if(!instrumentFrame){const string folder="Sprites/UI/ManualSailing/Instruments/";instrumentFrame=Resources.Load<Texture2D>(folder+"instrument_frame");instrumentShip=Resources.Load<Texture2D>(folder+"ship_top");instrumentHelm=Resources.Load<Texture2D>(folder+"helm");for(int i=0;i<4;i++)instrumentSails[i]=Resources.Load<Texture2D>(folder+"sail_"+i);}
   if(instrumentCaption==null){instrumentCaption=new GUIStyle(small){fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=false};instrumentCaption.normal.textColor=new Color(.88f,.82f,.64f);}
   Fill(InstrumentPanelRect,new Color(.055f,.075f,.060f,.83f));
   var top=new Rect(1696,216,192,192);var bottom=new Rect(1696,420,192,192);
   GUI.DrawTexture(top,instrumentFrame);GUI.DrawTexture(bottom,instrumentFrame);
   var center=top.center+new Vector2(0,-16);
   InstrumentSprite(instrumentShip,center,146,InstrumentShipAngle);
   var sailDirection=InstrumentScreenDirection(SeaInstrumentMath.SailNormal(s.heading,s.trim));
   InstrumentSprite(instrumentSails[InstrumentSailState],center,146,Mathf.Atan2(sailDirection.y,sailDirection.x)*Mathf.Rad2Deg+90);
   // Small blue arrow is the real wind direction transformed by the same orbit camera.
   var wind=InstrumentScreenDirection(SeaInstrumentMath.WorldDirection(s.wind));var side=new Vector2(-wind.y,wind.x);
   var anchor=center+side*62;var tip=anchor+wind*12;var color=new Color(.49f,.77f,.78f);
   Line(anchor-wind*12,tip,color,2);Line(tip,tip-wind*7+side*5,color,2);Line(tip,tip-wind*7-side*5,color,2);
   GUI.Label(new Rect(top.x+10,top.y+150,172,27),"船帆 · "+SailStateNames[InstrumentSailState],instrumentCaption);
   InstrumentSprite(instrumentHelm,bottom.center+new Vector2(0,-16),146,instrumentWheelAngle);
   GUI.Label(new Rect(bottom.x+10,bottom.y+150,172,27),"船舵 · A/D",instrumentCaption);
  }
 }
}
