using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using FPLedit.Shared;
using System.ComponentModel;
using System.Drawing;
using FPLedit.Shared.Filetypes;
using System.Diagnostics;
using FPLedit.Logger;

namespace FPLedit
{
    public partial class MainForm : Form, IInfo, IRestartable
    {
        private Timetable timetableBackup = null;

        private IImport open;
        private IExport save;

        private FileState fileState;

        private ExtensionManager extensionManager;
        private UndoManager undo;
        private RegisterStore registry;

        private List<string> lastFiles;
        private bool enable_last = true;

        public Timetable Timetable { get; set; }

        public ILog Logger { get; private set; }

        public ISettings Settings { get; private set; }

        #region FileState

        public FileState FileState
        {
            get { return fileState; }
            set
            {
                if (!value.Equals(fileState))
                {
                    fileState = value;
                    OnFileStateChanged();
                }
            }
        }

        public void SetUnsaved()
        {
            AddUndoStep();
            fileState.Saved = false;
            OnFileStateChanged();
        }

        private void OnFileStateChanged()
        {
            fileState.LineCreated = Timetable?.Stations.Count > 1; // Mind. 2 Bahnhöfe
            fileState.TrainsCreated = Timetable?.Trains.Count > 0;
            fileState.CanGoBack = undo.CanGoBack;

            saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = exportToolStripMenuItem.Enabled = fileState.Opened;

            FileStateChanged?.Invoke(this, new FileStateChangedEventArgs(fileState));

            Text = "FPLedit - "
                + (fileState.FileName != null ? (Path.GetFileName(fileState.FileName) + " ") : "")
                + (fileState.Saved ? "" : "*");
        }

        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        public event EventHandler ExtensionsLoaded;

        #endregion

        public MainForm()
        {
            InitializeComponent();

            Settings = new Settings();
            undo = new UndoManager();
            registry = new RegisterStore();

            open = new XMLImport();
            save = new XMLExport();
            saveFileDialog.Filter = save.Filter;
            openFileDialog.Filter = open.Filter;

            fileState = new FileState();

            if (Settings.Get("log.enable-file", false))
                Logger = new MultipleLogger(logTextBox, new TempLogger(this));
            else
                Logger = new MultipleLogger(logTextBox);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Extensions laden & initialisieren (=> Initialisiert Importer/Exporter)
            extensionManager = new ExtensionManager(Logger, Settings);
            var enabled_plgs = extensionManager.Plugins.Where(p => p.Enabled);
            foreach (var plugin in enabled_plgs)
                plugin.TryInit(this);

            InitializeExportImport();
            InitializeMenus();

            ExtensionsLoaded?.Invoke(this, new EventArgs());

            // Parameter: Fpledit.exe [Dateiname] ODER Datei aus Restart
            string[] args = Environment.GetCommandLineArgs();
            string fn = args.Length >= 2 ? args[1] : null;
            fn = Settings.Get("restart.file", fn);
            if (fn != null && File.Exists(fn))
                InternalOpen(fn);
            Settings.Remove("restart.file");
        }

        private void InitializeExportImport()
        {
            var exporters = registry.GetRegistered<IExport>();
            var importers = registry.GetRegistered<IImport>();

            exportFileDialog.Filter = string.Join("|", exporters.Select(ex => ex.Filter));
            importFileDialog.Filter = string.Join("|", importers.Select(im => im.Filter));

            // Letzten Exporter auswählen
            int exporter_idx = Settings.Get("exporter.last", -1);
            if (exporter_idx > -1 && exporters.Length > exporter_idx)
                exportFileDialog.FilterIndex = exporter_idx + 1;
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
                    var itm = lastFilesToolStripMenuItem.DropDownItems.Add(lf);
                    itm.Click += (s, a) =>
                    {
                        if (!NotifyIfUnsaved())
                            return;
                        InternalOpen(lf);
                    };
                }
            }
            else
                lastFilesToolStripMenuItem.Visible = false;

            // Hilfe Menü nach den Erweiterungen zusammenbasteln
            var helpItem = new ToolStripMenuItem("Hilfe");
            this.menuStrip.Items.AddRange(new[] { helpItem });
            var extItem = helpItem.DropDownItems.Add("Erweiterungen");
            extItem.Click += (s, ev) => (new ExtensionsForm(extensionManager, this)).ShowDialog();
            var docItem = helpItem.DropDownItems.Add("Online Hilfe");
            docItem.Click += (s, ev) => Process.Start("https://fahrplan.manuelhu.de/");
            var infoItem = helpItem.DropDownItems.Add("Info");
            infoItem.Click += (s, ev) => (new InfoForm(Settings)).ShowDialog();
        }

        #region FileHandling

        private void Import()
        {
            if (!NotifyIfUnsaved())
                return;
            if (importFileDialog.ShowDialog() == DialogResult.OK)
            {
                var importers = GetRegistered<IImport>();
                IImport import = importers[importFileDialog.FilterIndex - 1];
                Logger.Info("Öffne Datei " + importFileDialog.FileName);
                Timetable = import.Import(importFileDialog.FileName, Logger);
                if (Timetable == null)
                    return;
                Logger.Info("Datei erfolgeich geöffnet!");
                fileState.Opened = true;
                fileState.Saved = true;
                fileState.FileName = importFileDialog.FileName;
                OnFileStateChanged();
                ClearHistory();
            }
        }

        private void Export()
        {
            if (exportFileDialog.ShowDialog() == DialogResult.OK)
            {
                var exporters = GetRegistered<IExport>();
                IExport export = exporters[exportFileDialog.FilterIndex - 1];
                string filename = exportFileDialog.FileName;

                Logger.Info("Speichere Datei " + filename);
                bool ret = export.Export(Timetable, filename, this);
                if (ret == false)
                    return;
                Logger.Info("Speichern erfolgreich abgeschlossen!");
                Settings.Set("exporter.last", exportFileDialog.FilterIndex - 1);
            }
        }

        public void Open()
        {
            if (!NotifyIfUnsaved())
                return;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
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
            OnFileStateChanged();
            ClearHistory();

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
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
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
            OnFileStateChanged();
        }

        private void New()
        {
            if (!NotifyIfUnsaved())
                return;
            Timetable = new Timetable();
            fileState.Opened = true;
            fileState.Saved = false;
            fileState.FileName = null;
            OnFileStateChanged();
            ClearHistory();
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
            this.FormClosing -= MainForm_FormClosing;
            if (fileState.Opened)
                Settings.Set("restart.file", fileState.FileName);
            Application.Restart();
        }

        #endregion

        #region Drag'n'Drop
        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length == 1 && files[0].EndsWith(".fpl"))
                e.Effect = DragDropEffects.Copy;
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files.Length != 1 || !files[0].EndsWith(".fpl"))
                return;

            if (!NotifyIfUnsaved())
                return;
            InternalOpen(files[0]);
        }
        #endregion

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (enable_last)
                Settings.Set("files.last", string.Join(";", lastFiles));
            if (!NotifyIfUnsaved())
                e.Cancel = true;

            ClearTemp();
        }

        #region AutoUpdates
        private void AutoUpdate_Check(object sender, EventArgs e)
        {
            if (Settings.Get("updater.auto", "") == "")
            {
                var res = MessageBox.Show("FPLedit kann automatisch bei jedem Programmstart nach einer aktuelleren Version suchen. Dabei wird nur die IP-Adresse Ihres Computers übermittelt.", "Automatische Updateprüfung", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

        #region Backup & Undo
        public void Undo()
        {
            if (undo.CanGoBack)
                Timetable = undo.Undo();
            OnFileStateChanged();
        }

        public void StageUndoStep()
            => undo.StageUndoStep(Timetable);

        public void AddUndoStep()
        {
            undo.AddUndoStep();
            OnFileStateChanged();
        }

        private void ClearHistory()
        {
            undo.ClearHistory();
            OnFileStateChanged();
        }

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
        dynamic IInfo.Menu => menuStrip;

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

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
            => New();

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
            => Save(false);

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
            => Open();

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
            => Save(true);

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
            => Import();

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
            => Export();

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
            => Close();

        #endregion
    }
}
