using System;
using System.IO;
using System.Linq;
using UnityEngine;
using MaritimeLegend.Domain;
using GUI=MaritimeLegend.LiveGui;
using GUIUtility=MaritimeLegend.LiveGuiUtility;
namespace MaritimeLegend {
public partial class MaritimeGame {
internal void SailingView(){
 Panel(new Rect(30,130,1405,755));if(map)GUI.DrawTexture(new Rect(40,140,1385,735),map,ScaleMode.ScaleAndCrop);Fill(new Rect(40,140,1385,735),new Color(.015f,.12f,.16f,.6f));Label(65,148,1260,60,"航海 · 风与洋流",heading);
 foreach(var p in c.ports){var v=MapPoint(p.x,p.y);if(Btn(v.x-55,v.y+16,160,45,p.name))selectedPort=p.id;}
 DrawShip(MapPoint(s.seaX,s.seaY),(float)s.heading*Mathf.Rad2Deg,gold);
 Label(65,755,1290,85,"A/D 转舵 · W/S 调速 · Q/E 调帆\n风向 "+(s.wind*Mathf.Rad2Deg).ToString("0")+"° · 目的港 "+c.ports[s.seaTo].name,small);
 Panel(new Rect(1460,130,430,755));var sh=s.ships[0];Label(1490,160,365,55,"航行状态",heading);Label(1490,300,360,180,"船体 "+sh.hull.ToString("0")+"\n食水 "+sh.food.ToString("0")+"\n目的港距离 "+g.DistanceToPort().ToString("0.000"));
 if(Btn(1490,620,365,65,"驶入目的港",g.DistanceToPort()<=.035)){Notify(g.Arrive());page="城市";if(s.finalVoyage&&s.city==7){if(s.mode=="经典")StartBattle(true);else if(sh.food>=5&&sh.hull>=10){g.Finish();Change("结局");Save(false);sound.Cue(9);}else Notify("远航物资不足，请整备后重新发起挑战");}}
 if(Btn(1490,710,365,60,"紧急返港救援")){Notify(g.Rescue(),10);page="城市";}
}
}}
