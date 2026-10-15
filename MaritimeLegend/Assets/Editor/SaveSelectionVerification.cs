using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace MaritimeLegend.Editor {
 public static class SaveSelectionVerification {
  static RenderTexture originalTarget,aspectTarget;static int stage;static double started,next;static bool previousEnabled;static EnterPlayModeOptions previousOptions;
  public static void Run(){
   if(!Application.isBatchMode)throw new Exception("Use an isolated batch project.");
   Directory.CreateDirectory("Evidence/SaveSelection");
   EditorSceneManager.OpenScene("Assets/Scenes/MainMenuScene.unity");
   previousEnabled=EditorSettings.enterPlayModeOptionsEnabled;previousOptions=EditorSettings.enterPlayModeOptions;
   EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
   started=EditorApplication.timeSinceStartup;next=started+3;stage=0;
   EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
  }
  static void Tick(){
   if(EditorApplication.timeSinceStartup-started>110){Finish(new Exception("Selection verification timeout"));return;}
   if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.6;
   try{
    var game=UnityEngine.Object.FindAnyObjectByType<MaritimeGame>();var art=UnityEngine.Object.FindAnyObjectByType<SaveSelectionPresentation>();
    if(stage==0){SaveStore.TestDirectory=Path.GetFullPath("Evidence/SaveSelection/TestSaves");typeof(MaritimeGame).GetMethod("OpenSlots",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(game,new object[]{"new"});}
    else if(stage==1){if(art==null)throw new Exception("Missing selection presentation");art.PreviewHover=-1;}
    else if(stage==2){
     LiveGui.Capture("Evidence/SaveSelection/rest.png");
     for(int i=0;i<7;i++){var q=art.Corners(i);var center=(q[0]+q[1]+q[2]+q[3])/4;if(art.Hit(center)!=i)throw new Exception("Card hit failed: "+i);}
     if(art.Hit(new Vector2(10,500))!=-1)throw new Exception("Outside hit failed");art.PreviewHover=0;
    }
    else if(stage==3){if(art.HoverAmount(0)<.99f)throw new Exception("Hover did not rise");LiveGui.Capture("Evidence/SaveSelection/hover.png");art.Select(0);art.PreviewHover=-1;}
    else if(stage==4){if(art.HoverAmount(0)>.01f||art.Selected!=0)throw new Exception("Hover exit or selected persistence failed");LiveGui.Capture("Evidence/SaveSelection/selected.png");originalTarget=LiveGui.Camera.targetTexture;aspectTarget=new RenderTexture(1280,1024,24);aspectTarget.Create();LiveGui.Camera.targetTexture=aspectTarget;}
    else if(stage==5){LiveGui.Capture("Evidence/SaveSelection/aspect-5x4.png");LiveGui.Camera.targetTexture=originalTarget;aspectTarget.Release();UnityEngine.Object.Destroy(aspectTarget);}
    else if(stage==6){if(!LiveGui.ClickText("建立档案"))throw new Exception("Missing create action");}
    else if(stage==8){if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name!="CharacterCreationScene")throw new Exception("New slot navigation failed");game.Change("标题");}
    else if(stage==10){if(art!=null)throw new Exception("Scene resources leaked");File.WriteAllText("Evidence/SaveSelection/verification.txt","PASS: seven card hits, outside rejection, hover enter/exit, persistent selection, new slot navigation, scene cleanup.\n");Finish(null);return;}
    stage++;
   }catch(Exception e){Finish(e);}
  }
  static void Finish(Exception error){EditorApplication.update-=Tick;EditorSettings.enterPlayModeOptionsEnabled=previousEnabled;EditorSettings.enterPlayModeOptions=previousOptions;if(error!=null)Debug.LogException(error);else Debug.Log("SAVE_SELECTION_VERIFICATION_PASS");EditorApplication.Exit(error==null?0:1);}
 }
}
