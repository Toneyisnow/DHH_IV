using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
namespace MaritimeLegend {
[ExecuteAlways]
public sealed class ManualSeaV2Environment : MonoBehaviour {
 public enum SkyMode {Sunny,Sunset,Night}
 public SkyMode mode;
 public Material[] skies;
 public Material water;
 public Light sun;
 public Camera view;
 public bool showControls=true;
 public string mapTitle="Manual Sailing V2 | 640 x 480";
 SkyMode applied=(SkyMode)(-1);
 Material liveWater; Material originalWater;
 Material oldSky;Color oldAmbient;UnityEngine.Rendering.AmbientMode oldAmbientMode;
 int oldVsync;float oldShadowDistance;
 void OnEnable(){oldSky=RenderSettings.skybox;oldAmbient=RenderSettings.ambientLight;oldAmbientMode=RenderSettings.ambientMode;
  if(Application.isPlaying){oldVsync=QualitySettings.vSyncCount;oldShadowDistance=QualitySettings.shadowDistance;QualitySettings.vSyncCount=0;QualitySettings.shadowDistance=230;
   originalWater=water;liveWater=new Material(water);water=liveWater;
   foreach(var renderer in GetComponentsInChildren<MeshRenderer>())if(renderer.sharedMaterial==originalWater)renderer.sharedMaterial=liveWater;
  }
  Apply();
 }
 void OnDisable(){if(Application.isPlaying){QualitySettings.vSyncCount=oldVsync;QualitySettings.shadowDistance=oldShadowDistance;RenderSettings.skybox=oldSky;RenderSettings.ambientMode=oldAmbientMode;RenderSettings.ambientLight=oldAmbient;
  if(liveWater){foreach(var renderer in GetComponentsInChildren<MeshRenderer>())if(renderer.sharedMaterial==liveWater)renderer.sharedMaterial=originalWater;water=originalWater;Destroy(liveWater);}}
 }
 void OnValidate(){applied=(SkyMode)(-1);}
 void Update(){
#if ENABLE_INPUT_SYSTEM
  if(Application.isPlaying&&Keyboard.current!=null){if(Keyboard.current.digit1Key.wasPressedThisFrame)mode=SkyMode.Sunny;if(Keyboard.current.digit2Key.wasPressedThisFrame)mode=SkyMode.Sunset;if(Keyboard.current.digit3Key.wasPressedThisFrame)mode=SkyMode.Night;}
#elif ENABLE_LEGACY_INPUT_MANAGER
  if(Application.isPlaying){if(Input.GetKeyDown(KeyCode.Alpha1))mode=SkyMode.Sunny;if(Input.GetKeyDown(KeyCode.Alpha2))mode=SkyMode.Sunset;if(Input.GetKeyDown(KeyCode.Alpha3))mode=SkyMode.Night;}
#endif
  if(applied!=mode)Apply();
  if(water&&view)water.SetVector("_ViewForward",view.transform.forward);
 }
 public void Apply(){
  if(skies==null||skies.Length<3||!water||!sun)return;applied=mode;int i=(int)mode;
  RenderSettings.skybox=skies[i];RenderSettings.sun=sun;RenderSettings.fog=false;
  RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
  RenderSettings.ambientLight=i==0?new Color(.43f,.52f,.60f):i==1?new Color(.40f,.25f,.28f):new Color(.12f,.18f,.30f);
  sun.transform.rotation=Quaternion.Euler(i==0?48:i==1?8:6,i==0?-35:180,0);
  sun.color=i==0?new Color(1,.97f,.87f):i==1?new Color(1,.40f,.13f):new Color(.56f,.70f,1);
  sun.intensity=i==0?1.05f:i==1?1.1f:.65f;
  water.SetColor("_DeepColor",i==0?new Color(.035f,.43f,.69f):i==1?new Color(.12f,.25f,.37f):new Color(.025f,.085f,.18f));
  water.SetColor("_ShallowColor",i==0?new Color(.05f,.73f,.73f):i==1?new Color(.26f,.42f,.42f):new Color(.04f,.24f,.30f));
  water.SetColor("_SkyTint",i==0?new Color(.32f,.66f,.94f):i==1?new Color(1,.31f,.12f):new Color(.11f,.17f,.35f));
  water.SetColor("_SunColor",i==0?new Color(1,.97f,.80f):i==1?new Color(1,.55f,.16f):new Color(.62f,.78f,1));
  water.SetVector("_SunDirection",-sun.transform.forward);water.SetFloat("_Evening",i==1?1:0);water.SetFloat("_Night",i==2?1:0);
  if(view){view.clearFlags=CameraClearFlags.Skybox;water.SetVector("_ViewForward",view.transform.forward);}
 }
 float fps;
 void OnGUI(){if(!Application.isPlaying||!showControls)return;fps=Mathf.Lerp(fps,1/Mathf.Max(Time.unscaledDeltaTime,.0001f),.04f);
  GUI.Box(new Rect(12,12,385,106),mapTitle);
  if(GUI.Button(new Rect(22,39,115,25),"1  Sunny"))mode=SkyMode.Sunny;
  if(GUI.Button(new Rect(142,39,115,25),"2  Sunset"))mode=SkyMode.Sunset;
  if(GUI.Button(new Rect(262,39,115,25),"3  Starry night"))mode=SkyMode.Night;
  GUI.Label(new Rect(22,68,365,23),"WASD move | RMB drag orbit | Wheel zoom | R reset");
  GUI.Label(new Rect(22,91,365,23),$"{fps:F0} FPS | Pitch: {view.GetComponent<ManualSeaV2Camera>().pitch:F0} degrees");
 }
}
}

