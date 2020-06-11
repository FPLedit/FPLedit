using Eto.Forms;
using System;
using Eto.Drawing;
using System.Linq;
using System.IO;
using System.ComponentModel;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Editor.Rendering;
using FPLedit.Config;

namespace FPLedit
{
    internal sealed class MainForm : FForm, IPlugin
    {
        #region Controls
#pragma warning disable CS0649
        private readonly LogControl logTextBox;
        private readonly ButtonMenuItem saveMenu, saveAsMenu, exportMenu, importMenu, lastMenu, convertMenu;
        private readonly NetworkEditingControl networkEditingControl;
        private readonly StackLayout loadingStack;
#pragma warning restore CS0649
        #endregion

        internal CrashReporting.CrashReporter CrashReporter { get; }
        internal Bootstrapper Bootstrapper { get; }
        
        private TimetableChecks.TimetableCheckRunner checkRunner;
        private readonly LastFileHandler lfh;
        
        public static string LocEditMenu = "&Bearbeiten";
        public static string LocPreviewMenu = "&Vorschau";

        public MainForm(LastFileHandler lfh, CrashReporting.CrashReporter crashReporter, Bootstrapper bootstrapper)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            Icon = new Icon(this.GetResource("Resources.programm.ico"));

            this.lfh = lfh;
            Bootstrapper = bootstrapper;
            CrashReporter = crashReporter;

            lfh.LastFilesUpdates += UpdateLastFilesMenu;

            Bootstrapper.Logger.AttachLogger(logTextBox);
            Bootstrapper.InitializeUi(this);
            Bootstrapper.FileStateChanged += FileStateChanged;
            Bootstrapper.FileHandler.AsyncOperationStateChanged += FileHandlerOnAsyncOperationStateChanged;

            this.AddSizeStateHandler();
        }

        private void FileHandlerOnAsyncOperationStateChanged(object sender, bool e)
        {
            loadingStack.Visible = e;
        }
        
        #region Plugin Code
        
        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            // Initialize main UI component
            networkEditingControl.Initialize(pluginInterface);
            pluginInterface.FileOpened += (s, e) => networkEditingControl.ResetPan();
        }

        private void FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            Title = "FPLedit" + (e.FileState.Opened ? " - "
                + (e.FileState.FileName != null ? (Path.GetFileName(e.FileState.FileName) + " ") : "")
                + (e.FileState.Saved ? "" : "*") : "");

            saveMenu.Enabled = saveAsMenu.Enabled = exportMenu.Enabled = convertMenu.Enabled = e.FileState.Opened;
        }
        
        #endregion
        
        protected override void OnLoad(EventArgs e)
        {
            // Now we can load extensions and templates
            Bootstrapper.ExtensionManager.InjectPlugin(this, 0);
            Bootstrapper.BootstrapExtensions();
            
            var importers = Bootstrapper.GetRegistered<IImport>();

            // Maybe remove import menu.
            if (importers.Length == 0)
                Menu.ApplicationItems.Remove(importMenu);

            // Remove last files menu, if last file handling is disabled.
            if (!lfh.Enabled)
                lastMenu.Enabled = false;

#pragma warning disable CA2000
#if DEBUG
            Menu.HelpMenu.Items.Add(new SeparatorMenuItem());
            Menu.HelpMenu.CreateItem("Exception auslösen", clickHandler: (s, ev) => throw new Exception("Ausgelöste Exception"));
#endif
#pragma warning restore CA2000
            
            base.OnLoad(e);
        }

        protected override void OnShown(EventArgs e)
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
            
            foreach (var msg in Bootstrapper.PreBootstrapWarnings)
                Bootstrapper.Logger.Warning(msg);
            
            checkRunner = new TimetableChecks.TimetableCheckRunner(Bootstrapper); // CheckRunner initialisieren
            
            LoadStartFile();
            Bootstrapper.Update.AutoUpdateCheck(Bootstrapper.Logger);
            
            base.OnShown(e);
        }

        private void LoadStartFile()
        {
            // Parameter: Fpledit.exe [Dateiname] ODER Datei aus Restart
            var fn = OptionsParser.OpenFilename;
            fn = Bootstrapper.FullSettings.Get("restart.file", fn);
            if (fn != null)
            {
                if (File.Exists(fn))
                {
                    Bootstrapper.FileHandler.InternalOpen(fn, true);
                    lfh.AddLastFile(fn);
                }
                else
                    Bootstrapper.Logger.Error($"Angegebene Startdatei {fn} nicht gefunden!");
            }
            Bootstrapper.FullSettings.Remove("restart.file");
        }

        private void UpdateLastFilesMenu(object sender, EventArgs e)
        {
            lastMenu.Items.Clear();
            foreach (var lf in lfh.LastFiles)
            {
#pragma warning disable CA2000
                var itm = lastMenu.CreateItem(lf);
#pragma warning restore CA2000
                itm.Click += (s, a) =>
                {
                    if (Bootstrapper.FileHandler.NotifyIfUnsaved())
                        Bootstrapper.FileHandler.InternalOpen(lf, true);
                };
            }
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
        
        private void ProcessKeyDown(object s, KeyEventArgs e)
        {
            if ((NetworkRenderer.DispatchableKeys.Contains(e.Key) || NetworkEditingControl.DispatchableKeys.Contains(e.Key)) && e.Modifiers == Keys.None)
                networkEditingControl.DispatchKeystroke(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            ProcessKeyDown(null, e);
            base.OnKeyDown(e);
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
            Bootstrapper.FileHandler.InternalOpen(files[0].LocalPath, true);

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
        private void CloseFileMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.CloseFile();
        private void LinearNewMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.New(TimetableType.Linear);
        private void NetworkNewMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.New(TimetableType.Network);
        private void ConvertMenu_Click(object sender, EventArgs e) => Bootstrapper.FileHandler.ConvertTimetable();
        private void AboutMenu_Click(object sender, EventArgs e) => new InfoForm().ShowModal(this);
        private void SettingsMenu_Click(object sender, EventArgs e) => new SettingsUi.SettingsForm(Bootstrapper).ShowModal(this);
        private void HelpMenu_Click(object sender, EventArgs e) => Bootstrapper.OpenUrl("https://fahrplan.manuelhu.de/");
        #endregion

        protected override void Dispose(bool disposing)
        {
            checkRunner?.Dispose();
            foreach (var topLevelItem in Menu.Items)
                topLevelItem.DisposeMenu();
            base.Dispose(disposing);
        }
    }
}
