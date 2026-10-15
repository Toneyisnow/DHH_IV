using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Ship { public string id,ownerId; public int type,level=1; public double hull=100,sails=100,food=100; public int ammo=40; public bool busy; }
}
