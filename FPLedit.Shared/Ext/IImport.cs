using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IImport
    {
        Timetable Import(string filename, IPluginInterface pluginInterface, ILog replaceLog = null);

        string Filter { get; }
    }
}
