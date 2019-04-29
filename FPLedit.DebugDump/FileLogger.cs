using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.DebugDump
{
    public class FileLogger : ILog
    {
        private readonly string filename;

        public FileLogger(string fn)
        {
            filename = fn;

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
            try
            {
                using (StreamWriter r = new StreamWriter(filename, true))
                    r.WriteLine(DateTime.Now.ToString() + ": [" + type + "] " + message);
            }
            catch
            {
                Console.WriteLine(new string('#', 40));
                Console.WriteLine(new string('#', 40));
                Console.WriteLine(new string('#', 40));
                Console.WriteLine(new string('#', 40));
                Console.WriteLine("Error writing log file");
            }
        }
    }
}
