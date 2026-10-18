using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
    public partial class MaritimeGame {
        // Presentation/input adapter. Domain owns heading, trim, calendar and encounters.
        void OrdinarySailingInput(double seconds) {
            if(g.DialogueOpen||settingsOpen||confirm!="")return;
            var mouse=LiveGui.Mouse;
            if(Input.GetMouseButton(0)&&new Rect(65,215,1300,520).Contains(mouse))
                g.SteerToward((mouse.x-65-mapPan.x)/(1300*mapZoom),(mouse.y-150-mapPan.y)/(700*mapZoom),seconds);
            double axis=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
            g.AdjustSail(axis,seconds,Input.GetKeyDown(KeyCode.R));
            if(Input.GetKey(KeyCode.W))throttle=Mathf.Min(1,throttle+(float)seconds);
            if(Input.GetKey(KeyCode.S))throttle=Mathf.Max(c.balance.ui.minimumThrottle,throttle-(float)seconds);
            g.Sail(seconds,throttle);
            foreach(var e in s.sailing.encounters)if(g.ContactSeaEncounter(e.instanceId)){Save(true);break;}
        }
    }
}
