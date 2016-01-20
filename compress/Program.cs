using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace compress
{
    class Program
    {
        static void Main(string[] args)
        {
            string sep = "////////////";

            string f1 = File.ReadAllText("GLOB_TEMPLATE");
            string f2 = File.ReadAllText("LINE_TEMPLATE");
            string f3 = File.ReadAllText("TRAIN_TEMPLATE");

            string nl = Environment.NewLine;

            string res = sep + "GLOB_TEMPLATE" + sep + nl + f1;
            res += nl + sep + "LINE_TEMPLATE" + sep + nl + f2;
            res += nl + sep + "TRAIN_TEMPLATE" + sep + nl + f3;

            File.WriteAllText("output.tmpl", res);
        }
    }
}
