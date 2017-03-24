using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit
{
    internal class TempLogger : ILog
    {
        private string filename;

        public TempLogger()
        {
            filename = Path.Combine(Path.GetTempPath(), "fpledit_log.txt");
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

        public void Warning(string message)
        {
            Write(message, "WARN");
        }

        private void Write(string message, string type)
        {
            using (StreamWriter r = new StreamWriter(filename, true))
            {
                r.WriteLine(DateTime.Now.ToString() + ": [" + type + "] " + message);
            }
        }
    }
}
