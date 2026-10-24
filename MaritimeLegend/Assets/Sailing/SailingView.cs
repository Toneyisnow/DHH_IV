using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
public partial class MaritimeGame {
 LocalizationService sailingText;
 string SeaText(string key,params object[] args){if(sailingText==null)sailingText=new LocalizationService(c.localizationPlan);return sailingText.Text(CurrentLanguage,key,args);}
 internal void SailingView(){
  var voyage=s.sailing;var sh=s.ships[0];var prompt=g.PromptedEncounter;bool enabled=GUI.enabled;GUI.enabled=enabled&&prompt==null;
  ManualSeaBackdrop();
  var origin=new Vector2(voyage.manualSpawnX,voyage.manualSpawnY);float distance=Vector2.Distance(SeaPosition,origin);
  Fill(new Rect(32,28,440,94),new Color(.025f,.09f,.13f,.76f));Label(54,40,420,40,"印度洋 · 手动航海",heading);Label(56,86,400,30,GameCalendar.Date(voyage.calendar.elapsedGameDays)+"  /  三船纵队",small);
  if(Btn(1680,32,208,54,"暂停 / ESC"))paused=true;
  if(Btn(1430,100,458,50,"返回出发水域 · "+voyage.manualSpawnPort)){seaTarget=origin;seaHasTarget=true;seaStopped=false;}
  Fill(new Rect(30,958,1860,94),new Color(.025f,.09f,.13f,.88f));
  Label(52,971,970,34,(seaStopped?"已停船":seaCoast.Contact?(seaCoast.Sliding?"沿岸慢行":"海岸阻挡"):"航行中")+"  ·  航速 "+voyage.speed.ToString("0.000")+"  ·  船体 "+sh.hull.ToString("0")+"  ·  粮 "+sh.food.ToString("0.0")+"  ·  水 "+sh.water.ToString("0.0"),small);
  Label(52,1011,1000,32,"左键航行 · 右键水平环绕 · 滚轮缩放 · 空格停船 · W/S 调速 · A/D 调帆",small);
  if(Btn(1080,976,190,56,seaStopped?"继续航行":"停船"))seaStopped=!seaStopped;
  if(Btn(1290,976,280,56,"结束试航 / 返回城市",distance<.025f)){
   s.atSea=false;s.city=s.seaFrom;voyage.speed=0;voyage.manualMapId="";g.SetSeaRegion(c.sailingDefaults.portRegions[s.city]);seaOwner=null;Change("城市");
  }
  if(Btn(1590,976,270,56,"紧急返港救援")){Notify(g.Rescue(),10);seaOwner=null;Change("城市");}
  Label(40,914,1350,34,"出发水域："+voyage.manualSpawnPort+"  ·  距离 "+distance.ToString("0.000")+"  ·  俯角 "+indianOcean.Pitch.ToString("0")+"°"+(g.ManualFleet.shortage?"  ·  补给不足，请返港":""),small);
  SailingInstruments();
  GUI.enabled=enabled;
  if(prompt!=null&&!paused){Panel(new Rect(355,425,1210,300));Label(390,455,1130,110,SeaText(c.SeaEncounter(prompt.definitionId).promptKey));if(Btn(420,610,510,65,SeaText("ui.sailing.accept")))ResolveSeaPrompt(prompt,true);if(Btn(990,610,510,65,SeaText("ui.sailing.decline")))ResolveSeaPrompt(prompt,false);}
 }
 void ResolveSeaPrompt(SeaEncounterState e,bool accept){var key=g.ResolveSeaEncounter(e.instanceId,accept);Notify(SeaText(key,e.quantity>0?c.Product(e.productId).name:"",e.quantity,e.energyCost));Save(true);}
}
}
