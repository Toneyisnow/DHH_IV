using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace MaritimeLegend.Editor {
[InitializeOnLoad]
public static class EditorEntry {
 const string ScenePath="Assets/Scenes/MainMenuScene.unity";
 const string TestKey="MaritimeLegend.EditorPlayVerification";
 static double started;
 static bool requested;
 static EditorEntry(){EditorApplication.delayCall+=Configure;EditorApplication.update+=VerifyUpdate;EditorApplication.playModeStateChanged+=Changed;}
 static void Configure(){var scene=AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);if(scene!=null)EditorSceneManager.playModeStartScene=scene;}
 [MenuItem("Maritime Legend/Open Game Scene")]
 public static void Open(){if(EditorApplication.isPlaying)return;if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene(ScenePath);Configure();}
 [MenuItem("Maritime Legend/Play Game %#p")]
 public static void Play(){Configure();if(!EditorApplication.isPlaying)EditorApplication.EnterPlaymode();}
 public static void Verify(){SessionState.SetInt(TestKey,1);EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;EditorSceneManager.OpenScene(ScenePath);Configure();started=EditorApplication.timeSinceStartup;EditorApplication.EnterPlaymode();}
 static void Changed(PlayModeStateChange state){if(SessionState.GetInt(TestKey,0)==0)return;if(state==PlayModeStateChange.EnteredPlayMode){started=EditorApplication.timeSinceStartup;requested=false;}if(state==PlayModeStateChange.EnteredEditMode){int pass=SessionState.GetInt(TestKey,0);if(pass==1){SessionState.SetInt(TestKey,2);EditorApplication.delayCall+=()=>EditorApplication.EnterPlaymode();}else{SessionState.SetInt(TestKey,0);Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/EditorPlayVerification.txt","PASS: Unity Editor Play Mode entered twice, with domain reload disabled; game bootstrap, Canvas, title and new-game buttons verified.\n"+DateTime.UtcNow.ToString("O"));Debug.Log("EDITOR_PLAY_VERIFIED_TWICE");EditorApplication.Exit(0);}}}
 static void VerifyUpdate(){if(SessionState.GetInt(TestKey,0)==0||!EditorApplication.isPlaying||requested)return;if(EditorApplication.timeSinceStartup-started<5)return;try{if(UnityEngine.Object.FindAnyObjectByType<MaritimeGame>()==null)throw new Exception("Game bootstrap missing");if(UnityEngine.Object.FindAnyObjectByType<Canvas>()==null)throw new Exception("Canvas missing");if(!LiveGui.ClickText("Press any key"))throw new Exception("Title button missing");EditorApplication.delayCall+=CheckMenu;requested=true;}catch(Exception e){Debug.LogException(e);SessionState.SetInt(TestKey,0);EditorApplication.Exit(1);}}
 static void CheckMenu(){EditorApplication.delayCall+=()=>{try{if(!LiveGui.ClickText("新游戏"))throw new Exception("New game button missing");Debug.Log("EDITOR_PLAY_PASS "+SessionState.GetInt(TestKey,0));EditorApplication.ExitPlaymode();}catch(Exception e){Debug.LogException(e);SessionState.SetInt(TestKey,0);EditorApplication.Exit(1);}};}
}
}
