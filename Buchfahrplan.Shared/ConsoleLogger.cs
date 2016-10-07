using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Shared
{
    public class ConsoleLogger : ILog
    {
        public void Error(string message)
        {
            Console.WriteLine("Fehler: " + message);
        }

        public void Info(string message)
        {
            Console.WriteLine(message);
        }

        public void Warning(string message)
        {
            Console.WriteLine("Warnung; " + message);
        }
    }
}
