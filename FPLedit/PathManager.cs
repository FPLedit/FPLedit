using System;

namespace FPLedit
{
    internal sealed class PathManager
    {
        private static PathManager instance;
        private string appFilePath;

        public static PathManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new PathManager();
                return instance;
            }
        }

        private PathManager()
        {
        }

        public string AppDirectory
        {
            get
            {
                if (appFilePath == null)
                    throw new InvalidOperationException(nameof(AppDirectory) + " is not set!");
                return appFilePath;
            }
            set => appFilePath = value ?? throw new ArgumentNullException(nameof(value), "Tried to set " + nameof(AppDirectory) + " to null!");
        }
    }
}