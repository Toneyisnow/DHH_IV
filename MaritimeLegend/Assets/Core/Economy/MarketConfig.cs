using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class MarketConfig { public double spread,referenceStock,maxStock,pressureStep,pressureMin,pressureMax,dailyReversion; public double[] qualityFactors; }
}
