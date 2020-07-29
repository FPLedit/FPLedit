using Eto.Forms;
using FPLedit.Config;
using FPLedit.CrashReporting;
using System;
using System.Linq;
using FPLedit.Logger;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;
using FPLedit.Templating;
using Mono.Options;

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
            string flagWarning = null;
            var options = new OptionSet
            {
                { "log-console|mp-log", v => OptionsParser.ConsoleLog = v != null },
                { "tmpl-debug", v => OptionsParser.TemplateDebug = v != null },
                { "crash-debug", v => OptionsParser.CrashReporterDebug = v != null },
                { "<>", v =>
                    {
                        if (OptionsParser.OpenFilename == null)
                            OptionsParser.OpenFilename = v;
                        else
                            throw new Exception("Unbekanntes Flag oder doppelter Dateiname angegeben!");
                    }
                }
            };

            try
            {
                options.Parse(args);
            }
            catch (Exception e)
            {
                flagWarning = e.Message;
            }

            App = new Application();
            App.LocalizeString += OnLocalizeString;

            // Set app & settings paths
            PathManager.Instance.AppDirectory = Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.EntryExecutable);
            if (App.Platform.IsMac)
                PathManager.Instance.SettingsDirectory = Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.ApplicationSettings);

            var enableCrashReporting = OptionsParser.CrashReporterDebug;
#if !DEBUG || ENABLE_CRASH_REPORTING_DEBUG
            enableCrashReporting = true; // Aways enable on Release builds.
#endif
            if (enableCrashReporting)
                App.UnhandledException += UnhandledException;

            var (lfh, bootstrapper) = InitializeMainComponents();
            if (flagWarning != null)
                bootstrapper.PreBootstrapWarnings.Add(flagWarning);
            
            bootstrapper.PreBootstrapExtensions(); // Load extension files.
            
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

            App.Terminating += (s, e) =>
            {
                TemplateDebugger.GetInstance().Dispose();
            };
            
            App.Run(mainForm);
        }

        private static void OnLocalizeString(object sender, LocalizeEventArgs e)
        {
            e.LocalizedText = e.Text;
            if (e.Source is MenuBar)
            {
                e.LocalizedText = e.Text switch
                {
                    "&File" => T._("&Datei"),
                    "&Help" => T._("&Hilfe"),
                    "&View" => T._("&Ansicht"),
                    "&Edit" => T._("&Bearbeiten"),
                    "Quit" => T._("Beenden"),
                    _ => e.Text
                };
            }
            else if (e.Source is AboutDialog)
            {
                e.LocalizedText = e.Text switch
                {
                    "About" => T._("Info"),
                    "Credits" => T._("Mitwirkende"),
                    "License" => T._("Lizenz"),
                    "Developers:" => T._("Entwicklung:"),
                    "Designers:" => T._("Design:"),
                    "Documenters:" => T._("Dokumentation:"),
                    _ => e.Text
                };
            }
        }

        public static void Restart()
        {
            mainForm?.Close(); //HACK: Did not close old windows, when a modal dialog was open.
            App?.Restart();
        }

        private static void UnhandledException(object sender, Eto.UnhandledExceptionEventArgs e)
        {
            var report = new CrashReport(mainForm.Bootstrapper.ExtensionManager, e.ExceptionObject as Exception);
            if (crashReporter != null)
                crashReporter.Report(report);
            else
                Console.Error.WriteLine(report.Serialize());

            ExceptionQuit = true;
            App.Restart();
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
#pragma warning disable CA2000
            TemplateDebugger.GetInstance().AttachDebugger(new GuiTemplateDebugger()); // Attach javascript debugger form
#pragma warning restore CA2000

            var origDefaultVersion = Timetable.DefaultLinearVersion;
            Timetable.DefaultLinearVersion = bootstrapper.FullSettings.GetEnum("core.default-file-format", Timetable.DefaultLinearVersion);
            if (Timetable.DefaultLinearVersion.GetCompat() != TtVersionCompatType.ReadWrite)
            {
                bootstrapper.PreBootstrapWarnings.Add(T._("Gewählte Standardversion ist nicht R/W-kompatibel!"));
                Timetable.DefaultLinearVersion = origDefaultVersion;
            }
            
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
            
            var restartable = new RestartHandler(bootstrapper);
            
            // Add default plugins
            bootstrapper.ExtensionManager.InjectPlugin(new CorePlugins.MenuPlugin(), 0);
            bootstrapper.ExtensionManager.InjectPlugin(new Editor.EditorPlugin(), 0);
            bootstrapper.ExtensionManager.InjectPlugin(new CorePlugins.DefaultPlugin(restartable, bootstrapper), 0);

            return (lfh, bootstrapper);
        }
    }
}