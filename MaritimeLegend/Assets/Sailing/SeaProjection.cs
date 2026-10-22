using UnityEngine;
namespace MaritimeLegend {
// Shared by picking, sprites and the sea shader. Coordinates are top-left map UVs.
public static class SeaProjection {
 public const float Horizon=162, FocusY=540, MapWidth=16720, MapHeight=9410;
 public const float Depth=378f/(MapHeight*.70710678f*.75f);
 public static Vector2 Unproject(Vector2 screen,Vector2 camera) {
  float q=Mathf.Max(.015f,(screen.y-Horizon)/378f);
  return camera+new Vector2((screen.x-960)/(MapWidth*q),Depth*(1-Mathf.Pow(q,-.75f)));
 }
 public static Vector2 Project(Vector2 world,Vector2 camera) {
  var d=world-camera;float divisor=1-d.y/Depth;
  if(divisor<=.02f)return new Vector2(-10000,10000);
  float q=Mathf.Pow(divisor,-1/.75f);
  return new Vector2(960+d.x*MapWidth*q,Horizon+378*q);
 }
 public static Vector2 Follow(Vector2 ship,Vector2 camera) {
  var p=Project(ship,camera);var bounded=new Vector2(Mathf.Clamp(p.x,910,1010),Mathf.Clamp(p.y,550,570));
  return camera+ship-Unproject(bounded,camera);
 }
}
}
