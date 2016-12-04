using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using FPLedit.Shared;
using System.ComponentModel;
using System.Drawing;

namespace FPLedit
{
    public partial class MainForm : Form, IInfo
    {        
        public Timetable Timetable { get; set; }

        private Timetable timetableBackup = null;

        private List<IExport> exporters;
        private List<IImport> importers;
        private IExport lastExport;

        private FileState fileState;

        private ExtensionManager extensionManager;

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

            saveToolStripMenuItem.Enabled = saveAsToolStripMenuItem.Enabled = fileState.Opened;

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

            fileState = new FileState();            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            extensionManager = new ExtensionManager();
            foreach (var plugin in extensionManager.EnabledPlugins)
                plugin.Init(this);

            saveFileDialog.Filter = string.Join("|", exporters.Select(ex => ex.Filter));
            openFileDialog.Filter = string.Join("|", importers.Select(im => im.Filter));            
        }

        private void Open()
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
            {
                IImport import = importers[openFileDialog.FilterIndex - 1];
                logger.Info("Öffne Datei " + openFileDialog.FileName);
                Timetable = import.Import(openFileDialog.FileName, logger);
                if (Timetable == null)
                    return;
                logger.Info("Datei erfolgeich geöffnet!");                
                fileState.Opened = true;
                fileState.Saved = true;
                fileState.FileName = openFileDialog.FileName;
                OnFileStateChanged();
            }
        }

        private void Save(bool forceSaveAs)
        {
            IExport export = lastExport;
            string filename = fileState.FileName;

            bool saveAs = forceSaveAs || export == null || filename == null || filename == "";

            if (saveAs)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    export = exporters[saveFileDialog.FilterIndex - 1];
                    filename = saveFileDialog.FileName;
                }
                else
                    return;
            }

            logger.Info("Speichere Datei " + filename);
            bool ret = export.Export(Timetable, filename, logger);
            if (ret == false)
                return;
            logger.Info("Speichern erfolgreich abgeschlossen!");
            if (export.Reoppenable)
            {
                fileState.Saved = true;
                fileState.FileName = filename;
                OnFileStateChanged();
            }
            lastExport = export.Reoppenable ? export : null;
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
            logger.Info("Neue Datei erstellt");
        }

        #region IInfo
        dynamic IInfo.Menu
        {
            get { return menuStrip; }
        }

        public dynamic ShowDialog(dynamic form)
        {
            return form.ShowDialog();
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
    }
}
