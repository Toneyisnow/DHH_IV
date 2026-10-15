using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class EventDef { public string id,trigger,oneShotOrRepeat,dialogue,title,nextEvent; public int priority,cooldown; public string[] prerequisites,choices,conditions; public EventEffect[] effects; }
}
