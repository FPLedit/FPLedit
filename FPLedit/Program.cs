using Eto.Forms;
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
        static void Main()
        {
            var application = new Application();
            App = application;
            application.Run(new MainForm(application));
        }
    }
}
