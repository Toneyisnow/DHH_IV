using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using MaritimeLegend.Domain;
namespace MaritimeLegend {
// Native uGUI implementation of the approved fixed 1920x1080 prototype.
public sealed class PortTradePanel : MonoBehaviour {
 MaritimeGame owner;GameService game;TradeSession draft;Canvas canvas;RectTransform root,body,flyLayer;Font font;Text status;
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
  Background(root,new Rect(0,0,1920,1080),new Color(.055f,.10f,.12f));var bg=Box(root,"Harbor",new Rect(0,0,1920,1080)).gameObject.AddComponent<RawImage>();bg.texture=Resources.Load<Texture2D>("Trading/harbor-background");bg.color=new Color(1,1,1,.20f);bg.raycastTarget=false;
  body=Box(root,"Interface",new Rect(0,0,1920,1080));flyLayer=Box(root,"FlyingItems",new Rect(0,0,1920,1080));
  foreach(var product in owner.c.products){var icon=Resources.Load<Sprite>("Sprites/Commodities/"+product.id);if(icon!=null)icons[product.id]=icon;}
 }
 RectTransform Box(Transform parent,string name,Rect r){var o=new GameObject(name,typeof(RectTransform));var t=o.GetComponent<RectTransform>();t.SetParent(parent,false);t.anchorMin=t.anchorMax=t.pivot=new Vector2(0,1);t.anchoredPosition=new Vector2(r.x,-r.y);t.sizeDelta=r.size;return t;}
 void Background(Transform parent,Rect r,Color color){var im=Box(parent,"Surface",r).gameObject.AddComponent<Image>();im.color=color;im.raycastTarget=false;}
 Text Text(Transform parent,Rect r,string value,int size=20,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var t=Box(parent,"Text",r).gameObject.AddComponent<Text>();t.font=font;t.text=value;t.fontSize=size;t.color=color??paper;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;return t;}
 RectTransform Button(Transform parent,Rect r,string title,Action action,bool enabled=true,Color? color=null){var t=Box(parent,title,r);var im=t.gameObject.AddComponent<Image>();im.color=color??panel;var b=t.gameObject.AddComponent<Button>();b.targetGraphic=im;b.interactable=enabled;b.onClick.AddListener(()=>action());if(title!="")Text(t,new Rect(4,0,r.width-8,r.height),title,18,enabled?paper:new Color(.4f,.45f,.43f),TextAnchor.MiddleCenter);return t;}
 RectTransform Scroll(Transform parent,string key,Rect r,float height){var view=Box(parent,key,r);view.gameObject.AddComponent<Image>().color=new Color(0,0,0,.01f);view.gameObject.AddComponent<RectMask2D>();var sr=view.gameObject.AddComponent<ScrollRect>();var content=Box(view,"Content",new Rect(0,0,r.width-16,Math.Max(height,r.height)));sr.viewport=view;sr.content=content;sr.horizontal=false;sr.vertical=true;sr.scrollSensitivity=45;sr.movementType=ScrollRect.MovementType.Clamped;
  var bar=Box(parent,key+"Scrollbar",new Rect(r.x+r.width-10,r.y,8,r.height));bar.gameObject.AddComponent<Image>().color=new Color(.1f,.2f,.22f,.5f);var handle=Box(bar,"Handle",new Rect(0,0,8,30));handle.gameObject.AddComponent<Image>().color=new Color(.43f,.51f,.47f);var scrollbar=bar.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=handle;scrollbar.targetGraphic=handle.GetComponent<Image>();scrollbar.direction=Scrollbar.Direction.BottomToTop;sr.verticalScrollbar=scrollbar;sr.verticalNormalizedPosition=scrollPositions.TryGetValue(key,out float v)?v:1;sr.onValueChanged.AddListener(p=>scrollPositions[key]=p.y);return content;}
 string Stars(int quality){return quality==0?"✦":new string('★',quality);}
 void Icon(Transform parent,Rect r,string id,bool dim=false){if(icons.TryGetValue(id,out Sprite sprite)){var im=Box(parent,"ProductImage",r).gameObject.AddComponent<Image>();im.sprite=sprite;im.preserveAspect=true;im.color=dim?new Color(.30f,.30f,.30f):Color.white;im.raycastTarget=false;}else Text(parent,r,"◇",34,new Color(.5f,.6f,.57f),TextAnchor.MiddleCenter);}
 void Item(Transform parent,Rect r,TradeSession.Item item,bool dim=false,bool market=false,int price=0){
  if(item==null){Text(parent,new Rect(r.x+15,r.y+10,65,r.height-20),"▱",40,new Color(.35f,.48f,.48f),TextAnchor.MiddleCenter);Text(parent,new Rect(r.x+88,r.y,90,r.height),"空货仓",16,new Color(.45f,.56f,.54f));return;}
  if(market){Icon(parent,new Rect(r.x+24,r.y+23,r.width-48,126),item.product,dim);Text(parent,new Rect(r.x+8,r.y+5,r.width-20,24),Stars(item.quality),16,grades[item.quality],TextAnchor.MiddleRight);Text(parent,new Rect(r.x+12,r.y+153,r.width-24,30),game.C.Product(item.product).name,20);Text(parent,new Rect(r.x+12,r.y+186,r.width-24,32),price.ToString("N0")+" 金币",22,gold);}
  else{Icon(parent,new Rect(r.x+6,r.y+10,78,78),item.product,dim);Text(parent,new Rect(r.x+100,r.y+14,r.width-105,20),Stars(item.quality),13,grades[item.quality]);Text(parent,new Rect(r.x+100,r.y+34,r.width-105,28),game.C.Product(item.product).name,16);Text(parent,new Rect(r.x+100,r.y+64,r.width-105,25),price.ToString("N0"),16,gold);}
 }
 void Draw(){
  foreach(Transform child in body){child.gameObject.SetActive(false);Destroy(child.gameObject);}targets.Clear();
  Text(body,new Rect(48,35,600,24),game.C.ports[game.S.city].name,16);Text(body,new Rect(48,66,360,55),"港口交易所",36,gold);Text(body,new Rect(370,76,550,36),GameCalendar.Date(game.S.sailing.calendar.elapsedGameDays),19);Text(body,new Rect(1420,44,430,60),"金币  "+game.S.gold.ToString("N0"),30,gold,TextAnchor.MiddleRight);
  Button(body,new Rect(1680,1020,190,42),"返回城市",()=>{owner.Change("城市");canvas.gameObject.SetActive(false);game=null;});
  Background(body,new Rect(48,137,1824,1),gold*.5f);Text(body,new Rect(48,160,500,40),"城市供货",26);Text(body,new Rect(674,160,1100,40),"船队与产业园",26);
  var offers=new List<Tuple<string,int>>();foreach(var m in game.S.cities[game.S.city].market)for(int q=0;q<SaveContract.QualityCount;q++)if(m.Stock(q)>0)offers.Add(Tuple.Create(m.product,q));
  offers=offers.OrderByDescending(o=>game.C.ports[game.S.city].raw.Contains(o.Item1)).ThenBy(o=>o.Item1).ThenByDescending(o=>o.Item2).ToList();
  var city=Scroll(body,"CityGoods",new Rect(48,211,590,415),(float)Math.Ceiling(offers.Count/3f)*268);
  for(int i=0;i<offers.Count;i++){var o=offers[i];int price=draft.Quote(o.Item1,o.Item2,true);var r=new Rect(i%3*192,i/3*268,180,256);bool can=draft.Stock(o.Item1,o.Item2)>0&&draft.balance>=price;RectTransform card=null;card=Button(city,r,"",()=>Buy(o.Item1,o.Item2,card),can);targets["market:"+o.Item1+":"+o.Item2]=card;Item(card,new Rect(0,0,180,256),new TradeSession.Item{product=o.Item1,quality=o.Item2},!can,true,price);Text(card,new Rect(12,223,156,26),"库存  "+draft.Stock(o.Item1,o.Item2),16);}
  Background(body,new Rect(48,641,590,1),gold*.3f);var sell=Scroll(body,"SaleGoods",new Rect(48,658,590,236),(float)Math.Ceiling(draft.sales.Count/3f)*110);
  for(int i=0;i<draft.sales.Count;i++){var sale=draft.sales[i];RectTransform card=null;card=Button(sell,new Rect(i%3*192,i/3*110,180,100),"",()=>Return(sale,card),!flying.Contains(sale.item.id));targets["sale:"+sale.item.id]=card;if(!flying.Contains(sale.item.id))Item(card,new Rect(0,0,180,100),sale.item,false,false,draft.Quote(sale.item.product,sale.item.quality,false));}
  float fleetHeight=0;foreach(var h in draft.holds.Where(x=>!x.warehouse))fleetHeight+=42+(float)Math.Ceiling(h.cells.Count/6f)*110+18;
  var fleet=Scroll(body,"Fleet",new Rect(674,211,1198,425),fleetHeight);float y=0;
  for(int h=0;h<draft.holds.Count;h++){var hold=draft.holds[h];if(hold.warehouse)continue;Text(fleet,new Rect(12,y,1130,34),hold.name+(hold.locked?" · 运输中":""),22,h==draft.lastHold?gold:paper);if(h==draft.lastHold)Background(fleet,new Rect(0,y,2,38),gold);y+=42;DrawHold(fleet,h,y);y+=(float)Math.Ceiling(hold.cells.Count/6f)*110+18;}
  Background(body,new Rect(674,652,1198,1),gold*.3f);Text(body,new Rect(686,662,1000,34),"产业园仓库",23,draft.holds.Last().warehouse&&draft.lastHold==draft.holds.Count-1?gold:paper);
  if(draft.holds.Last().warehouse){int h=draft.holds.Count-1;var warehouse=Scroll(body,"Warehouse",new Rect(674,703,1198,155),(float)Math.Ceiling(draft.holds[h].cells.Count/6f)*110);DrawHold(warehouse,h,0);}else Text(body,new Rect(686,720,1000,70),"当前城市尚未开放产业园仓库",20);
  Text(body,new Rect(674,870,1198,28),"点击空货仓选定收货船只或仓库 · 点击货物准备卖出 · 点击待售货物放回",16);
  Background(body,new Rect(48,914,1824,1),gold*.6f);Text(body,new Rect(48,937,1250,58),"买入  "+draft.buyTotal.ToString("N0")+"     卖出  "+draft.sellTotal.ToString("N0")+"     成交后余额  "+draft.balance.ToString("N0"),24,gold);
  Button(body,new Rect(1560,933,312,62),"确认成交",Commit,draft.Count>0&&draft.error==""&&flying.Count==0,new Color(.51f,.42f,.25f));status=Text(body,new Rect(48,1018,1550,44),draft.error==""?"选择商品开始交易；离开交易所将撤回尚未成交的操作。":draft.error,17);
 }
 void DrawHold(RectTransform parent,int h,float y){for(int i=0;i<draft.holds[h].cells.Count;i++){int slot=i;var item=draft.holds[h].cells[i];RectTransform card=null;card=Button(parent,new Rect(i%6*197+12,y+i/6*110,188,100),"",()=>Select(h,slot,card),!draft.holds[h].locked&&(item==null||!flying.Contains(item.id)));targets["hold:"+h+":"+i]=card;if(item==null||!flying.Contains(item.id))Item(card,new Rect(0,0,188,100),item,false,false,item==null?0:draft.Quote(item.product,item.quality,!item.bought?false:true));}}
 Vector2 Position(RectTransform card){return (Vector2)root.InverseTransformPoint(card.TransformPoint(new Vector3(0,0,0)))+new Vector2(960,-540);}
 void Animate(TradeSession.Item item,Vector2 from,string target){flying.Add(item.id);Draw();Canvas.ForceUpdateCanvases();if(!targets.TryGetValue(target,out var destination)){flying.Remove(item.id);Draw();return;}Vector2 to=Position(destination);to.x=Mathf.Clamp(to.x,48,1684);to.y=Mathf.Clamp(to.y,-800,-211);StartCoroutine(Fly(item,from,to));}
 IEnumerator Fly(TradeSession.Item item,Vector2 from,Vector2 to){var ghost=Box(flyLayer,"InFlight",new Rect(from.x,-from.y,188,100));Background(ghost,new Rect(0,0,188,100),panel);Item(ghost,new Rect(0,0,188,100),item,false,false,draft.Quote(item.product,item.quality,item.bought));float start=Time.unscaledTime;while(Time.unscaledTime-start<.323f){ghost.anchoredPosition=Vector2.Lerp(from,to,(Time.unscaledTime-start)/.323f);yield return null;}Destroy(ghost.gameObject);flying.Remove(item.id);if(canvas.gameObject.activeSelf)Draw();}
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
