using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace MaritimeLegend.Editor {
 public static class OpeningVerification {
  static double started,next;
  static int stage;
  static bool oldOptionsEnabled;
  static EnterPlayModeOptions oldOptions;
  static readonly float[] times={.1f,.8f,1.7f,2.5f,3.6f,24f};
  public static void Run(){
   if(!Application.isBatchMode)throw new Exception("Run opening verification in an isolated batch-mode project copy.");
   Directory.CreateDirectory("Evidence/Opening");
   foreach(string name in new[]{"OpeningGlobe","OpeningAdditive"}){
    var shader=Resources.Load<Shader>("Opening/"+name);
    if(shader==null||ShaderUtil.ShaderHasError(shader))throw new Exception("Opening shader failed: "+name);
   }
   EditorSceneManager.OpenScene("Assets/Scenes/MainMenuScene.unity");
   oldOptionsEnabled=EditorSettings.enterPlayModeOptionsEnabled;oldOptions=EditorSettings.enterPlayModeOptions;
   EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
   started=EditorApplication.timeSinceStartup;next=started+3;stage=0;
   EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
  }
  static void Tick(){
   if(EditorApplication.timeSinceStartup-started>100){Fail(new Exception("Opening verification timed out"));return;}
   if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;
   next=EditorApplication.timeSinceStartup+.6;
   try{
    var art=UnityEngine.Object.FindAnyObjectByType<OpeningPresentation>();
    if(stage<times.Length*2){
     if(art==null||art.Logo==null||art.Background==null)throw new Exception("Missing opening resources");
     if(stage%2==0)art.previewTime=times[stage/2];
     else LiveGui.Capture("Evidence/Opening/frame-"+times[stage/2].ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+".png");
    }else if(stage==12){if(!LiveGui.ClickText("Press any key"))throw new Exception("Missing start control");next=EditorApplication.timeSinceStartup+.1;}
    else if(stage==13){if(LiveGui.ClickText("开始游戏"))throw new Exception("Start input guard expired too early");next=EditorApplication.timeSinceStartup+.85;}
    else if(stage==14){LiveGui.Capture("Evidence/Opening/menu.png");if(!LiveGui.ClickText("开始游戏"))throw new Exception("New game unavailable after transition");}
    else if(stage==16){
     if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="SaveLoadScene")throw new Exception("New game navigation failed");
     UnityEngine.Object.FindAnyObjectByType<MaritimeGame>().Change("标题");
    }else if(stage==18){
     if(art==null)throw new Exception("Opening not recreated on return");
     LiveGui.Capture("Evidence/Opening/return.png");
     File.WriteAllText("Evidence/Opening/verification.txt","PASS: shaders, six timeline frames, start input guard, menu, new-game navigation and return lifecycle.\n");
     Debug.Log("OPENING_VERIFICATION_PASS");Restore();EditorApplication.update-=Tick;EditorApplication.Exit(0);return;
    }
    stage++;
   }catch(Exception e){Fail(e);}
  }
  static void Restore(){EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled;EditorSettings.enterPlayModeOptions=oldOptions;}
  static void Fail(Exception e){Debug.LogException(e);Restore();EditorApplication.update-=Tick;EditorApplication.Exit(1);}
 }
}
