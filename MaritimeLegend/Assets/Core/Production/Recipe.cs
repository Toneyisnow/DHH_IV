using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Recipe { public string id,output,skill,unlock; public int quantity,industry,minLevel,fee; public float foregroundSeconds,awayCityMinutes,offlineHours,difficulty; public bool consumesTools; public Ingredient[] inputs; }
}
