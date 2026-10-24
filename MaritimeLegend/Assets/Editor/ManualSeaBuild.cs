using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
namespace MaritimeLegend.Editor {
public static class ManualSeaBuild {
 [MenuItem("Maritime Legend/Import Indian Ocean Landforms")]
 public static void ImportLandforms(){
  const string models="Assets/Resources/Models/Environment/IndianOcean";
  const string prefabs="Assets/Resources/Prefabs/Environment/IndianOcean";
  Directory.CreateDirectory(prefabs);AssetDatabase.Refresh();
  foreach(string file in Directory.GetFiles(models,"*.fbx")){
   string path=file.Replace('\\','/');var importer=(ModelImporter)AssetImporter.GetAtPath(path);
   importer.globalScale=1;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.isReadable=true;
   importer.meshCompression=ModelImporterMeshCompression.Off;importer.importNormals=ModelImporterNormals.Import;importer.SaveAndReimport();
   var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);var wrapper=new GameObject(Path.GetFileNameWithoutExtension(path));
   var child=(GameObject)PrefabUtility.InstantiatePrefab(model);child.transform.SetParent(wrapper.transform,false);child.transform.localRotation=Quaternion.Euler(0,180,0);
   PrefabUtility.SaveAsPrefabAsset(wrapper,prefabs+"/"+wrapper.name+".prefab");UnityEngine.Object.DestroyImmediate(wrapper);
  }
  AssetDatabase.SaveAssets();
 }
 public static void Build(){
  ImportLandforms();
  foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Sprites/UI/ManualSailing/Instruments"})){
   var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;
   importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.maxTextureSize=512;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
  }
  foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Textures/ManualSailing/Tiles"})){
   var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
   importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;importer.maxTextureSize=2048;importer.npotScale=TextureImporterNPOTScale.None;
   importer.mipmapEnabled=true;importer.isReadable=false;importer.alphaIsTransparency=false;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=8;importer.wrapMode=TextureWrapMode.Clamp;
   importer.textureCompression=TextureImporterCompression.Uncompressed;
   var platform=importer.GetDefaultPlatformTextureSettings();platform.maxTextureSize=2048;platform.format=TextureImporterFormat.RGBA32;importer.SetPlatformTextureSettings(platform);
   importer.ClearPlatformTextureSettings("Standalone");importer.SaveAndReimport();
  }
  foreach(string guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Sprites/ManualSailing"})){var path=AssetDatabase.GUIDToAssetPath(guid);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.maxTextureSize=path.EndsWith("southeast_asia.png")?4096:1024;importer.npotScale=TextureImporterNPOTScale.None;importer.anisoLevel=8;importer.filterMode=FilterMode.Trilinear;importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
  UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;
  PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
  Directory.CreateDirectory("Builds/ManualSailing");
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Array.ConvertAll(SceneNavigator.Names,n=>"Assets/Scenes/"+n+".unity"),locationPathName="Builds/ManualSailing/MaritimeLegend.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(result.summary.result!=BuildResult.Succeeded)throw new Exception("Manual sea build failed");Debug.Log("MANUAL_SEA_BUILD_PASS");
 }
}
}
