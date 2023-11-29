using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace FPLedit;

internal sealed class PathManager
{
    private static PathManager? instance;
    private string? appDirectory, settingsDirectory;

    public static PathManager Instance
    {
        get
        {
            instance ??= new PathManager();
            return instance;
        }
    }

    private PathManager() {}

    public string AppDirectory
    {
        get
        {
            if (appDirectory == null)
                throw new InvalidOperationException(nameof(AppDirectory) + " is not set!");
            return appDirectory;
        }
        set => appDirectory = value ?? throw new ArgumentNullException(nameof(value), "Tried to set " + nameof(AppDirectory) + " to null!");
    }

    public string SettingsDirectory
    {
        get => settingsDirectory ?? AppDirectory;
        set => settingsDirectory = value ?? throw new ArgumentNullException(nameof(value), "Tried to set " + nameof(SettingsDirectory) + " to null!");
    }

    private static string? tmpDirPath = null;
    public static string CreateTempDir()
    {
        if (tmpDirPath != null) return tmpDirPath;

        var tmpDir = "fpledit";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Sanitize the username, that it does not contain filename-special characters.
            var safeUserName = string.Join("", Environment.UserName.Where(char.IsLetterOrDigit));
            if (safeUserName == "")
                throw new InvalidOperationException(nameof(safeUserName) + " is empty!");
            tmpDir += '_' + safeUserName;
        }

        tmpDirPath = Path.Combine(Path.GetTempPath(), tmpDir);
        Directory.CreateDirectory(tmpDirPath);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (NativeMethods.chmod(tmpDirPath, 0b111_000_000) != 0) // try to set mode u=rwx
            {
                // (probably) another user created a directory with the same name.
                var errno = Marshal.GetLastPInvokeError();
                throw new InvalidOperationException($"Changing the file permissions of the temp directory failed with errno={errno}!");
            }
        }
        return tmpDirPath;
    }

    private static class NativeMethods
    {
        [DllImport("libc", SetLastError = true, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, CallingConvention = CallingConvention.Cdecl)]
        public static extern int chmod([MarshalAs(UnmanagedType.LPUTF8Str)] string pathname, int mode);
    }
}