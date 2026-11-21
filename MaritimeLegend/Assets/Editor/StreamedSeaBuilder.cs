using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace MaritimeLegend.Editor {
public static class StreamedSeaBuilder {
 [MenuItem("Maritime Legend/Manual Sailing V2/Build streamed world")]
 public static void Build(){
  var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Environment/SoutheastAsiaV2/Runtime.prefab");
  var root=UnityEngine.Object.Instantiate(source);root.name="Original SEA presentation - streamed world shell";
  try{
   var env=root.GetComponentInChildren<ManualSeaV2Environment>(true);var instances=root.GetComponentInChildren<ManualSeaV2Instances>(true);
   foreach(var lod in root.GetComponentsInChildren<LODGroup>(true))UnityEngine.Object.DestroyImmediate(lod.gameObject);
   instances.cells=Array.Empty<ManualSeaV2Instances.Cell>();
   // Remove the regional water mesh; the same water material draws streamed depth tiles.
   foreach(var mesh in root.GetComponentsInChildren<MeshRenderer>(true))if(mesh.sharedMaterial==env.water)UnityEngine.Object.DestroyImmediate(mesh.gameObject);
   var folder="Assets/Resources/Environment/StreamedSeasV2";Directory.CreateDirectory(folder);AssetDatabase.Refresh();
   var water=new Material(env.water);water.SetTexture("_DepthMap",Texture2D.blackTexture);AssetDatabase.CreateAsset(water,folder+"/Water.mat");env.water=water;
   env.mapTitle="World manual sailing";PrefabUtility.SaveAsPrefabAsset(root,folder+"/Runtime.prefab");AssetDatabase.SaveAssets();
  }finally{UnityEngine.Object.DestroyImmediate(root);}
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Streamed world review").AddComponent<StreamedSeaReview>();
  Directory.CreateDirectory("Assets/Scenes");EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/StreamedWorldSailing.unity");BuildSaved();
 }
 public static void BuildSaved(){
  PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;Directory.CreateDirectory("Build");
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/StreamedWorldSailing.unity"},locationPathName="Build/StreamedWorldSailing.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Streamed world build failed");
 }
}
}
