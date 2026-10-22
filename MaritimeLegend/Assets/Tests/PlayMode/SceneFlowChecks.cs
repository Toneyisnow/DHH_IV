using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Tests {
public sealed class SceneFlowChecks : MonoBehaviour {
 string dir;MaritimeGame game;WorldSession world;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void StartChecks(){if(Environment.GetCommandLineArgs().Contains("-sceneStructureSmoke")){var go=new GameObject("SceneFlowChecks");DontDestroyOnLoad(go);go.AddComponent<SceneFlowChecks>();}}
 void Start(){dir=Path.GetFullPath("SceneEvidence");Directory.CreateDirectory(dir);StartCoroutine(Guard(Run()));}
 IEnumerator Guard(IEnumerator run){while(true){object item;try{if(!run.MoveNext())break;item=run.Current;}catch(Exception e){Debug.LogException(e);File.WriteAllText(Path.Combine(dir,"FAILED.txt"),e.ToString());Application.Quit(1);yield break;}yield return item;}Debug.Log("SCENE_WORKFLOW_PLAYMODE_PASS");File.WriteAllText(Path.Combine(dir,"PASS.txt"),"Eight real scenes; two character steps; eight city prefabs; persistent workflow/production/fleet; save reload and offline restore; single audio/input/UI host.\n");Application.Quit(0);}
 void Check(bool ok,string label){if(!ok)throw new Exception("SCENE TEST FAILED: "+label);Debug.Log("SCENE CHECK PASS "+label);}
 void Capture(string name){LiveGui.Capture(Path.Combine(dir,name+".png"));}
 IEnumerator Run(){
 yield return new WaitForSecondsRealtime(2);game=FindAnyObjectByType<MaritimeGame>();world=FindAnyObjectByType<WorldSession>();Check(game!=null&&world!=null,"bootstrap");Check(SceneManager.GetActiveScene().name=="MainMenuScene","main menu scene");Capture("01-MainMenuScene");Check(LiveGui.ClickText("Press any key"),"title button");yield return new WaitForSecondsRealtime(.3f);Check(LiveGui.ClickText("开始游戏"),"new game button");yield return new WaitForSecondsRealtime(1);Check(SceneManager.GetActiveScene().name=="SaveLoadScene","save scene");Capture("02-SaveLoadScene");Check(LiveGui.ClickText("新游戏"),"empty slot");yield return new WaitForSecondsRealtime(1);Check(SceneManager.GetActiveScene().name=="CharacterCreationScene","character scene");Check(LiveGui.ClickText("欧洲 · 女"),"template");yield return new WaitForSecondsRealtime(.2f);Check(LiveGui.ClickText("下一步"),"customization");yield return new WaitForSecondsRealtime(.5f);Capture("03-CharacterCreationScene");Check(LiveGui.ClickText("以此家园"),"home selection");yield return new WaitForSecondsRealtime(1);Check(SceneManager.GetActiveScene().name=="CityScene","city scene");Check(world.State.gold==99999999,"requested initial gold");world.State.pendingEvents.Clear();
 var state=world.State;var service=world.Service;var workflow=world.Workflow;var catalog=world.Catalog;state.cities[0].factories[3]=1;var recipe=catalog.recipes.First(x=>x.output==catalog.Named("木板").id);foreach(var input in recipe.inputs)service.Inventory.Add(state.cities[0].warehouse,input.product,3,input.quantity*2,0);service.Start(recipe.id,0);var order=state.orders.Last();int quality=order.quality;string orderId=order.id;double progress=order.progress;
 state.routes.Add("0>1");service.BuyShip(1);service.AddAuto(1,catalog.Named("原木").id,3,999,1);Check(state.autoRoutes.Count==1,"automatic workflow task created");world.State.pendingEvents.Clear();
 foreach(string page in new[]{"城市","产业园","海图","自动航线","航线配置","标题","存档","航海","战斗","城市"}){
 if(page=="航海"){service.Depart(1);}if(page=="战斗"){state.mode="经典";game.StartBattle(false);}else game.Change(page);
 yield return new WaitForSecondsRealtime(.7f);Check(SceneManager.GetActiveScene().name==SceneNavigator.ForPage(page),"real scene for "+page);Check(ReferenceEquals(workflow,world.Workflow)&&ReferenceEquals(state,world.State),"workflow identity preserved "+page);Check(FindObjectsByType<GameAudio>(FindObjectsSortMode.None).Length==1,"single audio host");Check(FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"single event system");Check(FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length==1,"single canvas");state.pendingEvents.Clear();yield return new WaitForSecondsRealtime(.15f);Capture(SceneNavigator.ForPage(page)+"-"+page);
 }
 service.Rescue();state.pendingEvents.Clear();game.Change("城市");yield return new WaitForSecondsRealtime(.5f);
 for(int cityId=0;cityId<8;cityId++){state.city=cityId;yield return new WaitForSecondsRealtime(.15f);var city=FindAnyObjectByType<CityScene>();Check(city.Current.cityId==cityId&&city.ContentRoot.name==catalog.ports[cityId].cityKey,"dynamic city content "+cityId);Check(city.Interactions.Length==6,"unified interactions "+cityId);}
 Check(order.progress>progress&&order.quality==quality&&order.id==orderId,"order progress and seeded quality survive scene unload");Check(state.autoRoutes[0].progress>0,"automatic fleet continues across scenes");Check(workflow.Active&&workflow.ExecutedSteps>30,"workflow continuously ticks");
 state.screen="城市";SaveStore.Save(state);var restored=SaveStore.Load(state.slot);Check(restored.orders.Last().id==orderId&&restored.orders.Last().quality==quality,"save survives scene changes");restored.lastUtc=DateTime.UtcNow.AddHours(-3).ToString("O");world.State=restored;world.Service=new GameService(catalog,restored);world.Workflow.RestoreOffline(DateTime.UtcNow);Check(!workflow.Active,"old workflow stopped only on session replacement");Check(restored.orders.Last().completed,"offline workflow recovery");world.Service.Claim(orderId);int produced=restored.produced;world.Service.Claim(orderId);Check(restored.produced==produced,"repeated claim idempotence");
 game.Change("城市");yield return new WaitForSecondsRealtime(.5f);Capture("09-RestoredCity");
 }
}
}
