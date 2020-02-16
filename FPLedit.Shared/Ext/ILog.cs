using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface ILog
    {
        bool CanAttach { get; }

        void AttachLogger(ILog other);
        
        void Error(string message);

        void Warning(string message);

        void Info(string message);

        void Debug(string message);

        void LogException(Exception e);
    }

    public static class ExceptionExt
    {
        public static string GetExceptionDetails(this Exception exception)
        {
            try
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
            catch
            {
                return "Fehler beim Erstellen der Fehlerinformationen";
            }
        }
    }
}
