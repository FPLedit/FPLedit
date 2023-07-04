using FPLedit.Shared;
using System;
using System.IO;

namespace FPLedit.Logger
{
    public sealed class TempLogger : ILog
    {
        public bool CanAttach => false;

        public void AttachLogger(ILog other)
        {
        }
        
        private readonly string filename;

        public TempLogger(IPluginInterface pluginInterface)
        {
            filename = pluginInterface.GetTemp("fpledit_log.txt");

            var fi = new FileInfo(filename);
            if (fi.Exists && fi.Length > 10240) // > 10KB
                fi.Delete();

            Info("Started FPLedit");
        }

        public void Error(string message)
            => Write(message, "EROR");

        public void Info(string message)
            => Write(message, "INFO");

        public void LogException(Exception e)
            => Write(e.GetExceptionDetails(), "EXCP");

        public void Warning(string message)
            => Write(message, "WARN");

        public void Debug(string message)
            => Write(message, "DEBUG");

        private void Write(string message, string type)
        {
            using var r = new StreamWriter(filename, true);
            r.WriteLine(DateTime.Now.ToString("s") + ": [" + type + "] " + message);
        }
    }
}
