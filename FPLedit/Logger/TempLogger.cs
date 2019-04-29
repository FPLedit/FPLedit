using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Logger
{
    public class TempLogger : ILog
    {
        private readonly string filename;

        public TempLogger(IInfo info)
        {
            filename = info.GetTemp("fpledit_log.txt");

            var fi = new FileInfo(filename);
            if (fi.Exists && fi.Length > 10240) // > 10KB
                fi.Delete();

            Info("FPLedit Programmstart");
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
            using (StreamWriter r = new StreamWriter(filename, true))
                r.WriteLine(DateTime.Now.ToString() + ": [" + type + "] " + message);
        }
    }
}
