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
            App.Run(mainForm);
        }

        private static void UnhandledException(object sender, Eto.UnhandledExceptionEventArgs e)
        {
            var info = new CrashReport(mainForm.extensionManager, e.ExceptionObject as Exception);
            var telegram = info.Serialize();
            try
            {
                var fn_tt = mainForm.GetTemp("crash_tt.fpl");
                new Shared.Filetypes.XMLExport().Export(mainForm.Timetable, fn_tt, mainForm);

                var fn = mainForm.GetTemp("crash_report.xml");
                File.WriteAllText(fn, telegram);

                MessageBox.Show("Es ist ein unerwarteter Fehler in FPLedit aufgetreten." + Environment.NewLine + Environment.NewLine +
                    "FPLedit wird neu gestartet. Möglicherweise ist eine Wiederherstellung möglich.", MessageBoxType.Error);
            }
            catch
            {
                MessageBox.Show("Es ist ein unerwarteter Fehler in FPLedit aufgetreten. Es konnten keine weiteren Informationen gespeichert werden. FPLedit wird neu gestartet.");
            }

            ExceptionQuit = true;
            Process.Start(Assembly.GetEntryAssembly().Location);
            Environment.Exit(-1);
        }
    }
}
