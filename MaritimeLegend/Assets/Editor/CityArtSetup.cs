using System;using System.IO;using UnityEngine;using UnityEditor;
namespace MaritimeLegend.Editor {
public static class CityArtSetup {
 [MenuItem("Maritime Legend/City Art/Preview Lisbon")]
 public static void PreviewLisbon(){Preview(0);}
 [MenuItem("Maritime Legend/City Art/Preview Venice")]
 public static void PreviewVenice(){Preview(2);}
 static void Preview(int id){if(EditorApplication.isPlaying){var world=UnityEngine.Object.FindAnyObjectByType<WorldSession>();if(world!=null&&CityArtPreviewRuntime.Active){world.State.city=id;return;}Debug.LogWarning("Stop Play before opening an isolated city art preview.");return;}SessionState.SetInt("MaritimeLegend.CityArtPreview",id);EditorEntry.Play();}
 public static void Configure(){ConfigureTexture("Assets/Resources/Sprites/CityProps-v1.png");foreach(var key in new[]{"LisbonCity","VeniceCity"}){string name=key=="LisbonCity"?"Lisbon":"Venice";string folder="Assets/Cities/"+key+"/Art/";string atlas=folder+name+"Buildings-v2.png";ConfigureTexture(atlas);ConfigureTexture("Assets/Resources/Sprites/CityWalker-v2.png");var config=AssetDatabase.LoadAssetAtPath<CityDefinition>("Assets/Cities/"+key+"/"+key+".asset");if(config==null)throw new Exception("City missing "+key);config.buildingAtlas=AssetDatabase.LoadAssetAtPath<Texture2D>(atlas);config.buildingRects=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Data/Presentation/Atlases/"+name+"Buildings-v2.json");config.walkerAtlas=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Resources/Sprites/CityWalker-v2.png");config.walkerRects=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/Data/Presentation/Atlases/CityWalker-v2.json");config.spriteShader=Shader.Find("Maritime/CitySprite");config.depthShader=Shader.Find("Maritime/CityDepthLens");config.waterShader=Shader.Find("Maritime/CityWater");config.surfaceShader=Shader.Find("Maritime/CitySurface");config.atmosphereShader=Shader.Find("Maritime/CityAtmosphere");config.refined=true;if(config.buildingAtlas==null||config.buildingRects==null||config.spriteShader==null||config.depthShader==null||config.waterShader==null)throw new Exception("City art reference missing");EditorUtility.SetDirty(config);}AssetDatabase.SaveAssets();Debug.Log("CITY_ART_CONFIGURED");}
 static void ConfigureTexture(string path){var importer=AssetImporter.GetAtPath(path) as TextureImporter;if(importer==null)throw new Exception("Texture missing: "+path);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.npotScale=TextureImporterNPOTScale.None;importer.filterMode=FilterMode.Point;importer.wrapMode=TextureWrapMode.Clamp;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=2048;importer.SaveAndReimport();}
 public static void ConfigureDaylight(){
 ConfigureTexture("Assets/Resources/CityDaylight/Walker.png");
 foreach(var file in Directory.GetFiles("Assets/Resources/TownUI","*.png")){
  string path=file.Replace('\\','/');ConfigureTexture(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
 }
 foreach(var guid in AssetDatabase.FindAssets("t:CityDefinition")){
  var config=AssetDatabase.LoadAssetAtPath<CityDefinition>(AssetDatabase.GUIDToAssetPath(guid));
  config.spriteShader=Shader.Find("Maritime/CitySprite");config.depthShader=Shader.Find("Maritime/CityDepthLens");config.waterShader=Shader.Find("Maritime/CityWater");config.surfaceShader=Shader.Find("Maritime/CitySurface");config.atmosphereShader=Shader.Find("Maritime/CityAtmosphere");config.refined=true;EditorUtility.SetDirty(config);
 }
 AssetDatabase.SaveAssets();Debug.Log("CITY_DAYLIGHT_CONFIGURED");
 }
 public static void Build(){ConfigureDaylight();BuildGame.Build();}
 // An art-only executable keeps unrelated campaign regression failures visible without blocking visual QA.
 public static void BuildPreview(){
 ConfigureDaylight();SceneStructure.Validate();
 UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline=null;QualitySettings.renderPipeline=null;
 PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
 Directory.CreateDirectory("Builds/CityPreview");
 var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=System.Array.ConvertAll(EditorBuildSettings.scenes,x=>x.path),locationPathName="Builds/CityPreview/MaritimeLegend.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});
 if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("City preview build failed");
 Debug.Log("CITY_DAYLIGHT_PREVIEW_BUILD_PASS");
 }
}
}
