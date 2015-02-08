using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.FileModel
{
    [Serializable]
    public class Train
    {
        public string Name { get; set; }

        public string Line { get; set; }

        public Dictionary<Station, DateTime> Arrivals { get; set; }

        public Dictionary<Station, DateTime> Departures { get; set; }

        public string Locomotive { get; set; }

        public bool Negative { get; set; }
    }
}
