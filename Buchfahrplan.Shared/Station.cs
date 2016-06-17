using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.Shared
{
    [Serializable]
    public sealed class Station
    {
        public string Name { get; set; }

        public float Kilometre { get; set; }

        public int MaxVelocity { get; set; }

        public override string ToString()
        {
            return Name + " (" + Kilometre + ")";
        }
    }
}
