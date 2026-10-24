using System;
using UnityEngine;
namespace MaritimeLegend {
// Copy one imported tile at a time. Keep only the GPU array, not 28 CPU pixel buffers.
public static class SeaMapTiles {
 public static Texture2DArray Load(){
  var array=new Texture2DArray(1088,1088,28,TextureFormat.RGBA32,true,false){name="SEA 4x registered map tiles",filterMode=FilterMode.Trilinear,wrapMode=TextureWrapMode.Clamp,anisoLevel=8};
  array.Apply(false,true);
  for(int i=0;i<28;i++){
   var tile=Resources.Load<Texture2D>("Textures/ManualSailing/Tiles/map_"+i.ToString("00"));
   if(tile==null||tile.width!=1088||tile.height!=1088||tile.format!=TextureFormat.RGBA32)throw new InvalidOperationException("Map tile import mismatch: "+i);
   Graphics.CopyTexture(tile,0,array,i);
   Resources.UnloadAsset(tile);
  }
  Debug.Log("SEA_TILES_PASS: 28 RGBA32 tiles at 1088, effective map 6688 x 3764");
  return array;
 }
}
}
