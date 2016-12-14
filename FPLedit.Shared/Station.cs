using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Station : Meta
    {
        public string Name { get; set; }

        public float Kilometre { get; set; }

        public Station()
        {
            Name = "";
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return ToString(true);
        }

        [DebuggerStepThrough]
        public string ToString(bool includeKilometre)
        {
            if (includeKilometre)
                return Name + " [" + Kilometre + "]";
            else
                return Name;
        }
    }
}
