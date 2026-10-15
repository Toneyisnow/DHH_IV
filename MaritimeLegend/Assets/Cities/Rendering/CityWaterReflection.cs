using UnityEngine;
namespace MaritimeLegend {
[RequireComponent(typeof(Camera))]
public sealed class CityWaterReflection:MonoBehaviour {
 public Material water;public Renderer waterRenderer;public float waterHeight;Camera source,reflection;RenderTexture texture;bool rendering;
 public RenderTexture ReflectionTexture=>texture;public int ReflectionMask=>reflection==null?0:reflection.cullingMask;
 void Awake(){source=GetComponent<Camera>();}
 void OnPreCull(){if(rendering||water==null||waterRenderer==null)return;rendering=true;try{if(reflection==null){var go=new GameObject("Water reflection camera");go.transform.SetParent(transform);reflection=go.AddComponent<Camera>();reflection.enabled=false;texture=new RenderTexture(768,400,16,RenderTextureFormat.ARGBHalf);texture.wrapMode=TextureWrapMode.Clamp;texture.filterMode=FilterMode.Bilinear;texture.Create();}reflection.CopyFrom(source);reflection.enabled=false;reflection.targetTexture=texture;reflection.depthTextureMode=DepthTextureMode.None;reflection.cullingMask=source.cullingMask&~(1<<22);reflection.useOcclusionCulling=false;
 var mirror=Matrix4x4.identity;mirror.m11=-1;mirror.m13=2*waterHeight;reflection.worldToCameraMatrix=source.worldToCameraMatrix*mirror;Vector3 p=source.transform.position;p.y=2*waterHeight-p.y;reflection.transform.position=p;var point=reflection.worldToCameraMatrix.MultiplyPoint(new Vector3(0,waterHeight+.02f,0));var normal=reflection.worldToCameraMatrix.MultiplyVector(Vector3.up).normalized;var plane=new Vector4(normal.x,normal.y,normal.z,-Vector3.Dot(point,normal));reflection.projectionMatrix=source.CalculateObliqueMatrix(plane);bool culling=GL.invertCulling;bool wasVisible=waterRenderer.enabled;try{GL.invertCulling=!culling;waterRenderer.enabled=false;reflection.Render();}finally{waterRenderer.enabled=wasVisible;GL.invertCulling=culling;}water.SetTexture("_ReflectionTex",texture);water.SetMatrix("_ReflectionVP",GL.GetGPUProjectionMatrix(reflection.projectionMatrix,true)*reflection.worldToCameraMatrix);
 }finally{rendering=false;}}
 void OnDestroy(){if(texture!=null){texture.Release();Destroy(texture);}if(reflection!=null)Destroy(reflection.gameObject);}
}
}
