using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Serializable]
    public struct ArrDep
    {
        public TimeSpan Arrival { get; set; }

        public TimeSpan Departure { get; set; }
    }
}
