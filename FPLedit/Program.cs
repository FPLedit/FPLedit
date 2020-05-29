using Eto.Forms;
using FPLedit.Config;
using FPLedit.CrashReporting;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FPLedit.Logger;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;
using FPLedit.Templating;

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

            var enableCrashReporting = OptionsParser.CrashReporterDebug;
#if !DEBUG || ENABLE_CRASH_REPORTING_DEBUG
            enableCrashReporting = true; // Aways enable on Release builds.
#endif
            if (enableCrashReporting)
                App.UnhandledException += UnhandledException;

            var (lfh, bootstrapper) = InitializeMainComponents();
            mainForm = new MainForm(lfh, crashReporter, bootstrapper);
            crashReporter = mainForm.CrashReporter;
            
            // Close all other windows when attempting to close main form.
            mainForm.Closing += (s, e) =>
            {
                var windows = App.Windows.ToArray();
                foreach (var window in windows)
                {
                    if (window.Visible && window != mainForm)
                        window.Close();
                }
            };
            
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

        private static (LastFileHandler, Bootstrapper) InitializeMainComponents()
        {
            var lfh = new LastFileHandler();

            // Bootstrap the first main components
            var bootstrapper = new Bootstrapper(lfh);
            
            // Initialize CrashReporter, so it can be used early
            crashReporter = new CrashReporter(bootstrapper);
            
            // Wire up missin pieces in file handling
            lfh.Initialize(bootstrapper);

            // Initailize some loosely coupled UI components, so that extensions can use them
            EtoExtensions.Initialize(bootstrapper); // Initialize Eto extensions
            FontCollection.InitAsync(); // Load list of available fonts, async, as this should not be needed by any extension.
            TemplateDebugger.GetInstance().AttachDebugger(new GuiTemplateDebugger()); // Attach javascript debugger form
            
            Timetable.DefaultLinearVersion = bootstrapper.FullSettings.GetEnum("core.default-file-format", Timetable.DefaultLinearVersion);
            
            // Load logger before extensions
            var logger = new MultipleLogger();
            if (bootstrapper.FullSettings.Get("log.enable-file", false))
                logger.AttachLogger(new TempLogger(bootstrapper));
            if (OptionsParser.ConsoleLog)
                logger.AttachLogger(new ConsoleLogger());
            bootstrapper.InjectLogger(logger);
            
            // Output some version stats
            bootstrapper.Logger.Debug("Current version: " + VersionInformation.Current.DisplayVersion);
            bootstrapper.Logger.Debug("Runtime version: " + VersionInformation.Current.RuntimeVersion);
            bootstrapper.Logger.Debug("OS version: " + VersionInformation.Current.OsVersion);
            
            // Init feature flags
            FeatureFlags.Initialize(((IReducedPluginInterface)bootstrapper).Settings);
            
            // Add default plugins
            bootstrapper.ExtensionManager.InjectPlugin(new CorePlugins.MenuPlugin(), 0);
            bootstrapper.ExtensionManager.InjectPlugin(new Editor.EditorPlugin(), 0);
            bootstrapper.ExtensionManager.InjectPlugin(new CorePlugins.DefaultPlugin(), 0);

            return (lfh, bootstrapper);
        }
    }
}