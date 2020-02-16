using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IImport
    {
        Timetable Import(Stream stream, IReducedPluginInterface pluginInterface, ILog replaceLog = null);

        string Filter { get; }
    }
    
    public static class ImportExt
    {
        public static Timetable SafeImport(this IImport imp, string filename, IReducedPluginInterface pluginInterface, ILog replaceLog = null)
        {
            try
            {
                using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Read))
                    return imp.Import(stream, pluginInterface, replaceLog);
            }
            catch (Exception ex)
            {
                var log = replaceLog ?? pluginInterface.Logger;
                log.Error(imp.GetType().Name + ": " + ex.Message);
                log.LogException(ex);
                return null;
            }
        }
    }
}
