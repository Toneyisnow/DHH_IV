using UnityEngine;
using UnityEngine.UI;
namespace MaritimeLegend.UI.Widgets {
public static class PanelWidget {
 public struct Result {public RectTransform content;public WidgetScrollRect scroll;}
 public static void Border(WidgetFactory ui,Transform parent,string name,Rect rect){var border=WidgetFactory.Get<PanelBorder>(ui.Box(parent,name,rect).gameObject);border.color=Color.black;border.thickness=2;border.raycastTarget=false;}
 public static Result Scroll(WidgetFactory ui,Transform parent,string key,Rect rect,float extent,bool horizontal,float position){
  var view=ui.Box(parent,key,rect);WidgetFactory.Get<Image>(view.gameObject).color=new Color(0,0,0,.01f);WidgetFactory.Get<RectMask2D>(view.gameObject);var scroll=WidgetFactory.Get<WidgetScrollRect>(view.gameObject);
  var content=ui.Box(view,"Content",new Rect(0,0,horizontal?Mathf.Max(extent,rect.width):rect.width-16,horizontal?rect.height:Mathf.Max(extent,rect.height)));
  scroll.viewport=view;scroll.content=content;scroll.horizontal=horizontal;scroll.vertical=!horizontal;scroll.scrollSensitivity=45;scroll.movementType=ScrollRect.MovementType.Clamped;
  var bar=ui.Box(parent,key+"Scrollbar",horizontal?new Rect(rect.x,rect.y+rect.height+4,rect.width,8):new Rect(rect.x+rect.width-10,rect.y,8,rect.height));WidgetFactory.Get<Image>(bar.gameObject).color=new Color(.1f,.2f,.22f,.5f);
  var handle=ui.Box(bar,"Handle",horizontal?new Rect(0,0,30,8):new Rect(0,0,8,30));WidgetFactory.Get<Image>(handle.gameObject).color=new Color(.43f,.51f,.47f);
  var scrollbar=WidgetFactory.Get<Scrollbar>(bar.gameObject);scrollbar.handleRect=handle;scrollbar.targetGraphic=handle.GetComponent<Image>();scrollbar.direction=horizontal?Scrollbar.Direction.LeftToRight:Scrollbar.Direction.BottomToTop;
  scroll.onValueChanged.RemoveAllListeners();scroll.horizontalScrollbar=horizontal?scrollbar:null;scroll.verticalScrollbar=horizontal?null:scrollbar;
  scroll.horizontalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
  if(horizontal)scroll.horizontalNormalizedPosition=position;else scroll.verticalNormalizedPosition=position;
  bar.gameObject.SetActive(extent>(horizontal?rect.width:rect.height)+.5f);Border(ui,parent,key+"Border",rect);
  return new Result{content=content,scroll=scroll};
 }
}
}
