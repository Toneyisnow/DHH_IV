using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class CityState { public int id,reputation,weatherPreset; public bool license; public Inventory warehouse=new Inventory(); public int[] factories=new int[0]; public List<MarketEntry> market=new List<MarketEntry>(); }
}
