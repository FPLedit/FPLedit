using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.FileModel
{
    [Serializable]
    public sealed class Station
    {
        public string Name { get; set; }

        public float Kilometre { get; set; }

        public int MaxVelocity { get; set; }
    }
}
