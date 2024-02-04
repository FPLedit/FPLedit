using System;
using System.IO;
using System.Threading.Tasks;
using FPLedit.Shared.DefaultImplementations;

namespace FPLedit.Shared;

/// <summary>
/// Basic interface to provide exporter capabalities. Also a registrable component.
/// </summary>
/// <remarks>See <see cref="DefaultTemplateExport"/> for a default implementation.</remarks>
public interface IExport : IRegistrableComponent
{
    /// <summary>
    /// Invokes the exporter.
    /// </summary>
    /// <param name="tt">A readonly copy of the current timetable.</param>
    /// <param name="stream"></param>
    /// <param name="pluginInterface">A reduced PluginInterface that provides limited core features from FPledit.</param>
    /// <param name="flags">Exporter flags.</param>
    /// <returns>If the operation was successful.</returns>
    /// <remarks>This method must be thread-safe and MUST NOT call into UI directly, as it might be called on a non-UI thread.</remarks>
    bool Export(ITimetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null);

    /// <summary>
    /// Filetype filter of the form "description|pattern", e.g. "Description (*.ext)|*.ext"
    /// </summary>
    /// <remarks>Must always return the same value.</remarks>
    string Filter { get; }


    /// <summary>
    /// This function provides a safe way to execute any exporter to write to a file directly.
    /// </summary>
    /// <param name="tt">A readonly copy of the current timetable.</param>
    /// <param name="filename"></param>
    /// <param name="pluginInterface">A reduced PluginInterface that provides limited core features from FPledit.</param>
    /// <param name="flags">Exporter flags.</param>
    /// <returns>If the operation was successful.</returns>
    public bool SafeExport(ITimetable tt, string filename, IReducedPluginInterface pluginInterface, string[]? flags = null)
    {
        try
        {
            using var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write);
            stream.SetLength(0);
            return Export(tt, stream, pluginInterface, flags);
        }
        catch (Exception ex)
        {
            pluginInterface.Logger.Error(GetType().Name + ": " + ex.Message);
            pluginInterface.Logger.LogException(ex);
            return false;
        }
    }

    /// <summary>
    /// This function provides a safe way to async-execute any exporter to write to a file directly.
    /// </summary>
    /// <param name="tt">A readonly copy of the current timetable.</param>
    /// <param name="filename"></param>
    /// <param name="pluginInterface">A reduced PluginInterface that provides limited core features from FPledit.</param>
    /// <param name="flags">Exporter flags.</param>
    /// <returns>A Task that has not been started yet, which can be used to execute the exporter.</returns>
    public Task<bool> GetAsyncSafeExport(ITimetable tt, string filename, IReducedPluginInterface pluginInterface, string[]? flags = null)
        => new Task<bool>(() => SafeExport(tt, filename, pluginInterface, flags));
}