using UnityEngine;
namespace MaritimeLegend {
public sealed class RegionalRoadTile:MonoBehaviour {
 // N=1 (+Z), E=2 (+X), S=4, W=8. Rotation must snap to multiples of 90 degrees.
 public int connections;public float tileSize=4,roadWidth=2;
 public int RotatedConnections(int clockwiseQuarterTurns){int turns=((clockwiseQuarterTurns%4)+4)%4;int mask=connections&15;return ((mask<<turns)|(mask>>(4-turns)))&15;}
}
}
