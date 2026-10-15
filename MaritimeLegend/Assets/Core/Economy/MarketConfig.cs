using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class MarketConfig { public double spread,referenceStock,maxStock,pressureStep,pressureMin,pressureMax,dailyReversion; public double[] qualityFactors; public int initialRawLevel5Stock,initialRawLevel4MinPercent,initialRawLevel4MaxPercent; }
}
