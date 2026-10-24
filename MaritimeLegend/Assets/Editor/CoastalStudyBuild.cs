using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace MaritimeLegend.Editor {
public static class CoastalStudyBuild {
 [MenuItem("Maritime Legend/Build Coastal Study")]
 public static void Build(){
  const string models="Assets/Resources/Models/Environment/CoastalStudy";
  const string prefabs="Assets/Resources/Prefabs/Environment/CoastalStudy";Directory.CreateDirectory(prefabs);AssetDatabase.Refresh();
  var grass=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Textures/Environment/CoastalStudy/GrassPainted.png");
  grass.textureType=TextureImporterType.Default;grass.sRGBTexture=true;grass.mipmapEnabled=true;grass.filterMode=FilterMode.Trilinear;grass.anisoLevel=16;grass.wrapMode=TextureWrapMode.Repeat;grass.maxTextureSize=2048;grass.textureCompression=TextureImporterCompression.Uncompressed;grass.SaveAndReimport();
  foreach(string file in Directory.GetFiles(models,"*.fbx")){
   string path=file.Replace('\\','/');var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.globalScale=1;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;importer.isReadable=true;importer.meshCompression=ModelImporterMeshCompression.Off;importer.importNormals=ModelImporterNormals.Import;importer.SaveAndReimport();
   var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);var wrapper=new GameObject(Path.GetFileNameWithoutExtension(path));var instance=(GameObject)PrefabUtility.InstantiatePrefab(model);instance.transform.SetParent(wrapper.transform,false);
   // FBX axis conversion maps Blender XY to Unity -X/-Z. Restore authored map coordinates.
   instance.transform.localRotation=Quaternion.Euler(0,180,0);
   PrefabUtility.SaveAsPrefabAsset(wrapper,prefabs+"/"+wrapper.name+".prefab");UnityEngine.Object.DestroyImmediate(wrapper);
  }
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Coastal Study").AddComponent<CoastalStudyPreview>();
  EditorSceneManager.SaveScene(scene,"Assets/Scenes/CoastalStudy.unity");AssetDatabase.SaveAssets();
  UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;
  PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
  Directory.CreateDirectory("Builds/CoastalStudy");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/CoastalStudy.unity"},locationPathName="Builds/CoastalStudy/CoastalStudy.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Coastal study build failed");Debug.Log("COASTAL_STUDY_BUILD_PASS");
 }
}
}
