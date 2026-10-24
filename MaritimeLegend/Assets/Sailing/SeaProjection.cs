using UnityEngine;
namespace MaritimeLegend {
// Orthographic 45-degree camera: uniform ground scale, no horizon-dependent texture stretching.
public static class SeaProjection {
 public const float CameraZoom=1.3f,CameraHalfHeight=54/CameraZoom;
 public const float Horizon=162,FocusY=540,MapWidth=16720*CameraZoom,MapHeight=9410*CameraZoom;
 public static Vector2 Unproject(Vector2 screen,Vector2 camera)=>camera+new Vector2((screen.x-960)/MapWidth,(screen.y-FocusY)/(MapHeight*.70710678f));
 public static Vector2 Project(Vector2 world,Vector2 camera){var d=world-camera;return new Vector2(960+d.x*MapWidth,FocusY+d.y*MapHeight*.70710678f);}
 public static Vector2 Follow(Vector2 ship,Vector2 camera){var p=Project(ship,camera);var bounded=new Vector2(Mathf.Clamp(p.x,910,1010),Mathf.Clamp(p.y,550,570));return camera+ship-Unproject(bounded,camera);}
}
}
