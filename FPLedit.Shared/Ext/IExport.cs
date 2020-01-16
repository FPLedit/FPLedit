using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    public interface IExport
    {
        bool Export(Timetable tt, string filename, IPluginInterface pluginInterface, string[] flags = null);

        string Filter { get; }
    }
}
