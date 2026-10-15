using UnityEditor;
using UnityEngine;
namespace MaritimeLegend.Editor {
 public sealed class OpeningImport : AssetPostprocessor {
  void OnPreprocessTexture(){
   if(!assetPath.StartsWith("Assets/Resources/Opening/")&&!assetPath.StartsWith("Assets/Resources/SaveSelection/"))return;
   var t=(TextureImporter)assetImporter;
   t.textureType=TextureImporterType.Default;t.npotScale=TextureImporterNPOTScale.None;
   t.maxTextureSize=4096;t.textureCompression=TextureImporterCompression.Uncompressed;
   t.alphaIsTransparency=true;t.mipmapEnabled=assetPath.Contains("world_map");
   t.wrapMode=TextureWrapMode.Clamp;t.filterMode=FilterMode.Bilinear;
   if(assetPath.Contains("world_map")){t.wrapModeU=TextureWrapMode.Repeat;t.wrapModeV=TextureWrapMode.Clamp;t.alphaIsTransparency=false;}
  }
 }
}
