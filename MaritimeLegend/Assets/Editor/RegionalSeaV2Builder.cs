using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace MaritimeLegend.Editor {
// Reuses the approved SEA mesh, placement, lighting and water authoring pipeline.
public static class RegionalSeaV2Builder {
 public static readonly string[] Regions={"north_sea","mediterranean_sea","africa","indian_ocean","southeast_asia","east_asia","new_world"};
 [MenuItem("Maritime Legend/Manual Sailing V2/Build regional map assets")]
 public static void Generate(){
  var baseline=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Environment/SoutheastAsiaV2/Runtime.prefab");
  if(!baseline)throw new InvalidOperationException("Approved SEA runtime prefab missing");
  var report=new System.Text.StringBuilder();
  foreach(var id in Regions){
   var root=UnityEngine.Object.Instantiate(baseline);root.name="Manual sailing - "+id;
   try {
    var env=root.GetComponentInChildren<ManualSeaV2Environment>(true);
    report.AppendLine(id+": "+SoutheastAsiaV2Study.BuildTerrain(env,"RegionalSeasV2/"+id));
    env.mapTitle=id;env.enabled=false;
    foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=28;
    var folder="Assets/Resources/Environment/RegionalSeasV2/"+id;Directory.CreateDirectory(folder);AssetDatabase.Refresh();
    PrefabUtility.SaveAsPrefabAsset(root,folder+"/Runtime.prefab");
   }finally{UnityEngine.Object.DestroyImmediate(root);}
  }
  AssetDatabase.SaveAssets();
  Directory.CreateDirectory("RegionalReview");File.WriteAllText("RegionalReview/geometry.txt",report.ToString());
  foreach(var name in new[]{"Maritime/ManualSeaV2/Water","Maritime/ManualSeaV2/Land","Maritime/ManualSeaV2/Sky"})
   if(!Shader.Find(name)||ShaderUtil.ShaderHasError(Shader.Find(name)))throw new Exception("Shader failed: "+name);
  Debug.Log("REGIONAL_MAP_ASSETS_PASS");
 }
 public static void BuildReview(){
  Generate();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  new GameObject("Regional map art review").AddComponent<RegionalSeaV2Review>();
  Directory.CreateDirectory("Assets/Scenes");const string scene="Assets/Scenes/RegionalSeaV2Review.unity";
  EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),scene);
  BuildSavedReview();
 }
 public static void BuildSavedReview(){
  const string scene="Assets/Scenes/RegionalSeaV2Review.unity";
  PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
  Directory.CreateDirectory("Build");
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName="Build/RegionalSeaReview.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
  if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Regional review player build failed");
 }
}
}
