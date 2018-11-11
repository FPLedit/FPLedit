using Eto.Forms;
using FPLedit.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPLedit
{
    static class Program
    {
        public static Application App { get; private set; }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)] // Hopefully it doesn't break any things
        static void Main(string[] args)
        {
            OptionsParser.Init(args);

            var application = new Application();
            App = application;
            application.Run(new MainForm(application));
        }
    }
}
