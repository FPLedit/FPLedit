using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FPLedit.Shared.UI
{
    /// <summary>
    /// Helper class to shim Process.Start on corefx systems, mainly for opening file and web addresses.
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var p = TryStartShellExecute(url);
                if (p != null)
                    return p;
            }

            try
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                var escapedUrl = Uri.EscapeUriString(url);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return Process.Start("xdg-open", escapedUrl);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return Process.Start("open", escapedUrl);
            }
            catch
            {
                var p = TryStartShellExecute(url);
                if (p != null)
                    return p;
            }

            return null;
        }

        private static Process? TryStartShellExecute(string url)
        {
            try
            {
                return Process.Start(new ProcessStartInfo()
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
}