using Eto.Forms;
using System;
using Eto.Drawing;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Logger;
using FPLedit.Templating;
using FPLedit.Editor.Rendering;
using FPLedit.Config;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.Helpers;

namespace FPLedit
{
    internal sealed class MainForm : FForm, IRestartable, IPlugin
    {
        #region Controls
#pragma warning disable CS0649
        private readonly LogControl logTextBox;
        private readonly ButtonMenuItem saveMenu, saveAsMenu, exportMenu, importMenu, lastMenu, fileMenu, convertMenu;
        private readonly NetworkEditingControl networkEditingControl;
#pragma warning restore CS0649
        #endregion

        internal CrashReporting.CrashReporter CrashReporter { get; }
        internal Bootstrapper Bootstrapper { get; }
        
        private TimetableChecks.TimetableCheckRunner checkRunner;
        private readonly LastFileHandler lfh;
        
        public static string LocEditMenu = "Bearbeiten";
        public static string LocPreviewMenu = "Vorschau";
        public static string LocHelpMenu = "Hilfe";

        public MainForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            Icon = new Icon(this.GetResource("Resources.programm.ico"));

            lfh = new LastFileHandler();
            lfh.LastFilesUpdates += UpdateLastFilesMenu;
            
            // Bootstrap the first main components
            Bootstrapper = new Bootstrapper(this, lfh);
            
            // Initialize CrashReporter, so it can be used early
            CrashReporter = new CrashReporting.CrashReporter(Bootstrapper);
            
            // Wire up missin pieces in file handling
            lfh.Initialize(Bootstrapper);

            // Initailize some loosely coupled UI components, so that extensions can use them
            EtoExtensions.Initialize(Bootstrapper); // Initialize Eto extensions
            FontCollection.InitAsync(); // Load list of available fonts, async, as this should not be needed by any extension.
            TemplateDebugger.GetInstance().AttachDebugger(new GuiTemplateDebugger()); // Attach javascript debugger form
            
            Timetable.DefaultLinearVersion = Bootstrapper.FullSettings.GetEnum("core.default-file-format", Timetable.DefaultLinearVersion);
            
            // Load logger before extensions
            var logger = new MultipleLogger(logTextBox);
            if (Bootstrapper.FullSettings.Get("log.enable-file", false))
                logger.AttachLogger(new TempLogger(Bootstrapper));
            if (OptionsParser.MPCompatLog)
                logger.AttachLogger(new ConsoleLogger());
            Bootstrapper.InjectLogger(logger);
            
            // Output some version stats
            Bootstrapper.Logger.Debug("Current version: " + VersionInformation.Current.DisplayVersion);
            Bootstrapper.Logger.Debug("Runtime version: " + VersionInformation.Current.RuntimeVersion);
            Bootstrapper.Logger.Debug("OS version: " + VersionInformation.Current.OsVersion);

            // Now we can load extensions and templates
            Bootstrapper.ExtensionManager.InjectPlugin(new Editor.EditorPlugin(), 0);
            Bootstrapper.ExtensionManager.InjectPlugin(this, 0);
            Bootstrapper.BootstrapExtensions();

            if (Bootstrapper.FullSettings.IsReadonly)
                Bootstrapper.Logger.Warning("Die Einstellungsdatei ist nicht schreibbar. Änderungen an Programmeinstellungen werden verworfen.");
            
            void PassKeyDown(object s, KeyEventArgs e)
            {
                if (NetworkRenderer.DispatchableKeys.Contains(e.Key) && e.Modifiers == Keys.None)
                    networkEditingControl.DispatchKeystroke(e);
            }
            KeyDown += PassKeyDown;
            logTextBox.KeyDown += PassKeyDown; // Fix for Gtk, no harm on Wpf.
            
            Shown += OnShown;
            Bootstrapper.FileStateChanged += FileStateChanged;

            checkRunner = new TimetableChecks.TimetableCheckRunner(Bootstrapper); // CheckRunner initialisieren
            
            this.AddSizeStateHandler();
        }
        
        #region Plugin Code
        
        public void Init(IPluginInterface pluginInterface)
        {
            // Initilize main UI component
            networkEditingControl.Initialize(pluginInterface);
            pluginInterface.FileOpened += (s, e) => networkEditingControl.ResetPan();
            pluginInterface.ExtensionsLoaded += PluginInterfaceOnExtensionsLoaded;
        }

        private void PluginInterfaceOnExtensionsLoaded(object sender, EventArgs e)
        {
            var importers = Bootstrapper.GetRegistered<IImport>();

            // Ggf. Import bzw. Export-Menü entfernen
            if (importers.Length == 0)
                fileMenu.Items.Remove(importMenu);

            // Ggf. Letzte Dateien Menü entfernen
            if (!lfh.Enabled)
                lastMenu.Enabled = false;

            // Hilfe Menü nach den Erweiterungen zusammenbasteln
            var helpMenu = (ButtonMenuItem) Menu.GetItem(LocHelpMenu);
            if (helpMenu.Items.Any())
                helpMenu.Items.Add(new SeparatorMenuItem());
            helpMenu.CreateItem("Erweiterungen", clickHandler: (s, ev) => new ExtensionsForm(Bootstrapper.ExtensionManager, this).ShowModal(this));
            helpMenu.CreateItem("Vorlagen", clickHandler: (s, ev) => new TemplatesForm(Bootstrapper.TemplateManager as TemplateManager).ShowModal(this));
            helpMenu.Items.Add(new SeparatorMenuItem());
            helpMenu.CreateItem("Fenstergößen löschen", clickHandler: (s, ev) => SizeManager.Reset());
            helpMenu.Items.Add(new SeparatorMenuItem());
            helpMenu.CreateItem("Online Hilfe", clickHandler: (s, ev) => OpenHelper.Open("https://fahrplan.manuelhu.de/"));
            helpMenu.CreateItem("Info", clickHandler: (s, ev) => new InfoForm(Bootstrapper.FullSettings).ShowModal(this));

#if DEBUG
            helpMenu.Items.Add(new SeparatorMenuItem());
            helpMenu.CreateItem("Exception auslösen", clickHandler: (s, ev) => throw new Exception("Ausgelöste Exception"));
#endif
        }

        private void FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            Title = "FPLedit" + (e.FileState.Opened ? " - "
                + (e.FileState.FileName != null ? (Path.GetFileName(e.FileState.FileName) + " ") : "")
                + (e.FileState.Saved ? "" : "*") : "");

            saveMenu.Enabled = saveAsMenu.Enabled = exportMenu.Enabled = convertMenu.Enabled = e.FileState.Opened;
        }
        
        #endregion

        private void OnShown(object sender, EventArgs e)
        {
            // Hatten wir einen Crash beim letzten Mal?
            if (CrashReporter.HasCurrentReport)
            {
                try
                {
                    using (var cf = new CrashReporting.CrashForm(CrashReporter))
                    {
                        if (cf.ShowModal(this) == DialogResult.Ok)
                            CrashReporter.Restore(Bootstrapper.FileHandler);
                        CrashReporter.RemoveCrashFlag();
                    }
                }
                catch (Exception ex)
                {
                    Bootstrapper.Logger.Error("Fehlermeldung des letzten Absturzes konnte nicht angezeigt werden: " + ex.Message);
                    CrashReporter.RemoveCrashFlag(); // Der Crash crasht sogar noch die Fehlerbehandlung...
                }
            }
            
            LoadStartFile();
            Bootstrapper.Update.AutoUpdateCheck(Bootstrapper.Logger);
        }

        private void LoadStartFile()
        {
            // Parameter: Fpledit.exe [Dateiname] ODER Datei aus Restart
            var fn = OptionsParser.OpenFilename;
            fn = Bootstrapper.FullSettings.Get("restart.file", fn);
            if (fn != null && File.Exists(fn))
            {
                Bootstrapper.FileHandler.InternalOpen(fn);
                lfh.AddLastFile(fn);
            }
            Bootstrapper.FullSettings.Remove("restart.file");
        }

        private void UpdateLastFilesMenu(object sender, EventArgs e)
        {
            lastMenu.Items.Clear();
            foreach (var lf in lfh.LastFiles)
            {
                var itm = lastMenu.CreateItem(lf);
                itm.Click += (s, a) =>
                {
                    if (Bootstrapper.FileHandler.NotifyIfUnsaved())
                        Bootstrapper.FileHandler.InternalOpen(lf);
                };
            }
        }

        public void RestartWithCurrentFile()
        {
            if (!Bootstrapper.FileHandler.NotifyIfUnsaved())
                return;
            if (Bootstrapper.FileState.Opened)
                Bootstrapper.FullSettings.Set("restart.file", Bootstrapper.FileState.FileName);

            Process.Start(PathManager.Instance.AppFilePath);
            Program.App.Quit();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Bootstrapper.FullSettings.KeyExists("restart.file") && !Program.ExceptionQuit)
            {
                lfh.Persist();
                if (!Bootstrapper.FileHandler.NotifyIfUnsaved())
                    e.Cancel = true;

                Bootstrapper.ClearTemp();
            }
            
            base.OnClosing(e);
        }

        #region Drag'n'Drop
        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Effects = DragEffects.None;

            if (!e.Data.Types.Contains("FileDrop"))
                return;

            Uri[] files = e.Data.Uris;
            if (files.Length == 1 && files[0].AbsolutePath.EndsWith(".fpl"))
                e.Effects = DragEffects.Copy;

            base.OnDragEnter(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            Uri[] files = e.Data.Uris;
            if (files == null || files.Length != 1 || !files[0].AbsolutePath.EndsWith(".fpl"))
                return;

            if (!Bootstrapper.FileHandler.NotifyIfUnsaved())
                return;
            Bootstrapper.FileHandler.InternalOpen(files[0].LocalPath);

            base.OnDragDrop(e);
        }
        #endregion

        #region Events
        private void SaveMenu_Click(object sender, EventArgs e) => Bootstrapper.Save(false);
        private void OpenMenu_Click(object sender, EventArgs e) => Bootstrapper.Open();
        private void SaveAsMenu_Click(object sender, EventArgs e) => Bootstrapper.Save(true);
        private void ImportMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.Import();
        private void ExportMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.Export();
        private void QuitMenu_Click(object sender, EventArgs e) => Close();
        private void LinearNewMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.New(TimetableType.Linear);
        private void NetworkNewMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.New(TimetableType.Network);
        private void ConvertMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.ConvertTimetable();
        #endregion

        protected override void Dispose(bool disposing)
        {
            checkRunner?.Dispose();
            base.Dispose(disposing);
        }
    }
}
