using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
public sealed class TradeUiSmoke : MonoBehaviour {
 static void CapturePanel(string path,PortTradePanel panel){
  var canvas=panel.GetComponentsInChildren<Canvas>().Single(x=>x.name=="PortTradeCanvas");var go=new GameObject("TradeCaptureCamera");var camera=go.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=540;camera.nearClipPlane=.1f;camera.farClipPlane=2000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
  var nodes=canvas.GetComponentsInChildren<Transform>(true);var layers=nodes.Select(t=>t.gameObject.layer).ToArray();foreach(var node in nodes)node.gameObject.layer=31;camera.cullingMask=1<<31;
  var target=new RenderTexture(1920,1080,24);target.Create();camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=100;Canvas.ForceUpdateCanvases();camera.Render();for(int i=0;i<nodes.Length;i++)nodes[i].gameObject.layer=layers[i];
  var previous=RenderTexture.active;RenderTexture.active=target;var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(Path.GetFullPath(path),image.EncodeToPNG());RenderTexture.active=previous;canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;camera.targetTexture=null;target.Release();Destroy(target);Destroy(image);Destroy(go);
 }
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot(){if(Environment.GetCommandLineArgs().Contains("-tradeUISmoke"))new GameObject("TradeUiSmoke").AddComponent<TradeUiSmoke>();}
 IEnumerator Start(){DontDestroyOnLoad(gameObject);Application.runInBackground=true;yield return null;var owner=FindAnyObjectByType<MaritimeGame>();var args=Environment.GetCommandLineArgs();int input=Array.IndexOf(args,"-tradeSaveDirectory");if(input>=0){SaveStore.TestDirectory=args[input+1];owner.s=SaveStore.Load(2,true);}else owner.s=GameService.New(owner.c,0,0,"休闲",new Appearance(),"Trade smoke",123);
  // Always work on a cloned test save, never the player's live files.
  owner.s=CatalogLoader.Copy(owner.s);SaveStore.TestDirectory=Path.GetFullPath("Evidence/TradeUI/TestSaves");owner.s.pendingEvents.Clear();owner.s.atSea=false;owner.g=new GameService(owner.c,owner.s);owner.page="市场";
  yield return new WaitForSecondsRealtime(3);Canvas.ForceUpdateCanvases();var panel=FindAnyObjectByType<PortTradePanel>();if(panel==null)throw new Exception("Trade panel missing");
  foreach(var name in new[]{"小麦","橄榄油","羊毛","陶器"})if(!FindObjectsByType<Text>(FindObjectsSortMode.None).Any(t=>t.gameObject.activeInHierarchy&&t.text==name))throw new Exception("Missing test cargo "+name);
  var saleScroll=FindObjectsByType<ScrollRect>(FindObjectsSortMode.None).Single(x=>x.name=="SaleGoods");
  if(!saleScroll.horizontal||saleScroll.vertical||saleScroll.horizontalScrollbar.gameObject.activeSelf)throw new Exception("Empty horizontal panel scrollbar should be hidden");
  var originalSize=saleScroll.content.sizeDelta;saleScroll.content.sizeDelta=new Vector2(1200,originalSize.y);Canvas.ForceUpdateCanvases();yield return null;
  var wheel=new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){scrollDelta=new Vector2(0,-1)};saleScroll.OnScroll(wheel);if(saleScroll.horizontalNormalizedPosition<=0)throw new Exception("Mouse wheel did not scroll horizontal panel");
  saleScroll.content.sizeDelta=originalSize;saleScroll.horizontalNormalizedPosition=0;Canvas.ForceUpdateCanvases();yield return null;
  if(FindObjectsByType<Text>(FindObjectsSortMode.None).Any(t=>t.gameObject.activeInHierarchy&&t.text=="选择商品开始交易"))throw new Exception("Default hint still visible");
  Directory.CreateDirectory("Evidence/TradeUI");CapturePanel("Evidence/TradeUI/market-real-data.png",panel);yield return new WaitForSecondsRealtime(2);
  var fleetScroll=FindObjectsByType<ScrollRect>(FindObjectsSortMode.None).First(x=>x.name=="Fleet");
  var own=fleetScroll.content.GetComponentsInChildren<Button>().Where(b=>b.interactable).Take(2).ToArray();own[0].onClick.Invoke();own[1].onClick.Invoke();yield return new WaitForSecondsRealtime(.5f);
  var products=GameObject.Find("CityGoods/Content");if(products==null){var sr=FindObjectsByType<ScrollRect>(FindObjectsSortMode.None).First(x=>x.name=="CityGoods");products=sr.content.gameObject;}
  var buttons=products.GetComponentsInChildren<Button>().Where(b=>b.interactable).Take(2).ToArray();if(buttons.Length<2)throw new Exception("Missing market offers");buttons[0].onClick.Invoke();yield return null;
  var refreshed=FindObjectsByType<ScrollRect>(FindObjectsSortMode.None).First(x=>x.name=="CityGoods").content.GetComponentsInChildren<Button>().First(b=>b.interactable);refreshed.onClick.Invoke();
  var ghosts=GameObject.Find("FlyingItems").GetComponentsInChildren<UnityEngine.UI.Image>();if(ghosts.Any(x=>x.sprite!=null)||GameObject.Find("FlyingItems").GetComponentsInChildren<Text>().Length!=0)throw new Exception("Flight contains product content");
  yield return new WaitForSecondsRealtime(1);
  if(products.GetComponentsInChildren<Button>().First()!=buttons[0])throw new Exception("Market cards were rebuilt");
  var hover=buttons[0].GetComponent<TradeCardHover>();var rt=buttons[0].GetComponent<RectTransform>();float resting=rt.anchoredPosition.y;hover.OnPointerEnter(null);yield return new WaitForSecondsRealtime(.15f);if(rt.anchoredPosition.y<resting+5)throw new Exception("Hover lift missing");CapturePanel("Evidence/TradeUI/market-hover.png",panel);hover.OnPointerExit(null);
  CapturePanel("Evidence/TradeUI/market-pending-purchases.png",panel);yield return new WaitForSecondsRealtime(2);
  int before=owner.s.cargo.batches.Sum(b=>b.quantity);var confirm=FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name=="确认成交"&&b.gameObject.activeInHierarchy);if(!confirm.interactable)throw new Exception("Cannot confirm draft");confirm.onClick.Invoke();yield return null;
  if(owner.s.cargo.batches.Sum(b=>b.quantity)!=before)throw new Exception("Runtime cargo commit mismatch");
  if(!File.Exists(Path.GetFullPath("Evidence/TradeUI/market-real-data.png")))throw new Exception("Screenshot missing");
  Debug.Log("TRADE_UI_RUNTIME_PASS");Application.Quit();
 }
}
}
