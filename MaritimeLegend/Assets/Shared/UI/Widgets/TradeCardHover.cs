using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace MaritimeLegend {
// Translation only: keep the sprite at its original scale and preserve the layout footprint.
public sealed class TradeCardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
 RectTransform rect;Button button;Outline border;Vector2 rest;bool over;float lift;
 public void Bind(RectTransform target,Button control){rect=target;button=control;rest=target.anchoredPosition;
  if(border==null){border=gameObject.AddComponent<Outline>();border.effectColor=new Color(1,.82f,.4f,1);border.effectDistance=new Vector2(2,-2);border.useGraphicAlpha=false;}
  Apply();
 }
 public void OnPointerEnter(PointerEventData e){over=true;enabled=true;}
 public void OnPointerExit(PointerEventData e){over=false;enabled=true;}
 void Update(){float goal=over&&button!=null&&button.interactable?6:0;if(Mathf.Approximately(lift,goal))return;lift=Mathf.MoveTowards(lift,goal,60*Time.unscaledDeltaTime);Apply();}
 void Apply(){if(rect!=null)rect.anchoredPosition=rest+Vector2.up*lift;if(border!=null)border.enabled=over&&button!=null&&button.interactable;}
 void OnDisable(){if(!gameObject.activeInHierarchy){over=false;lift=0;if(rect!=null)rect.anchoredPosition=rest;if(border!=null)border.enabled=false;}}
}
}
