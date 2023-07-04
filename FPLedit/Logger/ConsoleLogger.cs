using FPLedit.Shared;
using System;

namespace FPLedit.Logger
{
    public sealed class ConsoleLogger : ILog
    {
        public bool CanAttach => false;

        public void AttachLogger(ILog other)
        {
        }
    
        public void Error(string message)
            => Console.WriteLine("[ERROR] " + message);

        public void Info(string message)
            => Console.WriteLine("[INFO] " + message);

        public void Warning(string message)
            => Console.WriteLine("[WARN] " + message);

        public void LogException(Exception e)
            => Console.WriteLine("[EXCEPTION] " + e.GetExceptionDetails());

        public void Debug(string message)
            => Console.WriteLine("[DEBUG] " + message);
    }
}
