using System;
using UnityEngine;
namespace MaritimeLegend {
public sealed class AutoSailingScene : FunctionalScene {
 readonly MapSurface surface=new MapSurface(); int product; Vector2 scroll;
 public override void Render(MaritimeGame game){if(game.page=="航线配置"){game.AutoConfig();return;}
 game.selectedPort=surface.Draw(game,game.selectedPort,"自动航线 · 舰队、供需与运输调度");
 foreach(var route in game.s.autoRoutes){var a=game.c.ports[route.from];var b=game.c.ports[route.to];float t=(float)(route.status=="返航中"?1-route.progress:route.progress);var pos=Vector2.Lerp(surface.Point(a.x,a.y),surface.Point(b.x,b.y),t);game.Label(pos.x,pos.y-35,220,45,(route.status=="暂停"?"! ":"▸ ")+route.status);}
 game.Panel(new Rect(1460,130,430,755));int city=game.selectedPort;var p=game.c.products[product];game.Label(1490,150,360,50,game.c.ports[city].name+" · 市场快照");if(game.Btn(1490,215,365,50,"商品："+p.name))product=(product+1)%game.c.products.Length;
 var entry=game.g.Market(city,p.id);game.Label(1490,285,365,145,"品质"+game.c.initial.route.quality+"供给 "+entry.Stock(game.c.initial.route.quality)+" / 压力 "+entry.pressure.ToString("0.00")+"\n买价 "+game.g.Quote(city,p.id,game.c.initial.route.quality,true)+" / 卖价 "+game.g.Quote(city,p.id,game.c.initial.route.quality,false)+"\n本城仓库 "+game.g.Inventory.Weight(game.s.cities[city].warehouse));
 if(game.Btn(1490,435,365,55,"配置自动运输"))game.Change("航线配置");
 scroll=LiveGui.BeginScrollView(new Rect(1480,510,385,325),scroll,new Rect(0,0,360,Math.Max(325,game.s.autoRoutes.Count*160)));
 int i=0;foreach(var a in game.s.autoRoutes.ToArray()){float y=i++*160;game.Label(0,y,355,75,game.c.ports[a.from].name+" → "+game.c.ports[a.to].name+"\n"+a.status+" · 盈亏 "+a.profit+"\n"+a.reason);if(game.Btn(0,y+80,165,48,a.status=="暂停"?"继续":"暂停")){if(a.status=="暂停")game.g.ResumeAuto(a);else game.g.StopAuto(a);}if(game.Btn(180,y+80,165,48,"解除并卸货"))game.Notify(game.g.DeleteAuto(a));}LiveGui.EndScrollView();
 }
}
}
