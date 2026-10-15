using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public class SkillResolver : ISkillResolver {
        public int EffectiveLevel(GameState s,int city,int industry){int partner=s.partners.Where(p=>p.active&&(p.id==4||p.id==6)).Select(x=>x.trust).DefaultIfEmpty(0).Max();return Math.Max(1,Math.Min(20,s.cities[city].factories[industry]+s.skillLevels[7]+partner/2+(city==7?1:0)));}
    }
}
