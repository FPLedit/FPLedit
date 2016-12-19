using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Station : Entity
    {
        public string Name
        {
            get
            {
                return GetAttribute<string>("name", "");
            }
            set
            {
                SetAttribute("name", value);
            }
        }

        public float Kilometre
        {
            get
            {
                return float.Parse(GetAttribute("km", "0.0"), CultureInfo.InvariantCulture);
            }
            set
            {
                SetAttribute("km", value.ToString("0.0", CultureInfo.InvariantCulture));
            }
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return Name + " [" + Kilometre + "]";
        }
    }
}
