using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace MaritimeLegend.Editor {
public static class ManualSeaBuild {
 public static void Build(){
  foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Sprites/ManualSailing"})){var path=AssetDatabase.GUIDToAssetPath(guid);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=path.EndsWith("southeast_asia.png")?4096:1024;importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
  Directory.CreateDirectory("Builds/ManualSailing");
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Array.ConvertAll(SceneNavigator.Names,n=>"Assets/Scenes/"+n+".unity"),locationPathName="Builds/ManualSailing/MaritimeLegend.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Manual sea build failed");Debug.Log("MANUAL_SEA_BUILD_PASS");
 }
}
}
