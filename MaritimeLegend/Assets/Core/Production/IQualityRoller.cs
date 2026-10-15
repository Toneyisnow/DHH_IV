using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public interface IQualityRoller { int Roll(int worst,int level,double difficulty,ref uint seed); }
}
