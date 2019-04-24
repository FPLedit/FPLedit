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
        private string filename;

        public FileLogger(string fn)
        {
            filename = fn;

            if (File.Exists(filename) && new FileInfo(filename).Length > 10240) // > 10KB
                File.Delete(filename);

            Write("FPLedit Programmstart", "INFO");
        }

        public void Error(string message)
        {
            Write(message, "EROR");
        }

        public void Info(string message)
        {
            Write(message, "INFO");
        }

        public void LogException(Exception e)
        {
            string details = "Fehler beim Erstellen der Fehlerinformationen";
            try
            {
                details = GetExceptionDetails(e);
            }
            catch { }
            Write(details, "EXCP");
        }

        private string GetExceptionDetails(Exception exception)
        {
            var properties = exception.GetType().GetProperties();
            var fields = properties.Select(property => new
            {
                property.Name,
                Value = property.GetValue(exception, null)
            })
                .Select(x => string.Format(
                    "{0} = {1}",
                    x.Name,
                    x.Value != null ? x.Value.ToString() : ""
                ));
            return string.Join("\n", fields);
        }

        public void Warning(string message)
        {
            Write(message, "WARN");
        }

        public void Debug(string message)
        {
            Write(message, "DEBUG");
        }

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
