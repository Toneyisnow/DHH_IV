using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public interface IRecipeCatalog { Recipe Recipe(string id); }
}
