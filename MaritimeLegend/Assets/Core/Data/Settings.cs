using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    [Serializable] public class Settings { public float master=.6f,music=.5f,sfx=.6f,brightness=1,textSpeed=1,moveSpeed=1,battleSpeed=1; public bool animations=true,fullscreen,productionPaused; }
}
