using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
public partial class MaritimeGame {
 LocalizationService sailingText;
 string SeaText(string key,params object[] args){if(sailingText==null)sailingText=new LocalizationService(c.localizationPlan);return sailingText.Text(s.settings.locale,key,args);}
 internal void SailingView(){
  var voyage=s.sailing;var sh=s.ships[0];var prompt=g.PromptedEncounter;bool enabled=GUI.enabled;GUI.enabled=enabled&&prompt==null;
  ManualSeaBackdrop();
  Fill(new Rect(32,28,410,94),new Color(.025f,.09f,.13f,.76f));Label(54,40,380,40,"东南亚海 · 手动航海",heading);Label(56,86,370,30,GameCalendar.Date(voyage.calendar.elapsedGameDays)+"  /  四船编队",small);
  if(Btn(1680,32,208,54,"暂停 / ESC"))paused=true;
  if(Btn(1430,100,458,50,"朝目的港航行 · "+c.ports[s.seaTo].name)){seaTarget=new Vector2((float)c.ports[s.seaTo].x,(float)c.ports[s.seaTo].y);seaHasTarget=true;seaStopped=false;}
  Fill(new Rect(30,958,1860,94),new Color(.025f,.09f,.13f,.88f));
  Label(52,971,880,34,(seaStopped?"已停船":"航行中")+"  ·  航速 "+voyage.speed.ToString("0.000")+"  ·  船体 "+sh.hull.ToString("0")+"  ·  粮 "+sh.food.ToString("0.0")+"  ·  水 "+sh.water.ToString("0.0"),small);
  Label(52,1011,970,32,"单击海面航行 · 空格停船/继续 · W/S 调速 · A/D 调帆 · R 复位",small);
  if(Btn(1050,976,220,56,seaStopped?"继续航行":"停船"))seaStopped=!seaStopped;
  if(Btn(1290,976,280,56,"驶入目的港",g.DistanceToPort()<=c.balance.navigation.arrivalRadius)){
   Notify(g.Arrive());if(!s.atSea){seaOwner=null;page="城市";}
   if(!s.atSea&&s.finalVoyage&&s.city==c.balance.quests.finalPort){if(s.mode=="经典")StartBattle(true);else if(sh.food>=5&&sh.hull>=10){g.Finish();Change("结局");Save(false);sound.Cue(9);}else Notify("远航物资不足，请整备后重新发起挑战");}
  }
  if(Btn(1590,976,270,56,"紧急返港救援")){Notify(g.Rescue(),10);seaOwner=null;page="城市";}
  Label(40,914,1100,34,"目的港："+c.ports[s.seaTo].name+"  ·  距离 "+g.DistanceToPort().ToString("0.000")+(g.ManualFleet.shortage?"  ·  补给不足，请返港":""),small);
  GUI.enabled=enabled;
  if(prompt!=null&&!paused){Panel(new Rect(355,425,1210,300));Label(390,455,1130,110,SeaText(c.SeaEncounter(prompt.definitionId).promptKey));if(Btn(420,610,510,65,SeaText("ui.sailing.accept")))ResolveSeaPrompt(prompt,true);if(Btn(990,610,510,65,SeaText("ui.sailing.decline")))ResolveSeaPrompt(prompt,false);}
 }
 void ResolveSeaPrompt(SeaEncounterState e,bool accept){var key=g.ResolveSeaEncounter(e.instanceId,accept);Notify(SeaText(key,e.quantity>0?c.Product(e.productId).name:"",e.quantity,e.energyCost));Save(true);}
}
}
