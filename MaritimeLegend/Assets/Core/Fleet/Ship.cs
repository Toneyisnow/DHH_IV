using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Ship { public string id,ownerId; public int type,level; public double hull,sails,food; public int sailingVersion,crew; public double water; public int ammo; public bool busy; }
}
