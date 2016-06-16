using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.Shared
{
    public interface IExport
    {
        bool Export(Timetable tt, string filename, ILog logger);

        string Filter { get; }

        bool Reoppenable { get; }
    }
}
