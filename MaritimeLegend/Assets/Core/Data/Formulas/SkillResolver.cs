using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public class SkillResolver : ISkillResolver {
        // Every continuous skill effect uses the same clamped 0..maxSkillPoint range.
        public static double Linear(int skillPoint,int maxSkillPoint,double minValue,double maxValue) {
            if(maxSkillPoint<=0)throw new ArgumentOutOfRangeException(nameof(maxSkillPoint));
            return minValue+(maxValue-minValue)*Math.Max(0,Math.Min(maxSkillPoint,skillPoint))/(double)maxSkillPoint;
        }
        readonly Catalog C; public SkillResolver(Catalog catalog){C=catalog;}
        public int EffectiveLevel(GameState s,int city,int industry){int partner=s.partners.Where(p=>p.active&&(p.id==C.balance.partnerRoles.craftsman||p.id==C.balance.partnerRoles.chemist)).Select(x=>x.trust).DefaultIfEmpty(0).Max();return Math.Max(C.balance.production.minimumEffectiveLevel,Math.Min(C.balance.production.maxEffectiveLevel,s.cities[city].factories[industry]+s.skillLevels[C.balance.skills.productionSkill]+partner/C.balance.production.partnerTrustDivisor+C.ports[city].productionLevelBonus));}
    }
}
