using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace MaritimeLegend.Editor {
public static class SailingV2Integration {
 [MenuItem("Maritime Legend/Manual Sailing V2/Build integrated V2 game")]
 public static void Build(){
  SceneStructure.Validate();DataConfigurationChecks.Run();ManualSailingChecks.Run();SailingArchitectureChecks.Run();
  foreach(string shader in new[]{"Maritime/ManualSeaV2/Sky","Maritime/ManualSeaV2/Clouds","Maritime/ManualSeaV2/Water","Maritime/ManualSeaV2/Exploration","Maritime/ManualSeaV2/Land","Maritime/ManualSeaV2/Ship"})
   if(!Shader.Find(shader)||ShaderUtil.ShaderHasError(Shader.Find(shader)))throw new System.Exception("Shader compile failed: "+shader);
  PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;
  UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;
  Directory.CreateDirectory("Builds/SailingV2Integrated");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=System.Array.ConvertAll(EditorBuildSettings.scenes,x=>x.path),locationPathName="Builds/SailingV2Integrated/MaritimeLegend.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("V2 integrated build failed");Debug.Log("V2_INTEGRATED_BUILD_PASS");
 }
 [MenuItem("Maritime Legend/Manual Sailing V2/Prepare integrated V2 scene")]
 public static void Prepare(){
  EditorSceneManager.OpenScene("Assets/Scenes/SoutheastAsiaSailingV2.unity");
  var env=Object.FindFirstObjectByType<ManualSeaV2Environment>();var camera=env.view;
  var wrapper=new GameObject("Southeast Asia V2 game environment");
  env.enabled=false;env.showControls=false;env.transform.SetParent(wrapper.transform,false);camera.transform.SetParent(wrapper.transform,false);
  camera.enabled=false;camera.cullingMask=1<<28;camera.tag="Untagged";
  var listener=camera.GetComponent<AudioListener>();if(listener)Object.DestroyImmediate(listener);
  camera.GetComponent<ManualSeaV2Camera>().enabled=false;env.sun.cullingMask=1<<28;
  var instances=env.GetComponent<ManualSeaV2Instances>();instances.enabled=false;instances.renderLayer=28;
  foreach(var ship in wrapper.GetComponentsInChildren<ManualSeaV2Ship>())ship.enabled=false;
  foreach(var item in wrapper.GetComponentsInChildren<Transform>())item.gameObject.layer=28;
  foreach(var item in wrapper.GetComponentsInChildren<ManualSeaV2Window>())Object.DestroyImmediate(item);
  foreach(var item in wrapper.GetComponentsInChildren<ManualSeaV2Benchmark>())Object.DestroyImmediate(item);
  wrapper.SetActive(false);Directory.CreateDirectory("Assets/Resources/Environment/SoutheastAsiaV2");
  PrefabUtility.SaveAsPrefabAsset(wrapper,"Assets/Resources/Environment/SoutheastAsiaV2/Runtime.prefab");
  AssetDatabase.SaveAssets();Debug.Log("V2_INTEGRATION_PREFAB_PASS");
 }
}
}
