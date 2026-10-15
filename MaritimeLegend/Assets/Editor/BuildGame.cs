using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
namespace MaritimeLegend.Editor {
 public static class BuildGame {
  [MenuItem("Maritime Legend/Build Windows")]
  public static void Build(){
   PlayerSettings.companyName="MaritimeLegend";PlayerSettings.productName="航海世家";PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=true;UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;
   PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
   var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);var input=settings.FindProperty("activeInputHandler");if(input!=null){input.intValue=2;settings.ApplyModifiedProperties();}
   SceneStructure.Validate();
   AssetDatabase.SaveAssets();DataConfigurationChecks.Run();InitialStockChecks.Run();DomainChecks.Run();Directory.CreateDirectory("Builds/Windows");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=System.Array.ConvertAll(EditorBuildSettings.scenes,x=>x.path),locationPathName="Builds/Windows/MaritimeLegend.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);Directory.CreateDirectory("Builds/Windows/TestData");foreach(var fixture in Directory.GetFiles("Assets/Tests/Data","*.json"))File.Copy(fixture,Path.Combine("Builds/Windows/TestData",Path.GetFileName(fixture)),true);Debug.Log("MARITIME_BUILD_SUCCESS "+report.summary.totalSize);
  }
 }
}
