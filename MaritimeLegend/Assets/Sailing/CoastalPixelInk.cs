using UnityEngine;
namespace MaritimeLegend {
// Native-resolution ink with a final edge anti-alias pass; no pixel grid or palette quantization.
[RequireComponent(typeof(Camera))]
public sealed class CoastalPixelInk:MonoBehaviour {
 public bool InkEnabled=true;
 public int RenderedFrames {get;private set;}
 Material ink;Camera geometryCamera;Shader geometryShader;
 void OnEnable(){ink=new Material(Resources.Load<Shader>("Shaders/CoastalPixelInk"));geometryShader=Resources.Load<Shader>("Shaders/CoastalOutlineGeometry");geometryCamera=new GameObject("Coastal outline geometry"){hideFlags=HideFlags.HideAndDontSave}.AddComponent<Camera>();geometryCamera.enabled=false;}
 void OnRenderImage(RenderTexture source,RenderTexture destination){
  if(!InkEnabled){Graphics.Blit(source,destination);return;}
  var outlined=RenderTexture.GetTemporary(source.width,source.height,0,RenderTextureFormat.ARGB32);outlined.filterMode=FilterMode.Bilinear;
  // Include transparent mountains and carry their fade into ink coverage, avoiding ghost outlines.
  var geometry=RenderTexture.GetTemporary(source.width,source.height,24,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear);geometry.filterMode=FilterMode.Point;
  geometryCamera.CopyFrom(GetComponent<Camera>());geometryCamera.transform.SetPositionAndRotation(transform.position,transform.rotation);geometryCamera.enabled=false;geometryCamera.targetTexture=geometry;geometryCamera.clearFlags=CameraClearFlags.SolidColor;geometryCamera.backgroundColor=new Color(.5f,.5f,1,0);geometryCamera.allowMSAA=false;geometryCamera.depthTextureMode=DepthTextureMode.None;
  geometryCamera.RenderWithShader(geometryShader,"");geometryCamera.targetTexture=null;ink.SetTexture("_CoastalGeometry",geometry);
  Graphics.Blit(source,outlined,ink,0);Graphics.Blit(outlined,destination,ink,1);RenderTexture.ReleaseTemporary(outlined);RenderTexture.ReleaseTemporary(geometry);RenderedFrames++;
 }
 void OnDisable(){if(ink)Destroy(ink);if(geometryCamera)Destroy(geometryCamera.gameObject);}
}
}
