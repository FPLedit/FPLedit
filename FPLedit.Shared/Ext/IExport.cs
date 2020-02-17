using System;
using System.IO;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    /// <summary>
    /// Basic interface to provide exporter capabalities.
    /// </summary>
    /// <remarks>See <see cref="FPLedit.Shared.DefaultImplementations.BasicTemplateExport"/> for a default implementation.</remarks>
    public interface IExport
    {
        /// <summary>
        /// Invokes the exporter.
        /// </summary>
        /// <param name="tt">A readonly copy of the current timetable.</param>
        /// <param name="stream"></param>
        /// <param name="pluginInterface">A reduced PluginInterface that provides limited core features from FPledit.</param>
        /// <param name="flags">Exporter flags.</param>
        /// <returns></returns>
        /// <remarks>This method must be thread-safe and MUST NOT call into UI directly, as it might be called on a non-UI thread.</remarks>
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

        public static Task<bool> GetAsyncSafeExport(this IExport exp, Timetable tt, string filename, IReducedPluginInterface pluginInterface, string[] flags = null) 
            => new Task<bool>(() => exp.SafeExport(tt, filename, pluginInterface, flags));
    }
}
