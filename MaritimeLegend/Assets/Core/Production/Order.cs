using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Order { public string id,ownerId,recipe; public int city,quality,effectiveLevel; public double progress; public bool completed,claimed,cancelled,repeat,autoSell; public List<Batch> locked=new List<Batch>(); }
}
