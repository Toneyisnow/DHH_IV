using UnityEngine;
namespace MaritimeLegend {
public sealed class WorldMapScene : FunctionalScene {
 readonly MapSurface surface=new MapSurface();int selected;
 public override void Render(MaritimeGame game){selected=surface.Draw(game,selected,"世界地图 · 港口、海域与探索");var port=game.c.ports[selected];game.Panel(new Rect(1460,130,430,755));game.Label(1490,160,365,60,port.name);game.Label(1490,250,355,230,port.trait+"\n\n海域 "+port.region+"\n探索状态："+(game.s.discovered.Contains(selected)?"已探明":"未知")+"\n本城许可："+(game.s.cities[selected].license?"开放":"未开放"));
 if(game.Btn(1490,550,365,65,"亲自出航至此港",selected!=game.s.city)){game.Notify(game.g.Depart(selected));if(game.s.atSea)game.Change("航海");}
 if(game.Btn(1490,650,365,65,"自动航线调度")){game.selectedPort=selected;game.Change("自动航线");}
 if(game.Btn(1490,750,365,65,"返回当前城市"))game.Change("城市");
 }
}
}
