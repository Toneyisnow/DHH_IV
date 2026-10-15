using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public interface IMarketService { int Quote(int city,string product,int quality,bool buy); string Trade(int city,string product,int quality,int quantity,bool buy,Inventory inventory); }
}
