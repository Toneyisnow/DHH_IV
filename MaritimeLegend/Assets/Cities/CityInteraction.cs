using UnityEngine;
namespace MaritimeLegend {
public sealed class CityInteraction : MonoBehaviour,ICityInteraction {
 public string label,destination; public Vector2 screenPosition;
 public string Label=>label;public Vector2 Position=>screenPosition;public string Destination=>destination=="海图"?"港口":destination;
 public void Interact(MaritimeGame game){game.WalkTo(this);}
}
}
