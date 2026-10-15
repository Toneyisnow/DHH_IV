using System.Linq;
using UnityEngine;
namespace MaritimeLegend {
public sealed class CityScene : FunctionalScene {
 public CityDefinition[] cities;public CityDiorama Diorama {get;private set;}
 public CityDefinition Current {get;private set;}
 public GameObject ContentRoot {get;private set;}
 public CityInteraction[] Interactions {get;private set;}=new CityInteraction[0];
 public void LoadCity(int cityId){if(Current!=null&&Current.cityId==cityId&&ContentRoot!=null)return;if(ContentRoot!=null){ContentRoot.SetActive(false);Destroy(ContentRoot);}Current=cities.First(x=>x.cityId==cityId);ContentRoot=Instantiate(Current.contentRoot,transform);ContentRoot.name=Current.cityKey;Interactions=ContentRoot.GetComponentsInChildren<CityInteraction>();Diorama=null;if(Current.refined){Diorama=ContentRoot.AddComponent<CityDiorama>();Diorama.Initialize(Current);}}
 public override void Render(MaritimeGame game){LoadCity(game.s.city);game.currentCity=this;if(Diorama!=null)Diorama.enabled=game.page=="城市";if(Diorama!=null)Diorama.View.enabled=game.page=="城市";switch(game.page){case "市场":game.MarketUI();break;case "市政厅":game.TownHall();break;case "伙伴":game.Partners();break;case "船坞":game.Shipyard();break;case "任务":game.Quests();break;case "详情":game.Details();break;case "结局":game.Ending();break;default:game.City();break;}}
}
}
