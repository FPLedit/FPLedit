#nullable enable
using System;

namespace FPLedit
{
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
    }
}