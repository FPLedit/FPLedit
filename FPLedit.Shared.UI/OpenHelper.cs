using System.Diagnostics;

namespace FPLedit.Shared.UI;

/// <summary>
/// Helper class for opening links/files with Process.Start, discarding all exceptions.
/// </summary>
/// <seealso cref="FPLedit.Shared.IUiPluginInterface.OpenUrl"/>
public static class OpenHelper
{
    public static void Open(string url)
    {
        var proc = OpenProc(url);
        proc?.Dispose();
    }

    public static Process? OpenProc(string url)
    {
        try
        {
            return Process.Start(new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = url,
            });
        }
        catch
        {
            return null;
        }
    }
}