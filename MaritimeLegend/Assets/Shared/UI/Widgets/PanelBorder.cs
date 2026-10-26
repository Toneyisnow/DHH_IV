using UnityEngine;
using UnityEngine.UI;
namespace MaritimeLegend.UI.Widgets {
// Four non-overlapping inner strips: no filled duplicate rectangle or layout expansion.
public sealed class PanelBorder : MaskableGraphic {
 public float thickness=2;
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float t=Mathf.Min(thickness,Mathf.Min(r.width,r.height)/2);Quad(vh,r.xMin,r.yMin,r.width,t);Quad(vh,r.xMin,r.yMax-t,r.width,t);Quad(vh,r.xMin,r.yMin+t,t,r.height-2*t);Quad(vh,r.xMax-t,r.yMin+t,t,r.height-2*t);}
 void Quad(VertexHelper vh,float x,float y,float w,float h){int i=vh.currentVertCount;vh.AddVert(new Vector3(x,y),color,Vector2.zero);vh.AddVert(new Vector3(x,y+h),color,Vector2.zero);vh.AddVert(new Vector3(x+w,y+h),color,Vector2.zero);vh.AddVert(new Vector3(x+w,y),color,Vector2.zero);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
}
}
