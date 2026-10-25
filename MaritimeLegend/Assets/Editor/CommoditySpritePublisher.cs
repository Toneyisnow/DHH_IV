using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
// Deterministic atlas extraction only: no repainting or background removal.
public static class CommoditySpritePublisher {
 [Serializable] public class Sheet {public string name,path;public int columns,rows;public string[] products;}
 [Serializable] public class Manifest {public Sheet[] sheets;}
 [MenuItem("Maritime Legend/Publish Commodity Pixel Sprites")]
 public static void Publish(){
  string source=Path.GetFullPath(Path.Combine(Application.dataPath,"../../ArtSource/CommodityPixelArt"));
  var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(source,"catalog.json")));
  string destination="Assets/Resources/Sprites/Commodities";Directory.CreateDirectory(destination);
  var ids=manifest.sheets.SelectMany(s=>s.products).ToArray();var catalog=CatalogLoader.Load();
  if(ids.Distinct().Count()!=ids.Length||!catalog.products.All(p=>ids.Contains(p.id)))throw new Exception("Commodity atlas coverage mismatch");
  foreach(var sheet in manifest.sheets){
   var atlas=new Texture2D(2,2,TextureFormat.RGBA32,false);if(!atlas.LoadImage(File.ReadAllBytes(Path.Combine(source,sheet.path))))throw new Exception("Cannot read "+sheet.path);
   var rgba=atlas.GetPixels32();if(!rgba.Any(c=>c.a==0))throw new Exception("Atlas is not transparent: "+sheet.name);
   for(int i=0;i<sheet.products.Length;i++){
    int col=i%sheet.columns,row=i/sheet.columns;
    int x=(int)Math.Round(col*atlas.width/(double)sheet.columns),right=(int)Math.Round((col+1)*atlas.width/(double)sheet.columns);
    int y=(int)Math.Round((sheet.rows-row-1)*atlas.height/(double)sheet.rows),top=(int)Math.Round((sheet.rows-row)*atlas.height/(double)sheet.rows);
    var pixels=atlas.GetPixels(x,y,right-x,top-y);int opaque=pixels.Count(c=>c.a>.05f),transparent=pixels.Count(c=>c.a<.01f);
    if(opaque<100||transparent<pixels.Length*.08f)throw new Exception("Invalid sprite transparency/content: "+sheet.products[i]);
    var icon=new Texture2D(right-x,top-y,TextureFormat.RGBA32,false);icon.SetPixels(pixels);icon.Apply();File.WriteAllBytes(destination+"/"+sheet.products[i]+".png",icon.EncodeToPNG());UnityEngine.Object.DestroyImmediate(icon);
   }
   UnityEngine.Object.DestroyImmediate(atlas);
  }
  AssetDatabase.Refresh();
  foreach(var id in ids){string path=destination+"/"+id+".png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=100;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=1024;importer.npotScale=TextureImporterNPOTScale.None;importer.SaveAndReimport();}
  Debug.Log("COMMODITY_SPRITES_PASS "+ids.Length+" individual transparent sprites");
 }
}
}
