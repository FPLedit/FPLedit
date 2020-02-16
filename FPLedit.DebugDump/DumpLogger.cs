using FPLedit.Shared;
using System;

namespace FPLedit.DebugDump
{
    public class DumpLogger : ILog
    {
        private readonly DumpWriter writer;

        public DumpLogger(DumpWriter writer)
        {
            this.writer = writer;
        }

        public bool CanAttach => false;

        public void AttachLogger(ILog other)
        {
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
            writer.WriteEvent(DumpEventType.Log, type, message);
        }
    }
}