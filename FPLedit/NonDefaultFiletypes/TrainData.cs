using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.NonDefaultFiletypes
{
    internal class TrainData
    {
        public List<Station> Path;
        public Dictionary<Station, ArrDep> ArrDeps;

        public TrainData(Train train)
        {
            Path = train.GetPath();
            ArrDeps = train.GetArrDeps();
        }
    }
}
