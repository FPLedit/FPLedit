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
    }
}
