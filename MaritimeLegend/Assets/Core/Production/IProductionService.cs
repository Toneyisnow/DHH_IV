using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public interface IProductionService { string Start(string recipe,int city); void Advance(double seconds,bool offline); string Claim(string orderId); }
}
