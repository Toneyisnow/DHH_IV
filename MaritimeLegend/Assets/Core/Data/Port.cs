using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Port { public int id,region; public string name,trait,cityKey; public float x,y; public string[] raw; public double qualityBonus; public int productionLevelBonus; public double[] productionFeeFactors,productionDurationFactors; }
}
