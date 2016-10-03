using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using Buchfahrplan.Shared;
using System.ComponentModel;

namespace Buchfahrplan
{
    public partial class MainForm : Form, IInfo
    {        
        public Timetable Timetable { get; set; }

        private Timetable timetableBackup = null;

        private List<IExport> exporters;
        private List<IImport> importers;

        private FileState fileState;

        public FileState FileState
        {
            get
            {
                return fileState;
            }
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

            saveToolStripMenuItem.Enabled = fileState.Opened;

            FileStateChanged?.Invoke(this, new FileStateChangedEventArgs(fileState));

            Text = "Buchfahrplan - " 
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
            foreach (var plugin in ExtensionManager.Plugins)
                plugin.Init(this);

            saveFileDialog.Filter = string.Join("|", exporters.Select(ex => ex.Filter));
            openFileDialog.Filter = string.Join("|", importers.Select(im => im.Filter));
        }        

        private void Open()
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                IImport import = importers[openFileDialog.FilterIndex - 1];
                logger.Log("Öffne Datei...");
                Timetable = import.Import(openFileDialog.FileName, logger);
                if (Timetable == null)
                    return;
                logger.Log("Datei erfolgeich geöffnet!");
                fileState.Opened = true;
                fileState.Saved = true;
                fileState.FileName = openFileDialog.FileName;
                OnFileStateChanged();
            }
        }

        private void Save()
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                IExport export = exporters[saveFileDialog.FilterIndex - 1];
                logger.Log("Speichere...");
                bool ret = export.Export(Timetable, saveFileDialog.FileName, logger);
                if (ret == false)
                    return;
                logger.Log("Speichern erfolgreich abgeschlossen!");
                if (export.Reoppenable)
                {
                    fileState.Saved = true;
                    fileState.FileName = saveFileDialog.FileName;
                    OnFileStateChanged();
                }
            }
        }        

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Timetable = new Timetable();
            fileState.Opened = true;
            fileState.Saved = false;
            fileState.FileName = null;
            OnFileStateChanged();
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

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!fileState.Saved && fileState.Opened)
            {
                DialogResult res = MessageBox.Show("Wollen Sie die Änderungen speichern?", "Buchfahrplan", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    e.Cancel = true;
                    Save();
                }
                else if (res == DialogResult.Cancel)
                    e.Cancel = true;
            }
        }

        private void infoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string nl = Environment.NewLine;
            MessageBox.Show($"Fahrplan{nl}{nl}© 2015-2016 Manuel Huber{nl}https://www.manuelhu.de{nl}https://github.com/ManuelHu",
                "Buchfahrplan Info", MessageBoxButtons.OK, MessageBoxIcon.Information);            
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
            => Save();

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
            => Open();
    }
}
