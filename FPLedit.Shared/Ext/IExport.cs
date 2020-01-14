using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    public interface IExport
    {
        bool Export(Timetable tt, string filename, IPluginInterface pluginInterface);

        string Filter { get; }
    }
}
