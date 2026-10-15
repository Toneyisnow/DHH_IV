using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Inventory { public List<Batch> batches=new List<Batch>(); public int capacity=200; }
}
