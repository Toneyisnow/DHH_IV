using UnityEngine;
namespace MaritimeLegend {
public interface ICityInteraction {string Label{get;} Vector2 Position{get;} string Destination{get;} void Interact(MaritimeGame game);}
}
