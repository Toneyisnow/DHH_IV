using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public interface ISkillResolver { int EffectiveLevel(GameState state,int city,int industry); }
}
