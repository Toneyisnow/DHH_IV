using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
public partial class GameService : IMarketService,IProductionService {
        public string License(){var city=S.cities[S.city];if(city.license)return "此城产业园已经开放";int foreign=S.cities.Count(x=>x.license)-1,cost=(300+foreign*200)*(HasPartner(9)?90:100)/100,rep=Math.Min(20,foreign*5);if(city.reputation<rep)return "本城声望需要 "+rep+"，可通过市场真实交易获得";if(S.gold<cost)return "金币不足，需要 "+cost;S.gold-=cost;city.license=true;Emit("license");CheckQuest();return "产业许可证已签发";}
        public string Depart(int destination){if(destination==S.city)return "目的港不能与当前港相同";if(S.ships[0].hull<10||S.ships[0].food<5)return "请先修理船只并补给";S.seaFrom=S.city;S.seaTo=destination;S.seaX=C.ports[S.city].x;S.seaY=C.ports[S.city].y;S.heading=Math.Atan2(C.ports[destination].y-S.seaY,C.ports[destination].x-S.seaX);S.atSea=true;S.screen="航海";return "解缆出航；A/D转舵、W/S调速，Q/E调帆，靠近目的港后入港";}
        public double Sail(double dt,double throttle){double windDot=Math.Cos(S.heading-S.wind),factor=Math.Max(.18,.6+.4*windDot)*(1-.2*Math.Abs(S.trim-.5));if(windDot<-.7)Emit("headwind");if(HasPartner(0)&&windDot<0)factor*=1.1;var ship=S.ships[0];double speed=.028*C.ships[ship.type].speed*factor*(.7+.3*ship.hull/100)*(1-.25*Inventory.Weight(S.cargo)/S.cargo.capacity)*throttle*S.settings.moveSpeed;S.seaX=Clamp(S.seaX+Math.Cos(S.heading)*speed*dt,.04,.96);S.seaY=Clamp(S.seaY+Math.Sin(S.heading)*speed*dt,.12,.89);return speed;}
        public double DistanceToPort(){var p=C.ports[S.seaTo];return Math.Sqrt(Math.Pow(p.x-S.seaX,2)+Math.Pow(p.y-S.seaY,2));}
        public string Arrive(){if(DistanceToPort()>.035)return "尚未进入目的港安全半径";string route=S.seaFrom+">"+S.seaTo;if(!S.routes.Contains(route)){S.routes.Add(route);Emit("route");Gain(0,25);}S.city=S.seaTo;S.atSea=false;S.screen="城市";if(!S.visited.Contains(S.city))S.visited.Add(S.city);if(!S.discovered.Contains(S.city))S.discovered.Add(S.city);foreach(var p in S.partners.Where(x=>x.active))Gain(Array.FindIndex(C.skills,x=>x.id==C.partners[p.id].skill1),5);CheckQuest();return "已抵达 "+C.ports[S.city].name;}
        public string Rescue(){S.finalVoyage=false;S.gold=Math.Max(0,S.gold-100);S.atSea=false;S.city=S.seaFrom;S.screen="城市";S.ships[0].hull=Math.Max(40,S.ships[0].hull);S.ships[0].food=30;return "港口救援队已接回船只，最多扣除100金币；货物保留";}
}}
