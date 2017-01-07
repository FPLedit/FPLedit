using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using FPLedit.Shared;
using System.ComponentModel;
using System.Drawing;
using FPLedit.Shared.Logger;
using FPLedit.Shared.Filetypes;

namespace FPLedit
{
    public partial class MainForm : Form, IInfo
    {
        public Timetable Timetable { get; set; }

        private Timetable timetableBackup = null;

        private List<IExport> exporters;
        private List<IImport> importers;

        private IImport open;
        private IExport save;

        private FileState fileState;

        private ExtensionManager extensionManager;

        public ILog Logger { get; private set; }

        public FileState FileState
        {
            get { return fileState; }
            set
            {
                if (value != fileState)
                {
                    fileState = value;
                    OnFileStateChanged();
                }
            }
        }

        public void SetUnsaved()
        {
            fileState.Saved = false;
            OnFileStateChanged();
        }

        private void OnFileStateChanged()
        {
            fileState.LineCreated = Timetable?.Stations.Count > 0;
            fileState.TrainsCreated = Timetable?.Trains.Count > 0;

            saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = exportToolStripMenuItem.Enabled = fileState.Opened;

            FileStateChanged?.Invoke(this, new FileStateChangedEventArgs(fileState));

            Text = "FPLedit - "
                + (fileState.FileName != null ? (Path.GetFileName(fileState.FileName) + " ") : "")
                + (fileState.Saved ? "" : "*");
        }

        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;

        public MainForm()
        {
            InitializeComponent();
            exporters = new List<IExport>();
            importers = new List<IImport>();

            open = new XMLImport();
            save = new XMLExport();

            fileState = new FileState();
            Logger = new MultipleLogger(logTextBox);
            //logger.Loggers.Add(new ConsoleLogger());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Extensions laden & initialisieren (=> Initialisiert Importer/Exporter)
            extensionManager = new ExtensionManager();
            foreach (var plugin in extensionManager.EnabledPlugins)
                plugin.Init(this);

            saveFileDialog.Filter = save.Filter;
            openFileDialog.Filter = open.Filter;

            exportFileDialog.Filter = string.Join("|", exporters.OrderByDescending(ex => ex.Reoppenable).Select(ex => ex.Filter));
            importFileDialog.Filter = string.Join("|", importers.Select(im => im.Filter));

            // Parameter Fpledit.exe [Dateiname]
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2 && File.Exists(args[2]))
                InternalOpen(args[2]);
        }

        #region FileHandling

        public void Import()
        {
            if (!fileState.Saved && fileState.Opened)
            {
                DialogResult res = NotifyChanged();
                if (res == DialogResult.Yes)
                    Save(false);
                if (res == DialogResult.Cancel)
                    return;
            }
            if (importFileDialog.ShowDialog() == DialogResult.OK)
            {
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
            }
        }

        public void Open()
        {
            if (!fileState.Saved && fileState.Opened)
            {
                DialogResult res = NotifyChanged();
                if (res == DialogResult.Yes)
                    Save(false);
                if (res == DialogResult.Cancel)
                    return;
            }
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                InternalOpen(openFileDialog.FileName);
        }

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
        }

        public void Export()
        {
            if (exportFileDialog.ShowDialog() == DialogResult.OK)
            {
                IExport export = exporters[exportFileDialog.FilterIndex - 1];
                string filename = exportFileDialog.FileName;

                Logger.Info("Speichere Datei " + filename);
                bool ret = export.Export(Timetable, filename, Logger);
                if (ret == false)
                    return;
                Logger.Info("Speichern erfolgreich abgeschlossen!");
                if (export.Reoppenable)
                {
                    fileState.Saved = true;
                    fileState.FileName = filename;
                    OnFileStateChanged();
                }
            }
        }

        public void Save(bool forceSaveAs)
        {
            string filename = fileState.FileName;

            bool saveAs = forceSaveAs || filename == null || filename == "";

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
            bool ret = save.Export(Timetable, filename, Logger);
            if (ret == false)
                return;
            Logger.Info("Speichern erfolgreich abgeschlossen!");
            fileState.Saved = true;
            fileState.FileName = filename;
            OnFileStateChanged();
        }

        public void Reload()
        {
            InternalOpen(fileState.FileName);
        }

        private void New()
        {
            if (!fileState.Saved && fileState.Opened)
            {
                DialogResult res = NotifyChanged();
                if (res == DialogResult.Yes)
                    Save(false);
                if (res == DialogResult.Cancel)
                    return;
            }
            Timetable = new Timetable();
            fileState.Opened = true;
            fileState.Saved = false;
            fileState.FileName = null;
            OnFileStateChanged();
            Logger.Info("Neue Datei erstellt");
        }

        #endregion

        #region IInfo
        dynamic IInfo.Menu
        {
            get { return menuStrip; }
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

        public void RegisterExport(IExport export)
            => exporters.Add(export);

        public void RegisterImport(IImport import)
            => importers.Add(import);
        #endregion        

        private void Info()
        {
            (new InfoForm()).ShowDialog();
        }

        private DialogResult NotifyChanged()
        {
            return MessageBox.Show("Wollen Sie die Änderungen speichern?",
                "FPLedit",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!fileState.Saved && fileState.Opened)
            {
                DialogResult res = NotifyChanged();
                if (res == DialogResult.Yes)
                {
                    e.Cancel = true;
                    Save(false);
                }
                else if (res == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        #region Events

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
            => New();

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
            => Info();

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
            => Save(false);

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
            => Open();

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
            => Save(true);

        private void extensionsToolStripMenuItem_Click(object sender, EventArgs e)
            => (new ExtensionsForm(extensionManager)).ShowDialog();

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
            => Import();

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
            => Export();

        #endregion
    }
}
