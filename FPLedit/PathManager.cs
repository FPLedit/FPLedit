using System;
using System.IO;

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

        public string AppFilePath
        {
            get
            {
                if (appFilePath == null)
                    throw new InvalidOperationException("AppFilePath is not set!");
                return appFilePath;
            }
            set => appFilePath = value ?? throw new InvalidOperationException("Tried to set AppFilePath to null!");
        }

        public string AppDirectory => Path.GetDirectoryName(AppFilePath);
    }
}