using System.Collections.Generic;
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
        public static Application? App { get; private set; }

        public static bool ExceptionQuit { get; private set; }

        private static MainForm? mainForm;
        private static CrashReporter? crashReporter;
        private static bool crashReporterStarted;

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            var prePreBootstrapWarnings = new List<string>();
            var options = new OptionSet
            {
                { "log-console|mp-log", v => OptionsParser.ConsoleLog = v != null },
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
                prePreBootstrapWarnings.Add(e.Message);
            }

            App = new Application();
            App.LocalizeString += OnLocalizeString;

            if (App.Platform.IsWpf)
            {
                // As of https://github.com/picoe/Eto/pull/2046/files, a default manifest is extracted by Eto and stored
                // in the temp dir. This is unwanted in our case, as we have our own windows manifest.
                // Use reflection to avoid loading Eto.Wpf on all platforms.
                try
                {
                    App.Handler.GetType()
                        .GetField("EnableVisualStyles", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)!
                        .SetValue(null, false);
                }
                catch (Exception e)
                {
                    prePreBootstrapWarnings.Add("Fehler beim Setzen von EnableVisualStyles: " + e.Message);
                }
            }

            // Set app & settings paths
            PathManager.Instance.AppDirectory = Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.EntryExecutable);
            if (App.Platform.IsMac)
                PathManager.Instance.SettingsDirectory = Eto.EtoEnvironment.GetFolderPath(Eto.EtoSpecialFolder.ApplicationSettings);

#if !DEBUG || ENABLE_CRASH_REPORTING_DEBUG
            var enableCrashReporting = true; // Aways enable on Release builds.
#else
            var enableCrashReporting = OptionsParser.CrashReporterDebug || !System.Diagnostics.Debugger.IsAttached;
#endif
            if (enableCrashReporting)
                App.UnhandledException += UnhandledException;

            var (lfh, bootstrapper) = InitializeMainComponents();
            if (prePreBootstrapWarnings.Any())
                bootstrapper.PreBootstrapWarnings.AddRange(prePreBootstrapWarnings);
            prePreBootstrapWarnings = null; // Avoid using the pre-pre-bootstrap warning collection after transfer to the bootstrapper.

            bootstrapper.PreBootstrapExtensions(); // Load extension files.

            mainForm = new MainForm(lfh, crashReporter!, bootstrapper);

            // Close all other windows when attempting to close main form.
            mainForm.Closing += (_, _) =>
            {
                var windows = App.Windows.ToArray();
                foreach (var window in windows)
                {
                    if (window.Visible && window != mainForm)
                        window.Close();
                }
                // Terminate the application - no idea why this is _now_ needed - it wasn't before.
                Environment.Exit(0);
            };

            App.Run(mainForm);
        }

        private static void OnLocalizeString(object? sender, LocalizeEventArgs e)
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

        private static void UnhandledException(object? sender, Eto.UnhandledExceptionEventArgs e)
        {
            var crashReporterStartedBefore = crashReporterStarted;
            var report = new CrashReport(mainForm?.Bootstrapper.ExtensionManager, e.ExceptionObject as Exception);
            if (crashReporter != null && !crashReporterStarted)
            {
                crashReporterStarted = true;
                crashReporter.Report(report);
            }
            else
                Console.Error.WriteLine(report.Serialize());

            bool shouldRestart;
            try { shouldRestart = !System.Diagnostics.Debugger.IsAttached; }
            catch { shouldRestart = true; }

            if (!crashReporterStartedBefore)
            {
                ExceptionQuit = true;
                if (shouldRestart)
                    App?.Restart();
                Environment.Exit(-1);
            }
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
            MGraphics.PreferSystemDrawing = bootstrapper.FullSettings.Get<bool>("bifpl.prefer-system-drawing-renderer");
#pragma warning disable CA2000
            TemplateDebugger.GetInstance().AttachDebugger(new GuiTemplateDebugger()); // Attach javascript debugger form
#pragma warning restore CA2000

            // Reset default file format versions on first run.
            var lastRunVersion = bootstrapper.FullSettings.Get<string>("updater.lastrun-version");
            if (!string.IsNullOrEmpty(lastRunVersion) && lastRunVersion != VersionInformation.Current.DisplayVersion)
            {
                bootstrapper.FullSettings.SetEnum("core.default-network-file-format", Timetable.PRESET_NETWORK_VERSION);
                bootstrapper.FullSettings.SetEnum("core.default-file-format", Timetable.PRESET_LINEAR_VERSION);
                bootstrapper.PreBootstrapWarnings.Add(T._("Die Standard-Dateiformatversionen wurden zurückgesetzt, da Sie die Version von FPLedit aktualisiert haben. Diese Einstellung kann unter Einstellungen > Dateiversionen geändert werden."));
            }
            bootstrapper.FullSettings.Set("updater.lastrun-version", VersionInformation.Current.DisplayVersion);
            
            // Load default versions for new timetable files from config.
            var linearDefaultVersion = bootstrapper.FullSettings.GetEnum("core.default-file-format", Timetable.DefaultLinearVersion);
            var linCompat = linearDefaultVersion.GetVersionCompat();
            if (linCompat.Compatibility == TtVersionCompatType.ReadWrite && linCompat.Type == TimetableType.Linear)
                Timetable.DefaultLinearVersion = linearDefaultVersion;
            else
                bootstrapper.PreBootstrapWarnings.Add(T._("Gewählte lineare Standardversion ist nicht R/W-kompatibel!"));

            var networkDefaultVersion = bootstrapper.FullSettings.GetEnum("core.default-network-file-format", Timetable.DefaultNetworkVersion);
            var netCompat = networkDefaultVersion.GetVersionCompat();
            if (netCompat.Compatibility == TtVersionCompatType.ReadWrite && netCompat.Type == TimetableType.Network)
                Timetable.DefaultNetworkVersion = networkDefaultVersion;
            else 
                bootstrapper.PreBootstrapWarnings.Add(T._("Gewählte Netzwerk-Standardversion ist nicht R/W-kompatibel!"));

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
