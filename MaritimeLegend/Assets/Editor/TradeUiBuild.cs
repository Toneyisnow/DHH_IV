using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace MaritimeLegend.Editor {
public static class TradeUiBuild {
 public static void Build(){
  CommoditySpritePublisher.Publish();TradeSessionChecks.Run();
  foreach(string path in new[]{"Assets/Resources/Trading/goods-atlas.png","Assets/Resources/Trading/harbor-background.png"}){var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer!=null){importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.filterMode=path.Contains("atlas")?FilterMode.Point:FilterMode.Bilinear;importer.SaveAndReimport();}}
  Directory.CreateDirectory("Builds/Windows");PlayerSettings.companyName="MaritimeLegend";PlayerSettings.productName="航海世家";PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Array.ConvertAll(EditorBuildSettings.scenes,x=>x.path),locationPathName="Builds/Windows/MaritimeLegend.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Trade UI build failed: "+result.summary.result);Debug.Log("TRADE_UI_BUILD_PASS");
 }
}
}
