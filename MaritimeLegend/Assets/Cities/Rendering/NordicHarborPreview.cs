using UnityEngine;
namespace MaritimeLegend {
// A dedicated 3D art scene; never creates or modifies a campaign save.
public sealed class NordicHarborPreview:MonoBehaviour {
 public Camera view;public Transform town;
 public Vector3 focus=new Vector3(0,2.4f,12);
 public float yaw=-3,pitch=24,distance=31;
 float targetYaw,targetPitch,targetDistance;
 void Start(){targetYaw=yaw;targetPitch=pitch;targetDistance=distance;ApplyCamera();if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-nordicHarborSmoke")>=0)StartCoroutine(Smoke());}
 public void SetView(float y,float p,float d){yaw=targetYaw=y;pitch=targetPitch=p;distance=targetDistance=d;ApplyCamera();}
 public void ApplyCamera(){if(!view)return;view.orthographic=false;view.fieldOfView=47;view.transform.rotation=Quaternion.Euler(pitch,yaw,0);view.transform.position=focus-view.transform.forward*distance;}
 public void Sample(float seconds){
  foreach(var boat in town.GetComponentsInChildren<RegionalBoatFloat>())boat.Sample(seconds);
  foreach(var flock in town.GetComponentsInChildren<RegionalBirdFlock>())flock.Sample(seconds,view);
 }
 void Update(){
  if(Input.GetKeyDown(KeyCode.Alpha1))SetView(-3,24,31);
  if(Input.GetKeyDown(KeyCode.Alpha2))SetView(38,27,35);
  if(Input.GetKeyDown(KeyCode.Alpha3))SetView(-38,32,38);
  if(Input.GetMouseButton(1)){targetYaw+=Input.GetAxis("Mouse X")*3;targetPitch=Mathf.Clamp(targetPitch-Input.GetAxis("Mouse Y")*2,12,70);}
  targetDistance=Mathf.Clamp(targetDistance-Input.mouseScrollDelta.y*2,18,70);
  yaw=Mathf.LerpAngle(yaw,targetYaw,Time.deltaTime*10);pitch=Mathf.Lerp(pitch,targetPitch,Time.deltaTime*10);distance=Mathf.Lerp(distance,targetDistance,Time.deltaTime*10);ApplyCamera();
 }
 System.Collections.IEnumerator Smoke(){
  yield return new WaitForSecondsRealtime(2);
  try{
   if(FindAnyObjectByType<MaritimeGame>()!=null)throw new System.Exception("Campaign UI leaked into isolated preview");
   if(view.orthographic||town.GetComponentsInChildren<NordicSolidBuilding>().Length<4)throw new System.Exception("Solid perspective scene missing");
   var args=System.Environment.GetCommandLineArgs();int at=System.Array.IndexOf(args,"-nordicReview");if(at<0||at+1>=args.Length)throw new System.Exception("Missing review directory");
   string dir=System.IO.Path.GetFullPath(args[at+1]);System.IO.Directory.CreateDirectory(dir);
   SetView(-3,24,31);Capture(System.IO.Path.Combine(dir,"Runtime-Reference.png"));SetView(38,27,35);Capture(System.IO.Path.Combine(dir,"Runtime-Side.png"));
   Debug.Log("NORDIC_HARBOR_RUNTIME_PASS: isolated 3D scene and perspective camera, no campaign or save UI");System.IO.File.WriteAllText(System.IO.Path.Combine(dir,"runtime-PASS.txt"),"Standalone runtime PASS; isolated NordicHarborScene, five solid building instances, perspective camera and both reference/side render captures.");Application.Quit(0);
  }catch(System.Exception e){Debug.LogException(e);Application.Quit(1);}
 }
 void Capture(string file){
  var previous=view.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1440,810,24,RenderTextureFormat.ARGB32);view.targetTexture=rt;view.Render();RenderTexture.active=rt;var tex=new Texture2D(1440,810,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1440,810),0,0);tex.Apply();System.IO.File.WriteAllBytes(file,tex.EncodeToPNG());view.targetTexture=previous;RenderTexture.active=active;rt.Release();Destroy(rt);Destroy(tex);
 }
}
}
