using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Editor {
public static class SceneStructure {
 [MenuItem("Maritime Legend/Rebuild eight scene assets")]
 public static void Generate(){
 var catalog=CatalogLoader.Load();var definitions=new CityDefinition[8];
 for(int i=0;i<8;i++){var port=catalog.ports[i];string folder="Assets/Cities/"+port.cityKey;Directory.CreateDirectory(folder+"/Art");Directory.CreateDirectory(folder+"/Buildings");AssetDatabase.Refresh();
 string source="Assets/Resources/Sprites/Ports/port"+i+".png",art=folder+"/Art/"+port.cityKey+".png";
 if(AssetDatabase.LoadAssetAtPath<Texture2D>(art)==null&&File.Exists(source)){string error=AssetDatabase.MoveAsset(source,art);if(error!="")throw new Exception(error);}
 var root=new GameObject(port.cityKey);
 string[] objects={"MarketStall","Shipyard","IndustryEntrance","CityHall","HarborTavern","Harbor"};string[] labels={"市场","船坞","产业园","市政厅","伙伴","码头"};string[] pages={"市场","船坞","产业园","市政厅","伙伴","海图"};Vector2[] positions={new Vector2(210,560),new Vector2(1080,530),new Vector2(550,430),new Vector2(820,360),new Vector2(380,720),new Vector2(1190,730)};
 for(int j=0;j<objects.Length;j++){var point=new GameObject(objects[j]);point.transform.SetParent(root.transform);var interaction=point.AddComponent<CityInteraction>();interaction.label=labels[j];interaction.destination=pages[j];interaction.screenPosition=positions[j];}
 var prefab=PrefabUtility.SaveAsPrefabAsset(root,folder+"/"+port.cityKey+".prefab");UnityEngine.Object.DestroyImmediate(root);
 string configPath=folder+"/"+port.cityKey+".asset";var config=AssetDatabase.LoadAssetAtPath<CityDefinition>(configPath);if(config==null){config=ScriptableObject.CreateInstance<CityDefinition>();AssetDatabase.CreateAsset(config,configPath);}config.cityId=port.id;config.cityKey=port.cityKey;config.displayName=port.name;config.background=AssetDatabase.LoadAssetAtPath<Texture2D>(art);config.contentRoot=prefab;config.eventIds=catalog.events.Select(x=>x.id).ToArray();config.weather=i<5?"近海微风":"季风港湾";EditorUtility.SetDirty(config);definitions[i]=config;
 }
 Directory.CreateDirectory("Assets/Scenes");
 foreach(string name in SceneNavigator.Names){var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var go=new GameObject(name);var type=typeof(MaritimeGame).Assembly.GetType("MaritimeLegend."+name,true);var controller=go.AddComponent(type);if(controller is CityScene city)city.cities=definitions;EditorSceneManager.SaveScene(scene,"Assets/Scenes/"+name+".unity");}
 EditorBuildSettings.scenes=SceneNavigator.Names.Select(x=>new EditorBuildSettingsScene("Assets/Scenes/"+x+".unity",true)).ToArray();AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/Scenes/MainMenuScene.unity");Debug.Log("EIGHT_SCENES_GENERATED");
 }
 public static void Validate(){var paths=Directory.GetFiles("Assets","*.unity",SearchOption.AllDirectories);if(paths.Length!=8)throw new Exception("Expected exactly eight scenes: "+paths.Length);foreach(string name in SceneNavigator.Names)if(!File.Exists("Assets/Scenes/"+name+".unity"))throw new Exception("Missing scene "+name);var builds=EditorBuildSettings.scenes;if(builds.Length!=8||builds.Any(x=>!x.enabled))throw new Exception("Build list must contain eight enabled scenes");var c=CatalogLoader.Load();foreach(var p in c.ports){if(!p.cityKey.EndsWith("City"))throw new Exception("City identifier suffix");var def=AssetDatabase.LoadAssetAtPath<CityDefinition>("Assets/Cities/"+p.cityKey+"/"+p.cityKey+".asset");if(def==null||def.cityId!=p.id||def.contentRoot==null||def.background==null)throw new Exception("City content missing "+p.cityKey);if(def.contentRoot.GetComponentsInChildren<CityInteraction>(true).Length!=6)throw new Exception("City interactions missing");}Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/SceneStructureChecks.txt","PASS: exactly eight scenes, eight build entries, eight stable cityIds, eight city configurations and root prefabs, 48 unified city interactions.\n");Debug.Log("SCENE_STRUCTURE_VALIDATED");}
 public static void ValidateAndBuild(){GenerateAndCheck();BuildGame.Build();}
 public static void GenerateAndCheck(){Generate();Validate();DomainChecks.Run();MaritimeLegend.Tests.WorkflowSimulation.Run(CatalogLoader.Load());Debug.Log("WORKFLOW_SIMULATION_PASS_30_DAYS");}
}
}
