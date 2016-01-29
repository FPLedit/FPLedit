using Buchfahrplan.FileModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan
{
    public interface IExport
    {
        void Export(Timetable tt, string filename);

        string Filter { get; }
    }
}
