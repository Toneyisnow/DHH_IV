using System;
using System.Collections.Generic;
using System.Linq;
namespace MaritimeLegend.Domain {
    public interface IClock { DateTime UtcNow {get;} double MonotonicSeconds {get;} }
}
