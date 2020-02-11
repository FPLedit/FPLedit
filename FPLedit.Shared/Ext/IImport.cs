using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IImport
    {
        Timetable Import(Stream stream, IPluginInterface pluginInterface, ILog replaceLog = null);

        string Filter { get; }
    }
    
    public static class ImportExt
    {
        public static Timetable Import(this IImport imp, string filename, IPluginInterface pluginInterface, ILog replaceLog = null)
        {
            using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read))
                return imp.Import(stream, pluginInterface, replaceLog);
        }
    }
}
