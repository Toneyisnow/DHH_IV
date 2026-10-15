using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Product { public string id,name,category; public int industry,basePrice,weight,stackSize,quality; public string[] demandTags; public int[] allowedCities; }
}
