using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class AutoRoute { public int quality=SaveContract.LegacyMarketQuality; public string id,shipId,product; public int from,to,quantity,minReserve,maxBuy,minSell,loops; public int spent,profit,budget; public double progress; public string status="",reason=""; public Inventory cargo=new Inventory(); }
}
