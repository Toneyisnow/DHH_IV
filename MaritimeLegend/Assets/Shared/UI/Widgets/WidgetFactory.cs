using System;
using UnityEngine;
using UnityEngine.UI;
namespace MaritimeLegend.UI.Widgets {
// The caller supplies a pooled or fresh rectangle factory; widgets have no game-state dependency.
public sealed class WidgetFactory {
 public readonly Func<Transform,string,Rect,RectTransform> Box;
 public readonly Font font;public readonly Color textColor,accent;
 public WidgetFactory(Func<Transform,string,Rect,RectTransform> box,Font font,Color textColor,Color accent){Box=box;this.font=font;this.textColor=textColor;this.accent=accent;}
 public static T Get<T>(GameObject go) where T:Component=>go.GetComponent<T>()??go.AddComponent<T>();
 public Text Label(Transform parent,Rect rect,string value,int size,Color? color=null,TextAnchor align=TextAnchor.MiddleLeft){var t=Get<Text>(Box(parent,"Text",rect).gameObject);t.font=font;t.text=value;t.fontSize=size;t.color=color??textColor;t.alignment=align;t.raycastTarget=false;t.horizontalOverflow=HorizontalWrapMode.Wrap;return t;}
 public Image Image(Transform parent,string name,Rect rect,Sprite sprite,Color color){var im=Get<Image>(Box(parent,name,rect).gameObject);im.sprite=sprite;im.color=color;im.raycastTarget=false;im.preserveAspect=sprite!=null;return im;}
}
public static class CardWidget {
 public static RectTransform Create(WidgetFactory ui,Transform parent,Rect rect,string title,Action click,bool enabled,Color background){var t=ui.Box(parent,title,rect);var image=WidgetFactory.Get<Image>(t.gameObject);image.color=background;image.raycastTarget=true;var button=WidgetFactory.Get<Button>(t.gameObject);button.targetGraphic=image;button.interactable=enabled;button.onClick.RemoveAllListeners();button.onClick.AddListener(()=>click?.Invoke());
  if(string.IsNullOrEmpty(title))WidgetFactory.Get<TradeCardHover>(t.gameObject).Bind(t,button);else ui.Label(t,new Rect(4,0,rect.width-8,rect.height),title,18,enabled?ui.textColor:new Color(.4f,.45f,.43f),TextAnchor.MiddleCenter);return t;
 }
}
public sealed class StockCardData {public Sprite image;public string name,grade,price,stock;public Color gradeColor;public bool dim;}
public static class StockCardWidget {
 public static void Render(WidgetFactory ui,Transform parent,Rect r,StockCardData data,bool market){
  if(data==null){ui.Label(parent,new Rect(r.x+15,r.y+10,65,r.height-20),"▱",40,new Color(.35f,.48f,.48f),TextAnchor.MiddleCenter);ui.Label(parent,new Rect(r.x+88,r.y,90,r.height),"空货仓",16,new Color(.45f,.56f,.54f));return;}
  var imageRect=market?new Rect(r.x+24,r.y+30,r.width-48,116):new Rect(r.x+6,r.y+10,78,78);
  if(data.image!=null)ui.Image(parent,"ProductImage",imageRect,data.image,data.dim?new Color(.3f,.3f,.3f):Color.white);else ui.Label(parent,imageRect,"◇",34,null,TextAnchor.MiddleCenter);
  if(market){ui.Label(parent,new Rect(r.x+98,r.y+5,r.width-108,24),data.grade,16,data.gradeColor,TextAnchor.MiddleRight);ui.Label(parent,new Rect(r.x+12,r.y+148,r.width-24,30),data.name,20);ui.Label(parent,new Rect(r.x+12,r.y+181,r.width-24,32),data.price,22,ui.accent);ui.Label(parent,new Rect(r.x+10,r.y+5,88,24),data.stock,14);}
  else{ui.Label(parent,new Rect(r.x+100,r.y+14,r.width-105,20),data.grade,13,data.gradeColor);ui.Label(parent,new Rect(r.x+100,r.y+34,r.width-105,28),data.name,16);ui.Label(parent,new Rect(r.x+100,r.y+64,r.width-105,25),data.price,16,ui.accent);}
 }
}
}
