using Eto.Forms;
using FPLedit.Properties;
using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;
using FPLedit.Shared;
using FPLedit.Templating;
using FPLedit.Shared.Filetypes;
using FPLedit.Logger;
using FPLedit.Shared.Templating;
using System.ComponentModel;
using System.Diagnostics;
using FPLedit.Editor.Network;
using FPLedit.Shared.UI;

namespace FPLedit
{
    public class MainForm : Form, IInfo, IRestartable
    {
        #region Controls
#pragma warning disable CS0649
        private LogControl logTextBox;
        private ButtonMenuItem saveMenu, saveAsMenu, exportMenu, lastMenu;
        private SaveFileDialog saveFileDialog, exportFileDialog;
        private OpenFileDialog openFileDialog, importFileDialog;
        private LineEditingControl lineEditingControl;
#pragma warning restore CS0649
        #endregion

        private const string TEMPLATE_PATH = "templates";

        private Timetable timetableBackup = null;

        private IImport open;
        private IExport save;

        private FileState fileState;
        private TemplateManager templateManager;
        private ExtensionManager extensionManager;
        private UndoManager undo;
        private RegisterStore registry;

        private List<string> lastFiles;
        private bool enable_last = true;

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

            saveMenu.Enabled = saveAsMenu.Enabled = exportMenu.Enabled = fileState.Opened;

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

            if (Settings.Get("log.enable-file", false))
                Logger = new MultipleLogger(logTextBox, new TempLogger(this));
            else
                Logger = new MultipleLogger(logTextBox);

            Init();
        }

        #region Initialization

        private void Init()
        {
            // Extensions laden & initialisieren (=> Initialisiert Importer/Exporter)
            new Editor.EditorPlugin().Init(this);

            extensionManager = new ExtensionManager(Logger, Settings, new UpdateManager(Settings));
            var enabled_plgs = extensionManager.Plugins.Where(p => p.Enabled);
            foreach (var plugin in enabled_plgs)
                plugin.TryInit(this);

            InitializeExportImport();
            InitializeMenus();

            // Vorlagen laden
            templateManager = new TemplateManager(registry, Logger);
            templateManager.LoadTemplates(TEMPLATE_PATH);

            ExtensionsLoaded?.Invoke(this, new EventArgs());

            Shown += LoadStartFile;
        }

        private void LoadStartFile(object sender, EventArgs e)
        {
            // Parameter: Fpledit.exe [Dateiname] ODER Datei aus Restart
            string[] args = Environment.GetCommandLineArgs();
            string fn = args.Length >= 2 ? args[1] : null;
            fn = Settings.Get("restart.file", fn);
            if (fn != null && File.Exists(fn))
                InternalOpen(fn);
            Settings.Remove("restart.file");
            Shown -= LoadStartFile;
        }

        private void InitializeExportImport()
        {
            var exporters = registry.GetRegistered<IExport>();
            var importers = registry.GetRegistered<IImport>();

            exporters.ToList().ForEach(ex => exportFileDialog.AddLegacyFilter(ex.Filter));
            importers.ToList().ForEach(im => importFileDialog.AddLegacyFilter(im.Filter));

            // Letzten Exporter auswählen
            int exporter_idx = Settings.Get("exporter.last", -1);
            if (exporter_idx > -1 && exporters.Length > exporter_idx)
                exportFileDialog.CurrentFilterIndex = exporter_idx + 1;
        }

        private void InitializeMenus()
        {
            // Zuletzt geöffnete Dateien anzeigen
            enable_last = Settings.Get("files.save-last", true);
            if (enable_last)
            {
                lastFiles = Settings.Get("files.last", "").Split(';').Where(s => s != "").Reverse().ToList();
                foreach (var lf in lastFiles)
                {
                    var itm = lastMenu.CreateItem(lf);
                    itm.Click += (s, a) =>
                    {
                        if (!NotifyIfUnsaved())
                            return;
                        InternalOpen(lf);
                    };
                }
            }
            else
                lastMenu.Enabled = false;

            // Hilfe Menü nach den Erweiterungen zusammenbasteln
            var helpItem = Menu.CreateItem("Hilfe");
            var extItem = helpItem.CreateItem("Erweiterungen");
            extItem.Click += (s, ev) => (new ExtensionsForm(extensionManager, this)).ShowModal(this);
            var tmplItem = helpItem.CreateItem("Vorlagen");
            tmplItem.Click += (s, ev) => (new TemplatesForm(templateManager, TEMPLATE_PATH)).ShowModal(this);
            helpItem.Items.Add(new SeparatorMenuItem());
            var docItem = helpItem.CreateItem("Online Hilfe");
            docItem.Click += (s, ev) => Process.Start("https://fahrplan.manuelhu.de/");
            var infoItem = helpItem.CreateItem("Info");
            infoItem.Click += (s, ev) => (new InfoForm(Settings)).ShowModal(this);
        }

        #endregion

        #region FileHandling

        private void Import()
        {
            if (!NotifyIfUnsaved())
                return;
            if (importFileDialog.ShowDialog(this) == DialogResult.Ok)
            {
                var importers = registry.GetRegistered<IImport>();
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
                IExport export = exporters[exportFileDialog.CurrentFilterIndex - 1];
                string filename = exportFileDialog.FileName;

                Logger.Info("Speichere Datei " + filename);
                bool ret = export.Export(Timetable, filename, this);
                if (ret == false)
                    return;
                Logger.Info("Speichern erfolgreich abgeschlossen!");
                Settings.Set("exporter.last", exportFileDialog.CurrentFilterIndex - 1);
            }
        }

        public void Open()
        {
            if (!NotifyIfUnsaved())
                return;
            if (openFileDialog.ShowDialog(this) == DialogResult.Ok)
                InternalOpen(openFileDialog.FileName);
        }

        public void Reload()
            => InternalOpen(fileState.FileName);

        private void InternalOpen(string filename)
        {
            Logger.Info("Öffne Datei " + filename);
            Timetable = open.Import(filename, Logger);
            if (Timetable == null)
                return;
            Logger.Info("Datei erfolgeich geöffnet!");
            fileState.Opened = true;
            fileState.Saved = true;
            fileState.FileName = filename;
            undo.ClearHistory();

            if (enable_last)
            {
                lastFiles.RemoveAll(s => s == filename); // Doppelte Dateinamen verhindern
                lastFiles.Add(filename);
                if (lastFiles.Count > 3) // Überlauf
                    lastFiles.RemoveAt(0);
            }
        }

        public void Save(bool forceSaveAs)
        {
            string filename = fileState.FileName;

            bool saveAs = forceSaveAs || filename == null || filename == "" || Path.GetExtension(filename) != ".fpl";

            if (saveAs)
            {
                if (saveFileDialog.ShowDialog(this) == DialogResult.Ok)
                    filename = saveFileDialog.FileName;
                else
                    return;
            }
            InternalSave(filename);
        }

        private void InternalSave(string filename)
        {
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

        #endregion

        #region AutoUpdates
        private void AutoUpdate_Check(object sender, EventArgs e)
        {
            // Beispiele für fehlende Funktionen
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                Logger.Warning("Sie verwenden FPLedit nicht auf Windows. Grundsätzlich ist FPLedit zwar mit allen Systemen kompatibel, auf denen Mono läuft, hat aber Einschränkungen in den Funktionen und möglichen Sicherheitsvorkehrungen und ist möglicherweise nicht getestet.");

            if (Settings.Get("updater.auto", "") == "")
            {
                var res = MessageBox.Show("FPLedit kann automatisch bei jedem Programmstart nach einer aktuelleren Version suchen. Dabei wird nur die IP-Adresse Ihres Computers übermittelt.", "Automatische Updateprüfung", MessageBoxButtons.YesNo, MessageBoxType.Question);
                Settings.Set("updater.auto", (res == DialogResult.Yes));
            }

            UpdateManager mg = new UpdateManager(Settings);
            if (!mg.AutoUpdateEnabled)
                return;

            mg.CheckResult = vi =>
            {
                if (vi != null)
                    Logger.Info($"Eine neue Programmversion ({vi.NewVersion.ToString()}) ist verfügbar! {vi.Description ?? ""} Hier herunterladen: {vi.DownloadUrl}");
                else
                    Logger.Info($"Sie benutzen die aktuelleste Version von FPLedit ({mg.GetCurrentVersion().ToString()})!");
            };

            mg.TextResult = t => Logger.Info(t);

            mg.CheckAsync();
        }
        #endregion

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Settings.KeyExists("restart.file"))
            {
                if (enable_last)
                    Settings.Set("files.last", string.Join(";", lastFiles));
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
            if (files.Length != 1 || !files[0].AbsolutePath.EndsWith(".fpl"))
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

        #endregion
    }
}
