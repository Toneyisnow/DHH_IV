using System;
using System.IO;
using System.Linq;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MaritimeLegend.Domain;
namespace MaritimeLegend.Tests {
public sealed class HarborFlowChecks : MonoBehaviour {
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-harborSmoke")){var go=new GameObject("Harbor flow checks");DontDestroyOnLoad(go);go.AddComponent<HarborFlowChecks>();}}
 void Start(){StartCoroutine(Guard(Run()));}
 IEnumerator Guard(IEnumerator run){while(true){object next;try{if(!run.MoveNext())break;next=run.Current;}catch(Exception e){Debug.LogException(e);Application.Quit(1);yield break;}yield return next;}Debug.Log("HARBOR_FLOW_PASS: port destination, return, automatic route, supply rejection, manual sailing scene, preserved state");Application.Quit(0);}
 void Check(bool ok,string why){if(!ok)throw new Exception("Harbor flow: "+why);}
 IEnumerator Run(){
  yield return null;var game=FindAnyObjectByType<MaritimeGame>();var world=FindAnyObjectByType<WorldSession>();
  string root=Path.GetFullPath(Path.Combine(Application.dataPath,"..","HarborTestData",Guid.NewGuid().ToString("N")));Directory.CreateDirectory(root);SaveStore.TestDirectory=root;UserDataPaths.TestRoot=root;
  game.s=GameService.New(game.c,0,0,"休闲",StateFactory.Appearance(game.c),"港口流程验证",42);game.s.pendingEvents.Clear();game.g=new GameService(game.c,game.s);var state=game.s;game.Change("城市");
  LiveGui.Camera.targetTexture=new RenderTexture(1920,1080,24);LiveGui.Camera.targetTexture.Create();
  yield return new WaitForSecondsRealtime(1);
  var city=FindAnyObjectByType<CityScene>();var harbor=city.Interactions.First(x=>x.Label=="码头");Check(harbor.Destination=="港口","legacy city port destination");
  Check(LiveGui.ClickText("港口",true),"city shortcut");yield return new WaitForSecondsRealtime(.4f);Check(game.page=="港口"&&!state.atSea,"port choice before departure");
  Directory.CreateDirectory(Path.Combine(root,"Evidence"));LiveGui.Capture(Path.Combine(root,"Evidence","harbor.png"));
  Check(LiveGui.ClickText("返回城市",true),"return button");yield return new WaitForSecondsRealtime(.3f);Check(game.page=="城市","return leaves player ashore");
  game.Change(harbor.Destination);yield return new WaitForSecondsRealtime(.3f);Check(LiveGui.ClickText("自动航行",true),"automatic option");yield return new WaitForSecondsRealtime(.5f);Check(SceneManager.GetActiveScene().name=="AutoSailingScene"&&!state.atSea,"automatic route scene");
  game.Change("港口");yield return new WaitForSecondsRealtime(.5f);double food=state.ships[0].food;state.ships[0].food=0;Check(LiveGui.ClickText("手动航行",true),"manual option");yield return new WaitForSecondsRealtime(.3f);Check(game.page=="港口"&&!state.atSea,"failed departure stays at harbor");
  state.ships[0].food=food;Check(LiveGui.ClickText("下一港",true),"destination selection");yield return new WaitForSecondsRealtime(.3f);int destination=game.selectedPort;Check(destination!=state.city,"valid destination");
  Check(LiveGui.ClickText("手动航行",true),"manual departure");float deadline=Time.realtimeSinceStartup+20;while((SceneManager.GetActiveScene().name!="SailingScene"||state.sailing.manualMapId!="indian_ocean")&&Time.realtimeSinceStartup<deadline)yield return null;Check(SceneManager.GetActiveScene().name=="SailingScene"&&game.page=="航海"&&state.atSea,"manual sea scene");Check(ReferenceEquals(game.s,state)&&state.seaTo==destination,"same save and selected port");Check(state.sailing.manualMapId=="indian_ocean"&&!string.IsNullOrEmpty(state.sailing.manualSpawnPort),"Indian regional departure");Check(game.SetManualSeaTarget(new Vector2(1350,680)),"new sea accepts target");
 }
}
}
