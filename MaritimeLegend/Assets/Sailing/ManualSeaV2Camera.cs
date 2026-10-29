using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace MaritimeLegend {
[ExecuteAlways, RequireComponent(typeof(Camera))]
public sealed class ManualSeaV2Camera : MonoBehaviour {
 [Range(10,60)] public float pitch=35;
 [Range(.05f,.3f)] public float skyHeightFraction=.15f;
 public float yaw=0;
 public Vector3 focus=new Vector3(0,0,-40);
 [Range(24,120)] public float distance=115;
 public Vector4 focusBounds=new Vector4(-310,-230,310,230);
 public Vector3 homeFocus=new Vector3(0,0,-40);
 public float homeDistance=115;
 public float maximumDistance=120;
 [Range(.05f,1)] public float dragSensitivity=.22f;
 Camera view;
 public float Pitch {get=>pitch;set{pitch=Mathf.Clamp(value,10,60);ApplyPose();}}
 public void ApplyPose(){if(!view)view=GetComponent<Camera>();view.orthographic=false;view.fieldOfView=2*Mathf.Atan(Mathf.Tan(pitch*Mathf.Deg2Rad)/(1-2*skyHeightFraction))*Mathf.Rad2Deg;transform.rotation=Quaternion.Euler(pitch,yaw,0);transform.position=focus-transform.forward*distance;}
 void OnEnable(){ApplyPose();}
 void OnValidate(){ApplyPose();}
 void LateUpdate(){
  if(!Application.isPlaying){ApplyPose();return;}
  float x=0,z=0,scroll=0,drag=0;bool reset=false,fleet=false;
#if ENABLE_INPUT_SYSTEM
  var k=Keyboard.current;var m=Mouse.current;
  if(k!=null){x=(k.dKey.isPressed?1:0)-(k.aKey.isPressed?1:0);z=(k.wKey.isPressed?1:0)-(k.sKey.isPressed?1:0);reset=k.rKey.wasPressedThisFrame;fleet=k.fKey.wasPressedThisFrame;}
  if(m!=null){scroll=m.scroll.ReadValue().y/120f;if(m.rightButton.isPressed)drag=m.delta.ReadValue().x;}
#elif ENABLE_LEGACY_INPUT_MANAGER
  x=(Input.GetKey(KeyCode.D)?1:0)-(Input.GetKey(KeyCode.A)?1:0);z=(Input.GetKey(KeyCode.W)?1:0)-(Input.GetKey(KeyCode.S)?1:0);
  scroll=Input.mouseScrollDelta.y;if(Input.GetMouseButton(1))drag=Input.GetAxisRaw("Mouse X")*10;reset=Input.GetKeyDown(KeyCode.R);fleet=Input.GetKeyDown(KeyCode.F);
#endif
  yaw+=drag*dragSensitivity;distance=Mathf.Clamp(distance*Mathf.Exp(-scroll*.12f),24,maximumDistance);
  var heading=Quaternion.Euler(0,yaw,0);focus+=heading*new Vector3(x,0,z)*(distance*.5f*Time.unscaledDeltaTime);
  focus.x=Mathf.Clamp(focus.x,focusBounds.x,focusBounds.z);focus.z=Mathf.Clamp(focus.z,focusBounds.y,focusBounds.w);
  if(reset){pitch=35;yaw=0;distance=homeDistance;focus=homeFocus;}
  if(fleet){focus=homeFocus;distance=60;}
  ApplyPose();
 }
}
}


