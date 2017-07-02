using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Logger
{
    public class TempLogger : ILog
    {
        private string filename;

        public TempLogger(IInfo info)
        {
            filename = info.GetTemp("fpledit_log.txt");

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
