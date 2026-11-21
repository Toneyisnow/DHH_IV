using UnityEngine;
namespace MaritimeLegend {
public sealed class RegionalBoatFloat:MonoBehaviour {
 public float amplitude=.09f,period=4.8f,phase,roll=.65f;
 Vector3 origin;Quaternion rotation;bool ready;
 void Initialize(){if(ready)return;origin=transform.localPosition;rotation=transform.localRotation;ready=true;}
 public void Sample(float time){Initialize();float a=time*Mathf.PI*2/Mathf.Max(period,.1f)+phase;transform.localPosition=origin+Vector3.up*(Mathf.Sin(a)*amplitude);transform.localRotation=rotation*Quaternion.Euler(0,0,Mathf.Sin(a+.6f)*roll);}
 void Awake(){Initialize();}void Update(){Sample(Time.time);}
}
}
