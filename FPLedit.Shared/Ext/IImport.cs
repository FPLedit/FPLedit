using System;
using System.IO;

namespace FPLedit.Shared
{
    /// <summary>
    /// Basic interface to provide importer capabalities.
    /// </summary>
    public interface IImport : IRegistrableComponent
    {
        /// <summary>
        /// Invokes the importer.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="pluginInterface">A reduced PluginInterface that provides limited core features from FPledit.</param>
        /// <param name="replaceLog">The importer should log to this logger if it is not null.</param>
        /// <returns>The newly imported Timetable instance, otherwise null.</returns>
        /// <remarks>This method must be thread-safe and MUST NOT call into UI directly, as it might be called on a non-UI thread.</remarks>
        Timetable? Import(Stream stream, IReducedPluginInterface pluginInterface, ILog? replaceLog = null);

        /// <summary>
        /// Filetype filter of the form "description|pattern", e.g. "Description (*.ext)|*.ext"
        /// </summary>
        /// <remarks>Must always return the same value.</remarks>
        string Filter { get; }
    }
    
    public static class ImportExt
    {
        public static Timetable? SafeImport(this IImport imp, string filename, IReducedPluginInterface pluginInterface, ILog? replaceLog = null)
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
