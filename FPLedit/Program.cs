using Eto.Forms;
using FPLedit.Config;
using FPLedit.CrashReporting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FPLedit
{
    static class Program
    {
        public static Application App { get; private set; }

        public static bool ExceptionQuit { get; private set; }

        private static MainForm mainForm;
        private static CrashReporter crashReporter;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        [LoaderOptimization(LoaderOptimization.MultiDomainHost)] // Hopefully it doesn't break any things
        static void Main(string[] args)
        {
            OptionsParser.Init(args);

            App = new Application();
            App.UnhandledException += UnhandledException;

            mainForm = new MainForm();
            crashReporter = mainForm.crashReporter;
            App.Run(mainForm);
        }

        private static void UnhandledException(object sender, Eto.UnhandledExceptionEventArgs e)
        {
            var info = new CrashReport(mainForm.extensionManager, e.ExceptionObject as Exception);
            crashReporter.Report(info);

            ExceptionQuit = true;
            Process.Start(Assembly.GetEntryAssembly().Location);
            Environment.Exit(-1);
        }
    }
}
