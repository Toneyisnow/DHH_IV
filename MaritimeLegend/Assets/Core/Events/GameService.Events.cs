using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
public partial class GameService : IMarketService,IProductionService {
        public void Log(string text){S.journal.Insert(0,text);if(S.journal.Count>120)S.journal.RemoveAt(S.journal.Count-1);}
        public void Emit(string trigger){foreach(var e in C.events.Where(x=>x.trigger==trigger).OrderByDescending(x=>x.priority)){if(S.eventFlags.Contains(e.id)||!e.prerequisites.All(S.eventFlags.Contains))continue;S.eventFlags.Add(e.id);S.pendingEvents.Add(e.id);foreach(var effect in e.effects)if(effect.type=="gold")S.gold+=effect.value;Log(e.title+"："+e.dialogue);}}
        public string ChooseEvent(bool support){if(S.pendingEvents.Count==0)return "没有待处理事件";var id=S.pendingEvents[0];if(support){if(S.gold<20)return "金币不足，需要20金币支援港口";S.gold-=20;S.cities[S.city].reputation+=5;Gain(5,5);Log("选择支援港口：20金币换得5声望。");}S.pendingEvents.RemoveAt(0);return support?"已支援港口，声望+5":"已记录事件";}
        public void CheckQuest(){if(S.chapter==1&&S.trades>0&&S.produced>0&&S.routes.Count>0){S.chapter=2;S.gold+=900;Log("第二章：诸港的契约。商会授予900金币发展金。");}if(S.chapter==2&&S.cities.Count(x=>x.license)>=3&&S.partners.Any(x=>x.active)&&S.produced>=3){S.chapter=3;S.gold+=3000;Log("第三章：世界之环。诸港联合提供3000金币远航资金。");}}
        public string SubmitMaterials(){if(S.materialsSubmitted)return "材料已经提交，无须重复交付";if(S.chapter<3)return "第三章开放此挑战";var wood=C.Named("木板").id;var cloth=C.Named("布匹").id;var a=S.cargo.batches.FirstOrDefault(x=>x.product==wood&&x.quality<=2);var b=S.cargo.batches.FirstOrDefault(x=>x.product==cloth&&x.quality<=2&&a!=null&&x.origin!=a.origin);if(a==null||b==null)return "船舱需要来自不同港口的品质2以上木板、布匹各1（可在不同港制造）";a.quantity--;b.quantity--;S.cargo.batches.RemoveAll(x=>x.quantity==0);S.materialsSubmitted=true;return "两港高品质材料已登记并交付";}
        public string BeginFinal(){if(S.ended)return "传奇航路已经建立，可继续自由经营";if(S.chapter<3||S.routes.Count<3||!S.materialsSubmitted||!S.instrumentMade||!S.sailsMade||S.ships[0].type==0)return "尚未满足最终挑战条件，查看任务面板";if(!S.partners.Any(x=>x.active&&x.trust>=2))return "需要一位信赖2级的当班伙伴";if(S.city==7)return "请从杭州以外的港口启程";S.finalVoyage=true;Emit("final");return Depart(7);}
        public void Finish(){if(S.ended)return;S.ended=true;S.finalVoyage=false;S.finalStage=4;S.gold+=5000;Log("传奇航路建立。你的家族把诸港连接成一张共同的海图。所有伙伴的远航故事仍将继续。");}
}}
