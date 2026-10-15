using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public interface IInventoryService { int Count(Inventory inventory,string product,int quality=-1); double Weight(Inventory inventory); }
}
