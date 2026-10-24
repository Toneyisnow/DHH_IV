using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
 public partial class MaritimeGame {
  void OrdinarySailingInput(double seconds) {
   if(g.DialogueOpen||settingsOpen||confirm!=""||s.pendingEvents.Count>0||g.PromptedEncounter!=null)return;
   EnsureManualSea();
   if(Input.GetMouseButtonDown(0)&&!LiveGui.PointerOverControl)SetManualSeaTarget(LiveGui.Mouse);
   if(Input.GetKeyDown(KeyCode.Space))seaStopped=!seaStopped;
   double axis=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
   ApplyManualSailInput(axis,seconds,Input.GetKeyDown(KeyCode.R));
   if(Input.GetKey(KeyCode.W))throttle=Mathf.Min(1,throttle+(float)seconds);
   if(Input.GetKey(KeyCode.S))throttle=Mathf.Max(c.balance.ui.minimumThrottle,throttle-(float)seconds);
   TickManualSea(seconds);
   foreach(var e in s.sailing.encounters)if(g.ContactSeaEncounter(e.instanceId)){Save(true);break;}
  }
 }
}
