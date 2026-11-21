using UnityEngine;
namespace MaritimeLegend {
public sealed class RegionalCoastalPlacement:MonoBehaviour {
 public enum Kind { Dock,MooredBoat,Shipyard }
 public Kind kind;
 // Authored XZ footprint relative to this object. The sample shoreline is local Z=0, water Z<0.
 public Rect footprint=new Rect(-1,-2,2,2);
 public bool RequiresWater=>true;
 public bool IsOnWater(Transform layout,float shorelineZ=0){
  foreach(var p in new[]{new Vector3(footprint.xMin,0,footprint.yMin),new Vector3(footprint.xMax,0,footprint.yMin),new Vector3(footprint.xMin,0,footprint.yMax),new Vector3(footprint.xMax,0,footprint.yMax)})
   if(layout.InverseTransformPoint(transform.TransformPoint(p)).z>shorelineZ+.001f)return false;
  return true;
 }
}
}
