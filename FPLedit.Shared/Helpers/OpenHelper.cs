using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FPLedit.Shared.Helpers
{
    /// <summary>
    /// Helper class to shim Process.Start on corefx systems, mainly for opening file and web addresses.
    /// </summary>
    public static class OpenHelper
    {
        public static Process Open(string url)
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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Process.Start(new ProcessStartInfo("cmd", $"/c start {url.Replace("&", "^&")}") { CreateNoWindow = true });
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return Process.Start("xdg-open", url);
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return Process.Start("open", url);
            }
            catch
            {
                var p = TryStartShellExecute(url);
                if (p != null)
                    return p;
            }

            return null;
        }

        private static Process TryStartShellExecute(string url)
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