using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Batch { public string product; public int quality,quantity,origin; public Batch(){} public Batch(string p,int q,int n,int o){product=p;quality=q;quantity=n;origin=o;} }
}
