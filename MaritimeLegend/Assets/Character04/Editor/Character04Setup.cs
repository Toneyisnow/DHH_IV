using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MaritimeLegend.CharacterArt;
namespace MaritimeLegend.Editor {
 public static class Character04Setup {
  const string Root="Assets/Character04",ScenePath=Root+"/Character04Preview.unity";
  static readonly string[] Names={"skin","skinLight","lip","dark","hair","hairLight","ivory","red","navy","teal","gold","leather","pants","white","iris"};
  static readonly Color[] Colors={new Color(.76f,.49f,.34f),new Color(.9f,.66f,.48f),new Color(.43f,.15f,.13f),new Color(.026f,.024f,.034f),new Color(.032f,.026f,.037f),new Color(.09f,.065f,.07f),new Color(.88f,.81f,.65f),new Color(.48f,.027f,.035f),new Color(.035f,.13f,.23f),new Color(.03f,.29f,.25f),new Color(.71f,.43f,.13f),new Color(.12f,.064f,.039f),new Color(.065f,.074f,.09f),new Color(.94f,.9f,.8f),new Color(.23f,.1f,.035f)};
  [MenuItem("Maritime Legend/Character 04/Open Test Scene")]
  public static void Open(){if(EditorApplication.isPlaying)return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;Create();EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);Selection.activeGameObject=UnityEngine.Object.FindAnyObjectByType<Character04Preview>().gameObject;}
  [MenuItem("Maritime Legend/Character 04/Play Test Scene")]
  public static void Play(){if(EditorApplication.isPlaying)return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;Create();EditorSceneManager.playModeStartScene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);EditorApplication.EnterPlaymode();}
  public static void Create(){
   Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();var shader=Shader.Find("Maritime/Character04Toon");if(shader==null)throw new Exception("Character shader missing");
   for(int i=0;i<Names.Length;i++){string path=Root+"/Materials/"+Names[i]+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(shader);AssetDatabase.CreateAsset(m,path);}m.shader=shader;m.color=Colors[i];EditorUtility.SetDirty(m);}
   string[] models={Root+"/Models/04_short_red_bob.fbx",Root+"/Models/04_long_navy_longhair.fbx"};
   foreach(var path in models){var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.animationType=ModelImporterAnimationType.Legacy;importer.importAnimation=true;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.SaveAndReimport();}
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   var holder=new GameObject("Character 04 - wardrobe controls");holder.transform.rotation=Quaternion.Euler(0,180,0);var preview=holder.AddComponent<Character04Preview>();
   preview.shortModel=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(models[0]));preview.longModel=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(models[1]));
   preview.shortModel.transform.SetParent(holder.transform,false);preview.longModel.transform.SetParent(holder.transform,false);
   preview.shortClips=Clips(models[0]);preview.longClips=Clips(models[1]);
   foreach(var model in new[]{preview.shortModel,preview.longModel}){
    var anim=model.GetComponent<Animation>();if(anim!=null)UnityEngine.Object.DestroyImmediate(anim);
    foreach(var r in model.GetComponentsInChildren<Renderer>(true)){
     var ms=r.sharedMaterials;for(int i=0;i<ms.Length;i++){string key=ms[i].name;var m=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/"+key+".mat");if(m==null)throw new Exception("No Unity material for "+key);ms[i]=m;}r.sharedMaterials=ms;
     if(r is SkinnedMeshRenderer sk)sk.updateWhenOffscreen=true;
     if(model==preview.longModel)r.enabled=false;
    }
   }
   preview.red=Mat("red");preview.navy=Mat("navy");preview.teal=Mat("teal");
   var cam=new GameObject("Character Camera").AddComponent<Camera>();cam.tag="MainCamera";cam.orthographic=true;cam.orthographicSize=2.05f;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.13f,.17f,.21f);cam.transform.position=new Vector3(.8f,2.1f,-8);cam.transform.LookAt(new Vector3(0,1.75f,0));cam.rect=new Rect(.27f,0,.73f,1);preview.previewCamera=cam;
   cam.gameObject.AddComponent<AudioListener>();
   var ground=GameObject.CreatePrimitive(PrimitiveType.Plane);ground.name="Studio floor";ground.transform.localScale=Vector3.one*2;ground.GetComponent<Renderer>().sharedMaterial=Mat("pants");
   EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();Debug.Log("CHARACTER04_SCENE_CREATED clips="+preview.shortClips.Length+"/"+preview.longClips.Length);
  }
  static Material Mat(string name)=>AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/"+name+".mat");
  static AnimationClip[] Clips(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__preview__")).ToArray();
  public static void BuildTest(){Create();Directory.CreateDirectory("Builds/Character04");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName="Builds/Character04/Character04Preview.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development});if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Character preview build failed");Debug.Log("CHARACTER04_BUILD_PASS");}
 }
}
