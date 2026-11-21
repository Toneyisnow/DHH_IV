using UnityEngine;

namespace MaritimeLegend {
// Local flight loop. Vegetation and fabric motion lives in RegionalCityElement.shader.
// Safe on nested prefab instances: the orbit is relative to its authored local position.
public sealed class RegionalCityMotion : MonoBehaviour {
 public float radius=2.4f, period=24f, bob=.16f, phase;
 Vector3 origin, authoredScale;
 bool initialized;
 void Awake(){Initialize();}
 void Initialize(){if(initialized)return;origin=transform.localPosition;authoredScale=transform.localScale;initialized=true;}
 public void Sample(float seconds){
  Initialize();float angle=seconds*Mathf.PI*2/Mathf.Max(period,1)+phase;
  transform.localPosition=origin+new Vector3(Mathf.Sin(angle)*radius,Mathf.Sin(angle*2)*bob,Mathf.Cos(angle)*.35f);
  // Turn at the ends of the flight path; a slight bank avoids a rigid hovering cutout.
  transform.localScale=new Vector3(authoredScale.x*(Mathf.Cos(angle)<0?-1:1),authoredScale.y,authoredScale.z);
 }
 void Update(){Sample(Time.time);}
}
}
