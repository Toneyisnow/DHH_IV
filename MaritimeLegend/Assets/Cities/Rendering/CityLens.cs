using UnityEngine;
namespace MaritimeLegend {
[RequireComponent(typeof(Camera))]
public sealed class CityLens : MonoBehaviour {
 public bool debugDepth;public Shader shader;public Transform focus;public float sharpBand=5.5f,blur=4.5f,fog=.15f;Material material;Camera view;
 void Awake(){view=GetComponent<Camera>();view.depthTextureMode|=DepthTextureMode.Depth;}
 void OnRenderImage(RenderTexture src,RenderTexture dst){if(material==null&&shader!=null)material=new Material(shader);if(material==null){Graphics.Blit(src,dst);return;}float distance=focus==null?30:Vector3.Dot(focus.position-view.transform.position,view.transform.forward);material.SetFloat("_DebugDepth",debugDepth?1:0);material.SetFloat("_Focus",distance);material.SetFloat("_Band",sharpBand);material.SetFloat("_Blur",blur);material.SetFloat("_Fog",fog);Graphics.Blit(src,dst,material);}
 void OnDestroy(){if(material!=null)Destroy(material);}
}
}
