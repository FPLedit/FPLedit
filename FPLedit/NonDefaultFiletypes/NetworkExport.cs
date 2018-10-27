using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.NonDefaultFiletypes
{
    public class NetworkExport : IExport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public bool Export(Timetable tt, string filename, IInfo info)
        {
            if (tt.Type == TimetableType.Network)
                throw new Exception("Der Fahrplan ist bereits ein Netzwerk-Fahrplan");

            var clone = tt.Clone();
            var ntt = new Timetable(TimetableType.Network);

            foreach (var attr in clone.Attributes)
                if (ntt.GetAttribute<string>(attr.Key) == null)
                    ntt.SetAttribute(attr.Key, attr.Value);

            var rt = Timetable.LINEAR_ROUTE_ID.ToString();
            var y = 0;
            foreach (var sta in clone.Stations)
            {
                var km_old = sta.GetAttribute("km", "");
                sta.SetAttribute("km", rt + ":" + km_old);
                sta.SetAttribute("fpl-rt", rt);
                sta.SetAttribute("fpl-pos", (y += 40).ToString() + ";0");
                ntt.AddStation(sta, 0);
            }

            foreach (var orig in clone.Trains)
            {
                var ntra = new Train(TrainDirection.tr, ntt);

                foreach (var attr in orig.Attributes)
                    if (ntra.GetAttribute<string>(attr.Key) == null)
                        ntra.SetAttribute(attr.Key, attr.Value);

                var path = orig.GetPath();
                ntra.AddAllArrDeps(path);

                foreach (var sta in path)
                {
                    var ardp = orig.GetArrDep(sta);
                    ntra.SetArrDep(sta, ardp);
                }

                ntt.AddTrain(ntra);
            }

            return new XMLExport().Export(ntt, filename, info);
        }
    }
}
