using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MaritimeLegend {
public partial class MaritimeGame {
 readonly Dictionary<string,Texture2D> townUi=new Dictionary<string,Texture2D>();
 GUIStyle townCaption,townNumber,townPaper,townBrand;
 bool townFunctions;
 Texture2D TownTexture(string name){if(!townUi.TryGetValue(name,out var texture)){texture=Resources.Load<Texture2D>("TownUI/"+name);townUi.Add(name,texture);}return texture;}
 void TownStyles(){if(townCaption!=null)return;
  townCaption=new GUIStyle(text){fontSize=25,wordWrap=false,alignment=TextAnchor.MiddleLeft,normal={textColor=new Color(.85f,.81f,.69f)}};
  townNumber=new GUIStyle(townCaption){fontSize=27,alignment=TextAnchor.MiddleRight};
  townPaper=new GUIStyle(townCaption){normal={textColor=new Color(.24f,.19f,.12f)}};
  townBrand=new GUIStyle(heading){fontSize=42};
 }
 void TownArt(string name,Rect rect){LiveGui.DrawTexture(rect,TownTexture(name));}
 bool TownButton(Rect rect,string caption,string art="function_button",int size=24){
  var prior=LiveGui.color;bool hover=rect.Contains(LiveGui.Mouse)&&LiveGui.enabled;
  LiveGui.color=hover?new Color(1.15f,1.12f,1.04f):Color.white;TownArt(art,rect);LiveGui.color=prior;
  var background=LiveGui.backgroundColor;LiveGui.backgroundColor=Color.clear;
  var style=new GUIStyle(button){fontSize=size};bool clicked=LiveGui.Button(rect,caption,style);LiveGui.backgroundColor=background;
  if(clicked)sound.Cue(1);return clicked;
 }
 void TownLabel(float y,string caption){TownArt("label_bar",new Rect(1603,y,292,46));Label(1640,y+3,232,38,caption,townCaption);}
 void TownStat(float y,string caption,string value){
  Label(1610,y,105,42,caption,townCaption);TownArt("data_bar_black",new Rect(1710,y,181,42));Label(1724,y+3,145,34,value,townNumber);
 }
 internal void RefinedCity(){
  TownStyles();var view=currentCity.Diorama;
  LiveGui.DrawTexture(view.ScreenViewport,view.Image);
  // Transparent artwork preserves the live scene and its walking input.
  TownArt("town_frame",new Rect(0,0,1580,1015));
  TownArt("sidebar_background",new Rect(1580,0,340,1015));
  Label(62,58,400,60,"航海世家",townBrand);
  Label(66,116,400,36,"港湾 · "+c.ports[s.city].name,townCaption);
  foreach(var marker in view.Landmarks){
   if(Vector3.Distance(view.Player.position,marker.Value)>20)continue;
   var p=view.Project(marker.Value+Vector3.up*.15f);
   string destination=marker.Key=="码头"?"海图":marker.Key.StartsWith("市场")?"市场":marker.Key=="酒馆"?"伙伴":marker.Key;
   if(p.x>100&&p.x<1480&&p.y>185&&p.y<920&&TownButton(new Rect(p.x-58,p.y-20,116,40),marker.Key,"label_bar",21))view.GoTo(marker.Value,destination);
  }
  TownLabel(38,c.ports[s.city].name);
  TownLabel(99,"港口  /  "+view.WeatherName);
  TownStat(165,"声望",s.cities[s.city].reputation.ToString());
  TownStat(217,"工坊等级",s.cities[s.city].factories.Sum().ToString());
  TownLabel(272,s.cities[s.city].license?"产业许可 · 已签发":"产业许可 · 未签发");
  TownLabel(337,s.name);
  TownStat(396,"船体",(s.ships.Count==0?0:s.ships.Average(x=>x.hull)).ToString("0")+"%");
  TownStat(447,"船帆",(s.ships.Count==0?0:s.ships.Average(x=>x.sails)).ToString("0")+"%");
  TownStat(498,"舰船",s.ships.Count+" 艘");
  TownStat(549,"货舱",g.Inventory.Weight(s.cargo)+" / "+s.cargo.capacity);
  TownLabel(604,"持有金币");
  TownArt("data_bar_black_alt",new Rect(1610,656,280,44));Label(1628,660,240,34,s.gold.ToString("N0"),townNumber);
  Label(1614,713,266,37,"本港物产",townCaption);
  var local=c.ports[s.city].raw;
  for(int i=0;i<5;i++){
   float y=752+i*44;TownArt(i%2==0?"product_row":"product_row_alt",new Rect(1607,y,285,42));
   string item=i<local.Length?local[i]:"—";var product=c.products.FirstOrDefault(x=>x.id==item||x.name==item);
   Label(1628,y+3,240,34,product==null?item:product.name,townPaper);
  }
  string[] labels={"航海图","市场","市政厅","伙伴","产业园","船坞","功能"};
  string[] destinations={"海图","市场","市政厅","伙伴","产业园","船坞"};
  for(int i=0;i<labels.Length;i++)if(TownButton(new Rect(28+i*220,1018,213,52),labels[i])){
   if(i==6)townFunctions=!townFunctions;else{townFunctions=false;Change(destinations[i]);}
  }
  Label(1605,1022,290,44,"第 "+(1+(int)(s.worldHours/24))+" 日  ·  第 "+s.chapter+" 章",townCaption);
  if(townFunctions){
   var old=LiveGui.backgroundColor;LiveGui.backgroundColor=Color.clear;LiveGui.Button(new Rect(1200,470,350,510),"",button);LiveGui.backgroundColor=old;
   TownArt("sidebar_background",new Rect(1200,470,350,510));
   string[] extra={"仓库","自动航线","任务","详情","切换天气","暂停 / ESC"};
   for(int i=0;i<extra.Length;i++)if(TownButton(new Rect(1226,500+i*66,298,54),extra[i])){
    townFunctions=false;if(i==4)view.CycleWeather();else if(i==5)paused=true;else Change(extra[i]);
   }
  }
 }
}
}
