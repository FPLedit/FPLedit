using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TemplaetingTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Template tmpl = new Template(File.ReadAllText("test.tmpl"));
            var s = tmpl.GenerateResult();
            Console.WriteLine(s);
            Console.ReadKey();
        }
    }
}
