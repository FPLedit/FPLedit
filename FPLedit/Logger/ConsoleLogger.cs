using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Logger
{
    public class ConsoleLogger : ILog
    {
        public void Error(string message)
        {
            Console.WriteLine("[ERROR] " + message);
        }

        public void Info(string message)
        {
            Console.WriteLine("[INFO] " + message);
        }

        public void Warning(string message)
        {
            Console.WriteLine("[WARN] " + message);
        }

        public void LogException(Exception e)
        {
            string details = "Fehler beim Erstellen der Fehlerinformationen";
            try
            {
                details = GetExceptionDetails(e);
            }
            catch { }
            Console.WriteLine("[EXCEPTION] " + details);
        }

        private string GetExceptionDetails(Exception exception)
        {
            var properties = exception.GetType().GetProperties();
            var fields = properties.Select(property => new {
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

        public void Debug(string message)
        {
            Console.WriteLine("[DEBUG] " + message);
        }
    }
}
