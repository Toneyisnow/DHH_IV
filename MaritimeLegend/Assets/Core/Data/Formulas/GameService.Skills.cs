using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
public partial class GameService : IMarketService,IProductionService {
        public bool HasPartner(int id){return S.partners.Any(x=>x.id==id&&x.active);}
        public void Gain(int skill,int amount){if(S.skillLevels[skill]<1)S.skillLevels[skill]=1;S.skillXp[skill]+=amount;while(S.skillLevels[skill]<C.skills[skill].maxLevel&&S.skillXp[skill]>=S.skillLevels[skill]*C.balance.skills.experiencePerLevel){S.skillXp[skill]-=S.skillLevels[skill]*C.balance.skills.experiencePerLevel;S.skillLevels[skill]++;}}
        public string Recruit(int id){var p=C.partners[id];if(p.port!=S.city)return "伙伴不在本港";if(S.partners.Any(x=>x.id==id))return "伙伴已加入";if(S.gold<p.fee)return "招募金币不足";S.gold-=p.fee;var state=StateFactory.Partner(C);state.id=id;state.active=S.partners.Count(x=>x.active)<C.ships[S.ships[0].type].slots;S.partners.Add(state);Emit("partner");CheckQuest();return p.name+" 已加入，受岗位限制的伙伴可在名单中换班";}
        public string TogglePartner(int id){var p=S.partners.First(x=>x.id==id);if(!p.active&&S.partners.Count(x=>x.active)>=C.ships[S.ships[0].type].slots)return "岗位已满，请先将另一位伙伴设为待命";p.active=!p.active;return p.active?"已编入当班":"已设为待命";}
        public string TrainPartner(int id){var p=S.partners.First(x=>x.id==id);if(p.trust>=C.balance.skills.maxTrust)return "已达到传奇专长";if(p.trust==C.balance.skills.legendaryPreviousTrust&&S.chapter<C.balance.skills.legendaryChapter)return "传奇专长需要第三章";int fee=C.balance.skills.trainingPricePerTrust*p.trust;if(S.gold<fee)return "金币不足";S.gold-=fee;p.trust++;return "通过航海研修，信赖提升至 "+p.trust;}
}}
