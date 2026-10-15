using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class MarketEntry { public string product; public int stock=80; public double pressure; public int bought,sold; }
}
