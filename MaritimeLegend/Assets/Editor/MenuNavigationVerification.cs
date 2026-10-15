using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace MaritimeLegend.Editor {
 public static class MenuNavigationVerification {
  static int stage;static double started,next;
  static bool oldEnabled;static EnterPlayModeOptions oldOptions;
  static MaritimeGame game;static WorldSession world;static EventSystem events;static Camera camera;static Canvas canvas;
  static Button pressed;static PointerEventData pointer;
  static readonly BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
  public static void Run(){
   if(!Application.isBatchMode)throw new Exception("Use an isolated batch project.");
   Directory.CreateDirectory("Evidence/MenuNavigation");
   EditorSceneManager.OpenScene("Assets/Scenes/MainMenuScene.unity");
   oldEnabled=EditorSettings.enterPlayModeOptionsEnabled;oldOptions=EditorSettings.enterPlayModeOptions;
   EditorSettings.enterPlayModeOptionsEnabled=true;EditorSettings.enterPlayModeOptions=EnterPlayModeOptions.DisableDomainReload;
   stage=0;started=EditorApplication.timeSinceStartup;next=started+3;EditorApplication.update+=Tick;EditorApplication.EnterPlaymode();
  }
  static void Check(bool value,string message){if(!value)throw new Exception(message);}
  static void Press(string caption){
   pressed=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault(b=>b.gameObject.activeInHierarchy&&b.GetComponentInChildren<Text>().text==caption);
   Check(pressed!=null&&pressed.IsInteractable(),"Button unavailable: "+caption);
   Canvas.ForceUpdateCanvases();var rect=(RectTransform)pressed.transform;
   pointer=new PointerEventData(events){button=PointerEventData.InputButton.Left,position=RectTransformUtility.WorldToScreenPoint(camera,rect.TransformPoint(rect.rect.center))};
   var hits=new List<RaycastResult>();events.RaycastAll(pointer,hits);
   Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<Button>()==pressed,"Button raycast obstructed: "+caption);
   ExecuteEvents.Execute(pressed.gameObject,pointer,ExecuteEvents.pointerEnterHandler);
   ExecuteEvents.Execute(pressed.gameObject,pointer,ExecuteEvents.pointerDownHandler);
  }
  static void Release(){
   Check(pressed!=null&&pressed.IsInteractable(),"Button disabled between pointer down and up");
   ExecuteEvents.Execute(pressed.gameObject,pointer,ExecuteEvents.pointerUpHandler);
   ExecuteEvents.Execute(pressed.gameObject,pointer,ExecuteEvents.pointerClickHandler);
   pressed=null;
  }
  static void Scene(string name,string mode=null){
   Check(SceneManager.GetActiveScene().name==name,"Wrong scene: "+name);
   Check(UnityEngine.Object.FindAnyObjectByType<MaritimeGame>()==game&&UnityEngine.Object.FindAnyObjectByType<WorldSession>()==world,"Persistent state replaced");
   Check(EventSystem.current==events&&LiveGui.Camera==camera&&UnityEngine.Object.FindAnyObjectByType<Canvas>()==canvas,"Persistent input/UI replaced");
   Check(UnityEngine.Object.FindObjectsByType<FunctionalScene>(FindObjectsSortMode.None).Length==1,"Ambiguous scene controller");
   Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Duplicate event systems");
   if(mode!=null)Check((string)typeof(MaritimeGame).GetField("slotMode",flags).GetValue(game)==mode,"Lost slot mode");
  }
  static void Tick(){
   if(EditorApplication.timeSinceStartup-started>100){Finish(new Exception("Menu navigation timed out"));return;}
   if(!EditorApplication.isPlaying||EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.35;
   try{
    switch(stage){
     case 0:
      game=UnityEngine.Object.FindAnyObjectByType<MaritimeGame>();world=UnityEngine.Object.FindAnyObjectByType<WorldSession>();events=EventSystem.current;camera=LiveGui.Camera;canvas=UnityEngine.Object.FindAnyObjectByType<Canvas>();
      SaveStore.TestDirectory=Path.GetFullPath("Evidence/MenuNavigation/TestSaves/"+Guid.NewGuid().ToString("N"));Press("Press any key");break;
     case 1:Release();next=EditorApplication.timeSinceStartup+1;break;
     case 2:
      var gate=typeof(MaritimeGame).GetMethod("TitleInputReady",flags);var latch=typeof(MaritimeGame).GetField("titleInputReleased",flags);latch.SetValue(game,false);
      Check(!(bool)gate.Invoke(game,new object[]{1f,true}),"Opening held gesture must be blocked");
      Check(!(bool)gate.Invoke(game,new object[]{.5f,false}),"Opening transition must be blocked");
      Check((bool)gate.Invoke(game,new object[]{1f,false}),"Released opening did not enable menu");
      Check((bool)gate.Invoke(game,new object[]{1f,true}),"Subsequent mouse hold disabled menu");break;
     case 3:Press("新游戏");break;
     case 4:Release();break;
     case 5:Scene("SaveLoadScene","new");LiveGui.Capture("Evidence/MenuNavigation/new.png");Press("返回");break;
     case 6:Release();break;
     case 7:Scene("MainMenuScene");Press("读取航海日志");break;
     case 8:Release();break;
     case 9:Scene("SaveLoadScene","load");LiveGui.Capture("Evidence/MenuNavigation/load.png");Press("返回");break;
     case 10:Release();break;
     case 11:Scene("MainMenuScene");Press("新游戏");break;
     case 12:Release();break;
     case 13:Scene("SaveLoadScene","new");Press("建立档案");break;
     case 14:Release();break;
     case 15:Scene("CharacterCreationScene");Press("返回选档");break;
     case 16:Release();break;
     case 17:Scene("SaveLoadScene","new");Press("返回");break;
     case 18:Release();break;
     case 19:Scene("MainMenuScene");File.WriteAllText("Evidence/MenuNavigation/verification.txt","PASS: opening release latch; subsequent held pointer remains enabled; raycast + pointer down/up/click; new/load mode transfer; repeated main/save/character scene round trips; persistent game/world/camera/canvas/event system.\n");Finish(null);return;
    }
    stage++;
   }catch(Exception e){Finish(e);}
  }
  static void Finish(Exception error){EditorApplication.update-=Tick;EditorSettings.enterPlayModeOptionsEnabled=oldEnabled;EditorSettings.enterPlayModeOptions=oldOptions;if(error!=null)Debug.LogException(error);else Debug.Log("MENU_NAVIGATION_VERIFICATION_PASS");EditorApplication.Exit(error==null?0:1);}
 }
}
