using UnityEngine;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
public partial class MaritimeGame {
 static readonly Rect CompassRect=new Rect(1684,700,216,242);
 Texture2D compassSheet;GUIStyle compassCaption;
 internal float CompassBearing {get{var d=SeaInstrumentMath.WorldDirection(s.heading);return Mathf.Repeat(Mathf.Atan2(d.x,-d.y)*Mathf.Rad2Deg,360);}}
 void SailingCompass(){
  if(!compassSheet)compassSheet=Resources.Load<Texture2D>("Sprites/UI/ManualSailing/Compass/sheet");
  if(compassCaption==null){compassCaption=new GUIStyle(small){fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=false};compassCaption.normal.textColor=new Color(.94f,.87f,.66f);}
  Fill(CompassRect,new Color(.025f,.09f,.13f,.8f));
  var center=new Vector2(1792,812);var matrix=GUI.matrix;
  LiveGuiUtility.RotateAroundPivot(CompassBearing,center);
  // First north-pointing dial, cropped from the existing 1774 x 887 atlas.
  GUI.DrawTextureRegion(new Rect(center.x-84,center.y-84,168,168),compassSheet,new Rect(70f/1774,1-402f/887,336f/1774,336f/887));
  GUI.matrix=matrix;
  GUI.Label(new Rect(1768,703,48,27),"北 N",compassCaption);
  GUI.Label(new Rect(1768,889,48,25),"南 S",compassCaption);
  GUI.Label(new Rect(1686,798,30,28),"西",compassCaption);GUI.Label(new Rect(1868,798,30,28),"东",compassCaption);
  GUI.Label(new Rect(1690,914,204,26),"船头航向 · "+(Mathf.RoundToInt(CompassBearing)%360).ToString("000")+"°",compassCaption);
 }
 void SailingPortTooltip(){
  if(paused||!GUI.enabled||LiveGui.PointerOverControl||OverSailingInstruments(LiveGui.Mouse))return;
  string name=indianOcean.HoverPort(LiveGui.Mouse);
  if(string.IsNullOrEmpty(name))return;
  var p=LiveGui.Mouse;var rect=new Rect(Mathf.Clamp(p.x+18,20,1640),Mathf.Clamp(p.y-52,160,890),240,44);
  Fill(rect,new Color(.025f,.075f,.09f,.94f));GUI.Label(rect,name,compassCaption);
 }
}
}
