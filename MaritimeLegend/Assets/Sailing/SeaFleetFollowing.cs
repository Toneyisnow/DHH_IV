using System;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
 public static class SeaFleetFollowing {
  public static ManualFollowerState[] Spawn(Vector2 position,double heading) => new[] {
   new ManualFollowerState{x=position.x,y=position.y,heading=heading},
   new ManualFollowerState{x=position.x,y=position.y,heading=heading}
  };
  public static void Step(ManualFollowerState[] followers,Vector2 primary,float primaryPixelsPerSecond,double seconds,double turnRate,CoastNavigation coast) {
   if(seconds<=0)return;
   var leader=coast.Pixels(primary);
   // Slack at low speed lets the fleet gather; under way the line stretches to 2.25 pixels.
   float gap=2.25f*Mathf.Clamp01(primaryPixelsPerSecond/2.4f);
   foreach(var ship in followers) {
    var position=coast.Pixels(new Vector2((float)ship.x,(float)ship.y));
    var offset=leader-position;float distance=offset.magnitude;
    if(distance>.001f) {
     double target=Math.Atan2(offset.y,offset.x);
     ship.heading=SailingFormulas.Turn(ship.heading,target,turnRate,seconds);
     double error=Math.Atan2(Math.Sin(target-ship.heading),Math.Cos(target-ship.heading));
     // Turn on the spot when facing away. Never snap to a point behind the leader.
     float alignment=Mathf.Max(0,(float)Math.Cos(error));
     float travel=Mathf.Min(Mathf.Max(0,distance-gap),(float)(Math.Max(2.4,primaryPixelsPerSecond*1.5)*seconds)*alignment);
     var direction=new Vector2((float)Math.Cos(ship.heading),(float)Math.Sin(ship.heading));
     position=coast.MovePixels(position,direction*travel);
     var uv=coast.UV(position);ship.x=uv.x;ship.y=uv.y;
    }
    leader=position;
   }
  }
 }
}
