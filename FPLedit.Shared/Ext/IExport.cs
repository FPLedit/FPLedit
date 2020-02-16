using System;
using System.IO;

namespace FPLedit.Shared
{
    public interface IExport
    {
        bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[] flags = null);

        string Filter { get; }
    }

    public static class ExportExt
    {
        public static bool SafeExport(this IExport exp, Timetable tt, string filename, IReducedPluginInterface pluginInterface, string[] flags = null)
        {
            try
            {
                using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    stream.SetLength(0);
                    return exp.Export(tt, stream, pluginInterface, flags);
                }
            }
            catch (Exception ex)
            {
                pluginInterface.Logger.Error( exp.GetType().Name + ": " + ex.Message);
                pluginInterface.Logger.LogException(ex);
                return false;
            }
        }
    }
}
