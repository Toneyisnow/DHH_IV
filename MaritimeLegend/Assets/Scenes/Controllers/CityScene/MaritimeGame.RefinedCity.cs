using UnityEngine;
namespace MaritimeLegend {public partial class MaritimeGame {
 internal void RefinedCity(){var view=currentCity.Diorama;LiveGui.DrawTexture(view.ScreenViewport,view.Image);foreach(var marker in view.Landmarks){var p=view.Project(marker.Value+Vector3.up*.15f);string destination=marker.Key=="码头"?"海图":marker.Key;if(Btn(p.x-45,p.y-18,90,35,marker.Key))view.GoTo(marker.Value,destination);}
 Panel(new Rect(1510,130,380,755));Label(1540,160,320,70,c.ports[s.city].name,heading);Label(1540,250,315,100,"港湾 · 微缩场景\n"+view.WeatherName);Label(1540,375,315,170,"本城声望 "+s.cities[s.city].reputation+"\n产业许可："+(s.cities[s.city].license?"开放":"未签发")+"\n\n点击道路行走，点击地点自动走近。",small);
 if(Btn(1540,575,315,58,"切换天气"))view.CycleWeather();Label(1540,665,315,140,view.Status,small);
 if(CityArtPreviewRuntime.Active){if(Btn(1540,790,150,50,"里斯本"))s.city=0;if(Btn(1700,790,150,50,"威尼斯"))s.city=2;}
 }
}}
