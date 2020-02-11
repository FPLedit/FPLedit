using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    public interface IExport
    {
        bool Export(Timetable tt, Stream stream, IPluginInterface pluginInterface, string[] flags = null);

        string Filter { get; }
    }

    public static class ExportExt
    {
        public static bool Export(this IExport exp, Timetable tt, string filename, IPluginInterface pluginInterface, string[] flags = null)
        {
            using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write))
            {
                stream.SetLength(0);
                return exp.Export(tt, stream, pluginInterface, flags);
            }
        }
    }
}
