using UnityEngine;
using UnityEngine.UI;
namespace MaritimeLegend {
 // Subdivision keeps the texture continuous over the table's projective surface.
 public sealed class TableQuad : BaseMeshEffect {
  public Vector2[] corners=new Vector2[4];
  public void Set(Vector2[] points){for(int i=0;i<4;i++)corners[i]=points[i];graphic.SetVerticesDirty();}
  public override void ModifyMesh(VertexHelper vh){
   if(!IsActive())return;
   var image=(RawImage)graphic;var r=image.rectTransform.rect;vh.Clear();
   const int n=12;
   for(int y=0;y<=n;y++)for(int x=0;x<=n;x++){
    float u=x/(float)n,v=y/(float)n;
    var p=Vector2.Lerp(Vector2.Lerp(corners[0],corners[1],u),Vector2.Lerp(corners[3],corners[2],u),v);
    vh.AddVert(new Vector3(r.x+p.x,r.yMax-p.y),image.color,new Vector2(u,1-v));
   }
   for(int y=0;y<n;y++)for(int x=0;x<n;x++){int a=y*(n+1)+x;vh.AddTriangle(a,a+n+1,a+1);vh.AddTriangle(a+1,a+n+1,a+n+2);}
  }
 }
}
