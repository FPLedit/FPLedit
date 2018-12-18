using Eto.Forms;
using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;
using FPLedit.Logger;
using FPLedit.Templating;
using FPLedit.Editor.Network;
using FPLedit.Config;
using FPLedit.NonDefaultFiletypes;

namespace FPLedit
{
    public class MainForm : Form, IInfo, IRestartable
    {
        #region Controls
#pragma warning disable CS0649
        private readonly LogControl logTextBox;
        private readonly ButtonMenuItem saveMenu, saveAsMenu, exportMenu, importMenu, lastMenu, fileMenu, convertMenu;
        private SaveFileDialog saveFileDialog, exportFileDialog;
        private OpenFileDialog openFileDialog, importFileDialog;
        private LineEditingControl lineEditingControl;
#pragma warning restore CS0649
        #endregion

        private string templatePath = "templates";

        private Timetable timetableBackup = null;

        private IImport open;
        private IExport save;

        private FileState fileState;
        private TemplateManager templateManager;
        private ExtensionManager extensionManager;
        private UndoManager undo;
        private RegisterStore registry;
        private UpdateManager update;

        private List<string> lastFiles;
        private bool enable_last = true;
        private string lastPath;

        private Application application;

        public Timetable Timetable { get; set; }

        public ILog Logger { get; private set; }

        public ISettings Settings { get; private set; }

        #region FileState

        public IFileState FileState => fileState;

        public void SetUnsaved()
        {
            undo.AddUndoStep();
            fileState.Saved = false;
        }

        private void OnFileStateChanged()
        {
            fileState.UpdateMetaProperties(Timetable, undo);

            saveMenu.Enabled = saveAsMenu.Enabled = exportMenu.Enabled = convertMenu.Enabled = fileState.Opened;

            FileStateChanged?.Invoke(this, new FileStateChangedEventArgs(fileState));

            Title = "FPLedit - "
                + (fileState.FileName != null ? (Path.GetFileName(fileState.FileName) + " ") : "")
                + (fileState.Saved ? "" : "*");
        }

        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        public event EventHandler ExtensionsLoaded;

        #endregion

        public MainForm(Application app)
        {
            application = app;
            Eto.Serialization.Xaml.XamlReader.Load(this);
            Icon = new Icon(this.GetResource("Resources.programm.ico"));

            Settings = new Settings();
            undo = new UndoManager();
            registry = new RegisterStore();
            update = new UpdateManager(Settings);
            extensionManager = new ExtensionManager(this, update);

            lineEditingControl.Initialize(this);

            saveFileDialog = new SaveFileDialog();
            openFileDialog = new OpenFileDialog();
            exportFileDialog = new SaveFileDialog();
            importFileDialog = new OpenFileDialog();

            open = new XMLImport();
            save = new XMLExport();
            saveFileDialog.AddLegacyFilter(save.Filter);
            openFileDialog.AddLegacyFilter(open.Filter);

            fileState = new FileState();
            fileState.FileStateInternalChanged += (s, e) => OnFileStateChanged();

            var logger = new MultipleLogger(logTextBox);
            if (Settings.Get("log.enable-file", false))
                logger.Loggers.Add(new TempLogger(this));
            if (OptionsParser.MPCompatLog)
                logger.Loggers.Add(new ConsoleLogger());
            Logger = logger;

            KeyDown += (s, e) =>
            {
                if (e.Key == Keys.R || e.Key == Keys.Escape)
                    lineEditingControl.DispatchKeystroke(e);
            };

            Init();
        }

        #region Initialization

        private void Init()
        {
            FontCollection.InitAsync(); // Asynchron Liste von verfügbaren Schriftarten laden
            EtoExtensions.Initialize(this); // UI-Erweiterungen initialiseren
            this.AddSizeStateHandler();

            // Extensions laden & initialisieren (=> Initialisiert Importer/Exporter)
            extensionManager.LoadExtensions();
            extensionManager.InitActivatedExtensions();

            InitializeExportImport();
            InitializeMenus();

            // Letzte Pfade initialisieren
            lastPath = Settings.Get("files.lastpath", "");
            if (lastPath != "" && Uri.TryCreate(lastPath, UriKind.Absolute, out Uri uri))
            {
                openFileDialog.Directory = uri;
                saveFileDialog.Directory = uri;
            }

            // Vorlagen laden
            templatePath = Settings.Get("tmpl.root", templatePath);
            templateManager = new TemplateManager(registry, Logger);
            templateManager.LoadTemplates(templatePath);

            ExtensionsLoaded?.Invoke(this, new EventArgs());

            Shown += LoadStartFile;
            Shown += (s, e) => update.AutoUpdateCheck(Logger);
        }

        private void LoadStartFile(object sender, EventArgs e)
        {
            // Parameter: Fpledit.exe [Dateiname] ODER Datei aus Restart
            var fn = OptionsParser.OpenFilename;
            fn = Settings.Get("restart.file", fn);
            if (fn != null && File.Exists(fn))
            {
                InternalOpen(fn);
                AddLastFile(fn);
            }
            Settings.Remove("restart.file");
            Shown -= LoadStartFile;
        }

        private void InitializeExportImport()
        {
            var exporters = registry.GetRegistered<IExport>();
            var importers = registry.GetRegistered<IImport>();

            exportFileDialog.AddLegacyFilter(exporters.Select(ex => ex.Filter).ToArray());
            importFileDialog.AddLegacyFilter(importers.Select(im => im.Filter).ToArray());

            // Letzten Exporter auswählen
            int exporter_idx = Settings.Get("exporter.last", -1);
            if (exporter_idx > -1 && exporters.Length > exporter_idx)
                exportFileDialog.CurrentFilterIndex = exporter_idx + 1;

            // Ggf. Import bzw. Export-Menü entfernen
            if (importers.Length == 0)
                fileMenu.Items.Remove(importMenu);
        }

        private void InitializeMenus()
        {
            // Zuletzt geöffnete Dateien anzeigen
            enable_last = Settings.Get("files.save-last", true);
            if (enable_last)
            {
                lastFiles = Settings.Get("files.last", "").Split(';').Where(s => s != "").Reverse().ToList();
                UpdateLastFilesMenu();
            }
            else
                lastMenu.Enabled = false;

            // Hilfe Menü nach den Erweiterungen zusammenbasteln
            var helpItem = Menu.CreateItem("Hilfe");
            var extItem = helpItem.CreateItem("Erweiterungen");
            extItem.Click += (s, ev) => new ExtensionsForm(extensionManager, this).ShowModal(this);
            var tmplItem = helpItem.CreateItem("Vorlagen");
            tmplItem.Click += (s, ev) => new TemplatesForm(templateManager, templatePath).ShowModal(this);
            helpItem.Items.Add(new SeparatorMenuItem());
            var clearSizesItem = helpItem.CreateItem("Fenstergößen löschen");
            clearSizesItem.Click += (s, ev) => SizeManager.Reset();
            helpItem.Items.Add(new SeparatorMenuItem());
            var docItem = helpItem.CreateItem("Online Hilfe");
            docItem.Click += (s, ev) => Process.Start("https://fahrplan.manuelhu.de/");
            var infoItem = helpItem.CreateItem("Info");
            infoItem.Click += (s, ev) => new InfoForm(Settings).ShowModal(this);
        }

        private void UpdateLastFilesMenu()
        {
            lastMenu.Items.Clear();
            foreach (var lf in lastFiles)
            {
                var itm = lastMenu.CreateItem(lf);
                itm.Click += (s, a) =>
                {
                    if (NotifyIfUnsaved())
                        InternalOpen(lf);
                };
            }
        }

        #endregion

        #region FileHandling
        private void Import()
        {
            var importers = registry.GetRegistered<IImport>();

            if (importers.Length == 0)
            {
                Logger.Error("Keine Importer gefunden, Import nicht möglich!");
                return;
            }

            if (!NotifyIfUnsaved())
                return;
            if (importFileDialog.ShowDialog(this) == DialogResult.Ok)
            {
                IImport import = importers[importFileDialog.CurrentFilterIndex - 1];
                Logger.Info("Öffne Datei " + importFileDialog.FileName);
                Timetable = import.Import(importFileDialog.FileName, Logger);
                if (Timetable == null)
                    return;
                Logger.Info("Datei erfolgeich geöffnet!");
                fileState.Opened = true;
                fileState.Saved = true;
                fileState.FileName = importFileDialog.FileName;
                undo.ClearHistory();
            }
        }

        private void Export()
        {
            if (exportFileDialog.ShowDialog(this) == DialogResult.Ok)
            {
                var exporters = registry.GetRegistered<IExport>();
                IExport export = exporters[exportFileDialog.CurrentFilterIndex];
                string filename = exportFileDialog.FileName;

                Logger.Info("Exportiere in Datei " + filename);
                bool ret = export.Export(Timetable, filename, this);
                if (ret == false)
                {
                    Logger.Error("Exportieren fehlgeschlagen!");
                    return;
                }
                Logger.Info("Exportieren erfolgreich abgeschlossen!");
                Settings.Set("exporter.last", exportFileDialog.CurrentFilterIndex);
            }
        }

        public void Open()
        {
            if (!NotifyIfUnsaved())
                return;
            if (openFileDialog.ShowDialog(this) == DialogResult.Ok)
            {
                InternalOpen(openFileDialog.FileName);
                UpdateLastPath(openFileDialog);
                AddLastFile(openFileDialog.FileName);
            }
        }

        public void Reload()
            => InternalOpen(fileState.FileName);

        private void InternalOpen(string filename)
        {
            Logger.Info("Öffne Datei " + filename);
            Timetable = open.Import(filename, Logger);
            if (Timetable == null)
                Logger.Error("Fehler beim Öffnen der Datei!");
            else
                Logger.Info("Datei erfolgeich geöffnet!");
            fileState.Opened = Timetable != null;
            fileState.Saved = true;
            fileState.FileName = Timetable != null ? filename : null;
            undo.ClearHistory();
            lineEditingControl.ResetPan();
        }

        public void Save(bool forceSaveAs)
        {
            string filename = fileState.FileName;

            bool saveAs = forceSaveAs || filename == null || filename == "" || Path.GetExtension(filename) != ".fpl";

            if (saveAs)
            {
                if (saveFileDialog.ShowDialog(this) != DialogResult.Ok)
                    return;
                filename = saveFileDialog.FileName;
                UpdateLastPath(saveFileDialog);
                AddLastFile(saveFileDialog.FileName);
            }
            InternalSave(filename);
        }

        private void InternalSave(string filename)
        {
            if (!filename.EndsWith(".fpl"))
                filename += ".fpl";

            Logger.Info("Speichere Datei " + filename);
            bool ret = save.Export(Timetable, filename, this);
            if (ret == false)
                return;
            Logger.Info("Speichern erfolgreich abgeschlossen!");
            fileState.Saved = true;
            fileState.FileName = filename;
        }

        private void New(TimetableType type)
        {
            if (!NotifyIfUnsaved())
                return;
            Timetable = new Timetable(type);
            fileState.Opened = true;
            fileState.Saved = false;
            fileState.FileName = null;
            undo.ClearHistory();
            Logger.Info("Neue Datei erstellt");
        }

        private bool NotifyIfUnsaved()
        {
            if (!fileState.Saved && fileState.Opened)
            {
                DialogResult res = MessageBox.Show("Wollen Sie die noch nicht gespeicherten Änderungen speichern?",
                    "FPLedit", MessageBoxButtons.YesNoCancel);

                if (res == DialogResult.Cancel)
                    return false;
                if (res == DialogResult.Yes)
                    Save(false);
            }
            return true;
        }

        public void RestartWithCurrentFile()
        {
            if (!NotifyIfUnsaved())
                return;
            if (fileState.Opened)
                Settings.Set("restart.file", fileState.FileName);
            application.Restart();
        }

        private void UpdateLastPath(FileDialog dialog)
        {
            lastPath = Path.GetDirectoryName(dialog.FileName);
            openFileDialog.Directory = new Uri(lastPath);
            saveFileDialog.Directory = new Uri(lastPath);
        }

        private void AddLastFile(string filename)
        {
            if (enable_last)
            {
                if (!filename.EndsWith(".fpl"))
                    filename += ".fpl";

                lastFiles.RemoveAll(s => s == filename); // Doppelte Dateinamen verhindern
                lastFiles.Insert(0, filename);
                if (lastFiles.Count > 3) // Überlauf
                    lastFiles.RemoveAt(lastFiles.Count - 1);

                UpdateLastFilesMenu();
            }
        }

        private void ConvertTimetable()
        {
            IExport exp = (Timetable.Type == TimetableType.Linear) ? (IExport)new NetworkExport() : new LinearExport();
            string orig = (Timetable.Type == TimetableType.Linear) ? "Linear-Fahrplan" : "Netzwerk-Fahrplan";
            string dest = (Timetable.Type == TimetableType.Linear) ? "Netzwerk-Fahrplan" : "Linear-Fahrplan";

            if (MessageBox.Show($"Die aktuelle Datei ist ein {orig}. Es wird zu einem {dest} konvertiert.", "FPLedit", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            var sfd = new SaveFileDialog();
            sfd.AddLegacyFilter(exp.Filter);
            if (sfd.ShowDialog(this) == DialogResult.Ok)
            {
                Logger.Info("Konvertiere Datei...");
                bool ret = exp.Export(Timetable, sfd.FileName, this);
                if (ret == false)
                    return;
                Logger.Info("Konvertieren erfolgreich abgeschlossen!");
                InternalOpen(sfd.FileName);
            }
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Settings.KeyExists("restart.file"))
            {
                if (enable_last)
                    Settings.Set("files.last", string.Join(";", lastFiles));
                Settings.Set("files.lastpath", lastPath);
                if (!NotifyIfUnsaved())
                    e.Cancel = true;

                ClearTemp();
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

            if (!NotifyIfUnsaved())
                return;
            InternalOpen(files[0].LocalPath);

            base.OnDragDrop(e);
        }
        #endregion

        #region Backup & Undo
        public void Undo()
        {
            if (undo.CanGoBack)
                Timetable = undo.Undo();
        }

        public void StageUndoStep()
            => undo.StageUndoStep(Timetable);

        public void BackupTimetable()
        {
            timetableBackup = Timetable.Clone();
        }

        public void RestoreTimetable()
        {
            Timetable = timetableBackup;
            ClearBackup();
        }

        public void ClearBackup()
        {
            timetableBackup = null;
        }
        #endregion

        #region IInfo
        dynamic IInfo.Menu => Menu;

        public ITemplateManager TemplateManager => templateManager;

        public dynamic RootForm => this;

        public void Register<T>(T obj)
            => registry.Register<T>(obj);

        public T[] GetRegistered<T>()
            => registry.GetRegistered<T>();

        public string GetTemp(string filename)
        {
            var dirpath = Path.Combine(Path.GetTempPath(), "fpledit");
            if (!Directory.Exists(dirpath))
                Directory.CreateDirectory(dirpath);
            return Path.Combine(dirpath, filename);
        }

        private void ClearTemp()
        {
            var dirpath = Path.Combine(Path.GetTempPath(), "fpledit");
            if (Directory.Exists(dirpath))
                Directory.Delete(dirpath, true);
        }
        #endregion

        #region Events
        private void saveMenu_Click(object sender, EventArgs e)
            => Save(false);

        private void openMenu_Click(object sender, EventArgs e)
            => Open();

        private void saveAsMenu_Click(object sender, EventArgs e)
            => Save(true);

        private void importMenu_Click(object sender, EventArgs e)
            => Import();

        private void exportMenu_Click(object sender, EventArgs e)
            => Export();

        private void quitMenu_Click(object sender, EventArgs e)
            => Close();

        private void linearNewMenu_Click(object sender, EventArgs e)
            => New(TimetableType.Linear);

        private void networkNewMenu_Click(object sender, EventArgs e)
            => New(TimetableType.Network);

        private void convertMenu_Click(object sender, EventArgs e)
            => ConvertTimetable();
        #endregion
    }
}
