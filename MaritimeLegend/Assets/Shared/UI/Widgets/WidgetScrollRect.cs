using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace MaritimeLegend.UI.Widgets {
public sealed class WidgetScrollRect : ScrollRect {
 public override void OnScroll(PointerEventData e){
  if(!IsActive())return;
  if(horizontal&&!vertical){Canvas.ForceUpdateCanvases();float overflow=content.rect.width-viewport.rect.width;if(overflow<=.5f)return;float delta=Mathf.Abs(e.scrollDelta.x)>.01f?e.scrollDelta.x:-e.scrollDelta.y;StopMovement();horizontalNormalizedPosition=Mathf.Clamp01(horizontalNormalizedPosition+delta*scrollSensitivity/overflow);}
  else base.OnScroll(e);
 }
}
}
