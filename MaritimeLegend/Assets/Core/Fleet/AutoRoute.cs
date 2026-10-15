using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class AutoRoute { public string id,shipId,product; public int from,to,quantity=5,minReserve,maxBuy=999,minSell=1,loops=1; public int spent,profit,budget=2000; public double progress; public string status="装货中",reason=""; public Inventory cargo=new Inventory(); }
}
