using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MaritimeLegend.Domain;
using MaritimeLegend.UI.Widgets;
namespace MaritimeLegend {
// Native uGUI implementation of the approved fixed 1920x1080 prototype.
public sealed class PortTradePanel : MonoBehaviour {
 MaritimeGame owner;GameService game;TradeSession draft;Canvas canvas;RectTransform root,body,flyLayer;Font font;Text status;
 bool drawing;
 readonly Dictionary<string,RectTransform> pooled=new Dictionary<string,RectTransform>();
 readonly Dictionary<int,int> cursors=new Dictionary<int,int>();readonly HashSet<RectTransform> used=new HashSet<RectTransform>();
 static T Component<T>(GameObject go) where T:Component {return go.GetComponent<T>()??go.AddComponent<T>();}
 readonly Dictionary<string,Sprite> icons=new Dictionary<string,Sprite>();readonly Dictionary<string,RectTransform> targets=new Dictionary<string,RectTransform>();
 readonly Dictionary<string,float> scrollPositions=new Dictionary<string,float>();readonly HashSet<int> flying=new HashSet<int>();
 readonly Color paper=new Color(.90f,.86f,.75f),gold=new Color(.84f,.72f,.47f),panel=new Color(.07f,.14f,.17f,.87f);
 readonly Color[] grades={new Color(.85f,.9f,1),new Color(.6f,.76f,.55f),new Color(.5f,.76f,.81f),new Color(.69f,.63f,.84f),new Color(.82f,.67f,.45f),new Color(.95f,.81f,.48f)};
 public void Open(MaritimeGame host,GameService service){owner=host;if(canvas==null)BuildCanvas();if(game!=service){game=service;draft=new TradeSession(game);canvas.gameObject.SetActive(true);Draw();}else if(!canvas.gameObject.activeSelf){canvas.gameObject.SetActive(true);Draw();}}
 void Update(){if(canvas!=null&&canvas.gameObject.activeSelf&&owner!=null&&(owner.page!="市场"||!owner.TradeCanInteract)){canvas.gameObject.SetActive(false);StopAllCoroutines();flying.Clear();foreach(Transform child in flyLayer)Destroy(child.gameObject);if(owner.page!="市场")game=null;}}

 void BuildCanvas(){
  font=Font.CreateDynamicFontFromOSFont(new[]{"Microsoft YaHei","SimHei","Arial"},22);
  var go=new GameObject("PortTradeCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));go.transform.SetParent(transform,false);canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=20;
  var scale=go.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
  root=Box(go.transform,"TradeDialog",new Rect(0,0,1920,1080));root.anchorMin=root.anchorMax=root.pivot=new Vector2(.5f,.5f);root.anchoredPosition=Vector2.zero;
  Background(root,new Rect(0,0,1920,1080),new Color(.055f,.10f,.12f));var bg=Component<RawImage>(Box(root,"Harbor",new Rect(0,0,1920,1080)).gameObject);bg.texture=Resources.Load<Texture2D>("Trading/harbor-background");bg.color=new Color(1,1,1,.20f);bg.raycastTarget=false;
  body=Box(root,"Interface",new Rect(0,0,1920,1080));flyLayer=Box(root,"FlyingItems",new Rect(0,0,1920,1080));flyLayer.gameObject.AddComponent<Canvas>();
  foreach(var product in owner.c.products){var icon=Resources.Load<Sprite>("Sprites/Commodities/"+product.id);if(icon!=null)icons[product.id]=icon;}
 }
 RectTransform Box(Transform parent,string name,Rect r){
  RectTransform t;int id=parent.GetInstanceID();cursors.TryGetValue(id,out int n);string key=id+":"+n+":"+name;
  if(drawing)cursors[id]=n+1;
  if(!drawing||!pooled.TryGetValue(key,out t)||t==null){var o=new GameObject(name,typeof(RectTransform));t=o.GetComponent<RectTransform>();t.SetParent(parent,false);if(drawing)pooled[key]=t;}
  if(drawing)used.Add(t);t.gameObject.SetActive(true);t.anchorMin=t.anchorMax=t.pivot=new Vector2(0,1);t.anchoredPosition=new Vector2(r.x,-r.y);t.sizeDelta=r.size;return t;
 }
 void Background(Transform parent,Rect r,Color color){var im=Component<Image>(Box(parent,"Surface",r).gameObject);im.color=color;im.raycastTarget=false;}
 Text Text(Transform parent,Rect r,string value,int size=20,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var t=Component<Text>(Box(parent,"Text",r).gameObject);t.font=font;t.text=value;t.fontSize=size;t.color=color??paper;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;return t;}
 RectTransform Button(Transform parent,Rect r,string title,Action action,bool enabled=true,Color? color=null){return CardWidget.Create(Widgets,parent,r,title,action,enabled,color??panel);}
 WidgetFactory widgets;
 WidgetFactory Widgets=>widgets??(widgets=new WidgetFactory(Box,font,paper,gold));
 RectTransform Scroll(Transform parent,string key,Rect r,float extent,bool horizontal=false){
  var result=PanelWidget.Scroll(Widgets,parent,key,r,extent,horizontal,scrollPositions.TryGetValue(key,out float v)?v:horizontal?0:1);
  result.scroll.onValueChanged.AddListener(p=>scrollPositions[key]=horizontal?p.x:p.y);return result.content;
 }
 void Item(Transform parent,Rect r,TradeSession.Item item,bool dim=false,bool market=false,int price=0){
  StockCardData data=null;
  if(item!=null){icons.TryGetValue(item.product,out var sprite);data=new StockCardData{image=sprite,name=game.C.Product(item.product).name,grade=item.quality==0?"✦":new string('★',item.quality),gradeColor=grades[item.quality],price=price.ToString("N0")+(market?" 金币":""),dim=dim,stock=market?"库存  "+draft.Stock(item.product,item.quality):null};}
  StockCardWidget.Render(Widgets,parent,r,data,market);
 }
 void Draw(){
  drawing=true;cursors.Clear();used.Clear();targets.Clear();
  Text(body,new Rect(48,15,600,24),game.C.ports[game.S.city].name,16);Text(body,new Rect(48,36,360,55),"港口交易所",36,gold);Text(body,new Rect(370,46,550,36),GameCalendar.Date(game.S.sailing.calendar.elapsedGameDays),19);Text(body,new Rect(1420,24,430,60),"金币  "+game.S.gold.ToString("N0"),30,gold,TextAnchor.MiddleRight);
  Button(body,new Rect(1676,1018,196,48),"返回城市",()=>{owner.Change("城市");canvas.gameObject.SetActive(false);game=null;});
  Background(body,new Rect(48,107,1824,1),gold*.5f);Text(body,new Rect(48,120,500,40),"城市供货",26);Text(body,new Rect(674,120,1100,40),"船队与产业园",26);
  var offers=new List<Tuple<string,int>>();foreach(var m in game.S.cities[game.S.city].market)for(int q=0;q<SaveContract.QualityCount;q++)if(m.Stock(q)>0)offers.Add(Tuple.Create(m.product,q));
  offers=offers.OrderByDescending(o=>game.C.ports[game.S.city].raw.Contains(o.Item1)).ThenBy(o=>o.Item1).ThenByDescending(o=>o.Item2).ToList();
  var city=Scroll(body,"CityGoods",new Rect(48,171,590,700),16+(float)Math.Ceiling(offers.Count/3f)*232-12);
  for(int i=0;i<offers.Count;i++){var o=offers[i];int price=draft.Quote(o.Item1,o.Item2,true);var r=new Rect(i%3*192,i/3*232+8,180,220);bool can=draft.Stock(o.Item1,o.Item2)>0&&draft.balance>=price;RectTransform card=null;card=Button(city,r,"",()=>Buy(o.Item1,o.Item2,card),can);targets["market:"+o.Item1+":"+o.Item2]=card;Item(card,new Rect(0,0,180,220),new TradeSession.Item{product=o.Item1,quality=o.Item2},!can,true,price);}
  Background(body,new Rect(48,879,590,1),gold*.3f);var sell=Scroll(body,"SaleGoods",new Rect(48,887,590,110),Math.Max(0,draft.sales.Count*192-12),true);
  for(int i=0;i<draft.sales.Count;i++){var sale=draft.sales[i];RectTransform card=null;card=Button(sell,new Rect(i*192,6,180,100),"",()=>Return(sale,card),!flying.Contains(sale.item.id));targets["sale:"+sale.item.id]=card;if(!flying.Contains(sale.item.id))Item(card,new Rect(0,0,180,100),sale.item,false,false,draft.Quote(sale.item.product,sale.item.quality,false));}
  float fleetHeight=0;foreach(var h in draft.holds.Where(x=>!x.warehouse))fleetHeight+=42+(float)Math.Ceiling(Math.Max(6,h.cells.Count)/6f)*110+18;
  var fleet=Scroll(body,"Fleet",new Rect(674,171,1198,395),fleetHeight);float y=0;
  for(int h=0;h<draft.holds.Count;h++){var hold=draft.holds[h];if(hold.warehouse)continue;Text(fleet,new Rect(12,y,1130,34),hold.name+(hold.locked?" · 运输中":""),22,h==draft.lastHold?gold:paper);if(h==draft.lastHold)Background(fleet,new Rect(0,y,2,38),gold);y+=42;DrawHold(fleet,h,y);y+=(float)Math.Ceiling(Math.Max(6,hold.cells.Count)/6f)*110+18;}
  Background(body,new Rect(674,582,1198,1),gold*.3f);Text(body,new Rect(686,592,1000,34),"产业园仓库",23,draft.holds.Last().warehouse&&draft.lastHold==draft.holds.Count-1?gold:paper);
  if(draft.holds.Last().warehouse){int h=draft.holds.Count-1;var warehouse=Scroll(body,"Warehouse",new Rect(674,633,1198,225),(float)Math.Ceiling(draft.holds[h].cells.Count/6f)*110);DrawHold(warehouse,h,0);}else Text(body,new Rect(686,650,1000,70),"当前城市尚未开放产业园仓库",20);
  Text(body,new Rect(674,970,1198,28),"点击空货仓选定收货船只或仓库 · 点击货物准备卖出 · 点击待售货物放回",16);
  Background(body,new Rect(48,1012,1824,1),gold*.6f);
  var totals=Text(body,new Rect(48,1018,850,48),"买入  "+draft.buyTotal.ToString("N0")+"     卖出  "+draft.sellTotal.ToString("N0")+"     成交后余额  "+draft.balance.ToString("N0"),22,gold);
  float hintX=48+Math.Min(totals.preferredWidth,850)+24;totals.rectTransform.sizeDelta=new Vector2(hintX-72,48);
  status=Text(body,new Rect(hintX,1018,1432-hintX,48),draft.error,16);
  Button(body,new Rect(1456,1018,196,48),"确认成交",Commit,draft.Count>0&&draft.error==""&&flying.Count==0,new Color(.51f,.42f,.25f));
  foreach(var node in pooled.Values)if(node!=null&&!used.Contains(node))node.gameObject.SetActive(false);drawing=false;
 }
 void DrawHold(RectTransform parent,int h,float y){var hold=draft.holds[h];int count=hold.warehouse?hold.cells.Count:Math.Max(6,hold.cells.Count);
  for(int i=0;i<count;i++){int slot=i;bool reserved=i>=hold.slotLimit&&(i>=hold.cells.Count||hold.cells[i]==null);var item=i<hold.cells.Count?hold.cells[i]:null;RectTransform card=null;
   card=Button(parent,new Rect(i%6*197+12,y+i/6*110+6,188,100),"",()=>Select(h,slot,card),!reserved&&!hold.locked&&(item==null||!flying.Contains(item.id)),reserved?new Color(.08f,.14f,.16f,.3f):(Color?)null);
   targets["hold:"+h+":"+i]=card;
   if(reserved){Text(card,new Rect(0,0,188,100),i<6&&!hold.warehouse?"预留货仓":"—",16,new Color(.33f,.43f,.43f),TextAnchor.MiddleCenter);continue;}
   if(item==null||!flying.Contains(item.id))Item(card,new Rect(0,0,188,100),item,false,false,item==null?0:draft.Quote(item.product,item.quality,!item.bought));
   if(i>=hold.slotLimit)Text(card,new Rect(3,80,95,18),"待整理货物",11,gold);
  }
 }
 Vector2 Position(RectTransform card){return (Vector2)root.InverseTransformPoint(card.TransformPoint(new Vector3(0,0,0)))+new Vector2(960,-540);}
 void Animate(TradeSession.Item item,Vector2 from,string target){flying.Add(item.id);Draw();Canvas.ForceUpdateCanvases();if(!targets.TryGetValue(target,out var destination)){flying.Remove(item.id);Draw();return;}Vector2 to=Position(destination);to.x=Mathf.Clamp(to.x,48,1684);to.y=Mathf.Clamp(to.y,-893,-171);StartCoroutine(Fly(item,from,to));}
 IEnumerator Fly(TradeSession.Item item,Vector2 from,Vector2 to){var ghost=Box(flyLayer,"InFlight",new Rect(from.x,-from.y,188,100));Background(ghost,new Rect(0,0,188,2),gold);Background(ghost,new Rect(0,98,188,2),gold);Background(ghost,new Rect(0,0,2,100),gold);Background(ghost,new Rect(186,0,2,100),gold);float start=Time.unscaledTime;while(Time.unscaledTime-start<.323f){ghost.anchoredPosition=Vector2.Lerp(from,to,(Time.unscaledTime-start)/.323f);yield return null;}Destroy(ghost.gameObject);flying.Remove(item.id);if(canvas.gameObject.activeSelf)Draw();}
 void Buy(string id,int q,RectTransform card){Vector2 from=Position(card);if(!draft.Buy(id,q,out int slot)){status.text=draft.error;return;}Animate(draft.holds[draft.lastHold].cells[slot],from,"hold:"+draft.lastHold+":"+slot);}
 void Select(int h,int slot,RectTransform card){Vector2 from=Position(card);var item=draft.holds[h].cells[slot];if(!draft.Select(h,slot)){Draw();return;}Animate(item,from,item.bought?"market:"+item.product+":"+item.quality:"sale:"+item.id);}
 void Return(TradeSession.Sale sale,RectTransform card){Vector2 from=Position(card);if(!draft.Return(sale,out int slot)){status.text=draft.error;return;}Animate(sale.item,from,"hold:"+sale.hold+":"+slot);}
 void Commit(){if(flying.Count>0)return;if(!draft.Commit(out GameState next)){status.text=draft.error;return;}owner.ApplyTrade(next);game=owner.g;draft=new TradeSession(game);Draw();status.text="交易完成，已自动保存。";}
}
public partial class MaritimeGame {
 PortTradePanel tradePanel;
 internal bool TradeCanInteract=>!paused&&!settingsOpen&&confirm==""&&s!=null&&s.pendingEvents.Count==0;
 internal void ApplyTrade(GameState state){s=state;g=new GameService(c,s);Save(true);}
 internal void OpenTradePanel(){if(!TradeCanInteract)return;if(tradePanel==null)tradePanel=gameObject.AddComponent<PortTradePanel>();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  if(g.SeedTradeTestCargo())Save(true);
#endif
  tradePanel.Open(this,g);}
}
}
