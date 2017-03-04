using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace FPLedit.Installer
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();

            var inst = new Installer();

            if (args.Contains("/uninstall") || inst.IsInstalled())
                inst.Uninstall();
            else
                inst.Install();
        }
    }
}
