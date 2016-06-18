using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.Shared
{
    [Serializable]
    public sealed class Train
    {
        public string Name { get; set; }

        public string Line { get; set; }

        public Dictionary<Station, DateTime> Arrivals { get; set; }

        public Dictionary<Station, DateTime> Departures { get; set; }

        public string Locomotive { get; set; }

        public bool Direction { get; set; }

        public bool[] Days { get; set; }

        public Train()
        {
            Arrivals = new Dictionary<Station, DateTime>();
            Departures = new Dictionary<Station, DateTime>();
            Days = new bool[7];
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
