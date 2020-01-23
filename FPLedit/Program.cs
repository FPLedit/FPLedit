using Eto.Forms;
using FPLedit.Config;
using FPLedit.CrashReporting;
using System;
using System.Diagnostics;
using System.Reflection;

namespace FPLedit
{
    internal static class Program
    {
        public static Application App { get; private set; }

        public static bool ExceptionQuit { get; private set; }

        private static MainForm mainForm;
        private static CrashReporter crashReporter;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            PathManager.Instance.AppFilePath = Assembly.GetExecutingAssembly().Location;
            
            OptionsParser.Init(args);

            App = new Application();
#if !DEBUG || CRASH_DEBUG
            App.UnhandledException += UnhandledException;
#endif

            mainForm = new MainForm();
            crashReporter = mainForm.CrashReporter;
            App.Run(mainForm);
        }

        private static void UnhandledException(object sender, Eto.UnhandledExceptionEventArgs e)
        {
            var report = new CrashReport(mainForm.Bootstrapper.ExtensionManager, e.ExceptionObject as Exception);
            if (crashReporter != null)
                crashReporter.Report(report);
            else
                Console.Error.WriteLine(report.Serialize());

            ExceptionQuit = true;
            Process.Start(PathManager.Instance.AppFilePath);
            Environment.Exit(-1);
        }
    }
}
