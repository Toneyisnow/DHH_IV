using UnityEngine;

namespace MaritimeLegend {
// One original painted atlas, six independent repeating cells. Geometry stays intact.
public static class CityPixelMaterials {
 public static void Apply(Material material,string kind) {
  int tile;Vector2 scale;
  switch(kind) {
   case "cobble":tile=0;scale=new Vector2(1f/3,1f/4.5f);break;
   case "plaster":tile=1;scale=new Vector2(1f/4,1f/6);break;
   case "roof":case "tile":tile=2;scale=new Vector2(1f/3,1f/4.5f);break;
   case "wood":tile=3;scale=new Vector2(1f/1.6f,1f/2.4f);break;
   case "leaf":case "leaflight":tile=4;scale=new Vector2(1f/2.4f,1f/3.6f);break;
   case "stone":tile=5;scale=new Vector2(1f/3,1f/4.5f);break;
   case "canvas":case "cream":material.SetFloat("_Cloth",1);return;
   default:return;
  }
  var texture=Resources.Load<Texture2D>("CityDaylight/Textures/PixelMaterials");
  if(texture==null)throw new System.InvalidOperationException("Missing city pixel material atlas");
  material.mainTexture=texture;material.SetFloat("_Painted",1);material.SetFloat("_Tile",tile);
  material.SetVector("_TileScale",new Vector4(scale.x,scale.y,0,0));
  material.color=kind=="leaflight"?new Color(1.10f,1.06f,.90f):Color.white;
  material.SetFloat("_Foliage",tile==4?1:0);
 }
}
}
