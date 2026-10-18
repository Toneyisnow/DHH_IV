using System;
using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
namespace MaritimeLegend {
public partial class MaritimeGame {
    LocalizationService sailingText;
    string SeaText(string key,params object[] args) {
        if(sailingText==null)sailingText=new LocalizationService(c.localizationPlan);
        return sailingText.Text(s.settings.locale,key,args);
    }
    internal void SailingView() {
        var voyage=s.sailing;var sh=s.ships[0];var prompt=g.PromptedEncounter;
        bool enabled=GUI.enabled;GUI.enabled=enabled&&prompt==null;
        Panel(new Rect(30,130,1405,755));
        if(map)GUI.DrawTexture(new Rect(40,140,1385,735),map,ScaleMode.ScaleAndCrop);
        Fill(new Rect(40,140,1385,735),new Color(.015f,.12f,.16f,.6f));
        Label(65,148,1260,60,GameCalendar.Date(voyage.calendar.elapsedGameDays)+" · "+SeaText(c.SeaRegion(voyage.regionId).nameKey),heading);
        foreach(var p in c.ports){var v=MapPoint(p.x,p.y);if(Btn(v.x-55,v.y+16,160,45,p.name))selectedPort=p.id;}
        foreach(var e in voyage.encounters) {
            if(e.claimed||e.regionId!=voyage.regionId)continue;
            var v=MapPoint(e.x,e.y);
            // Temporary geometry; domain state remains the authority for rewards.
            Fill(new Rect(v.x-7,v.y-7,14,14),new Color(.5f,.9f,.8f));
        }
        var shipPoint=MapPoint(s.seaX,s.seaY);
        DrawShip(shipPoint,(float)s.heading*Mathf.Rad2Deg,gold);
        float sail=(float)(s.heading+SailingFormulas.SailDegrees(s.trim)*Math.PI/180);
        var sailLine=new Vector2(Mathf.Cos(sail),Mathf.Sin(sail))*24;
        Line(shipPoint-sailLine,shipPoint+sailLine,Color.white,3);
        Label(65,755,1290,85,"鼠标按住指定航向 · A/D 或方向键调帆 · R 重置帆位 · W/S 调速\n风向 "+SailingFormulas.HeadingDegrees(s.wind).ToString("0")+"° · 目的港 "+c.ports[s.seaTo].name,small);
        Panel(new Rect(1460,130,430,755));Label(1490,160,365,55,"航行状态",heading);
        Label(1490,235,360,340,"船体 "+sh.hull.ToString("0")+"   船员 "+sh.crew+
            "\n粮食 "+sh.food.ToString("0.0")+"   水 "+sh.water.ToString("0.0")+
            "\n精力 "+voyage.energy.ToString("0.0")+"   舰队经验 "+g.ManualFleet.experience+
            "\n航向 "+SailingFormulas.HeadingDegrees(s.heading).ToString("0")+"°   帆角 "+SailingFormulas.SailDegrees(s.trim).ToString("0")+"°"+
            "\n航速 "+voyage.speed.ToString("0.000")+"   距离 "+g.DistanceToPort().ToString("0.000")+
            (g.ManualFleet.shortage?"\n补给不足，请返港或求援。":""),small);
        if(Btn(1490,620,365,65,"驶入目的港",g.DistanceToPort()<=c.balance.navigation.arrivalRadius)) {
            Notify(g.Arrive());if(!s.atSea)page="城市";
            if(!s.atSea&&s.finalVoyage&&s.city==c.balance.quests.finalPort) {
                if(s.mode=="经典")StartBattle(true);
                else if(sh.food>=5&&sh.hull>=10){g.Finish();Change("结局");Save(false);sound.Cue(9);}
                else Notify("远航物资不足，请整备后重新发起挑战");
            }
        }
        if(Btn(1490,710,365,60,"紧急返港救援")){Notify(g.Rescue(),10);page="城市";}
        GUI.enabled=enabled;
        if(prompt!=null&&!paused) {
            Panel(new Rect(355,425,1210,300));
            Label(390,455,1130,110,SeaText(c.SeaEncounter(prompt.definitionId).promptKey));
            if(Btn(420,610,510,65,SeaText("ui.sailing.accept")))ResolveSeaPrompt(prompt,true);
            if(Btn(990,610,510,65,SeaText("ui.sailing.decline")))ResolveSeaPrompt(prompt,false);
        }
    }
    void ResolveSeaPrompt(SeaEncounterState e,bool accept) {
        var key=g.ResolveSeaEncounter(e.instanceId,accept);
        Notify(SeaText(key,e.quantity>0?c.Product(e.productId).name:"",e.quantity,e.energyCost));
        Save(true);
    }
}}
