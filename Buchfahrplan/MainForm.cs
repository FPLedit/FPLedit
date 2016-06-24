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
        public Timetable Timetable { get; private set; }

        public bool FileOpened
        {
            get
            {
                return fileOpened;
            }
            private set
            {
                if (value != fileOpened)
                {
                    fileOpened = value;
                    if (FileStateChanged != null)
                        FileStateChanged(this, new FileStateChangedEventArgs(fileOpened, fileSaved));
                }
            }
        }

        public bool FileSaved
        {
            get
            {
                return fileSaved;
            }
            private set
            {
                if (value != fileOpened)
                {
                    fileSaved = value;
                    if (FileStateChanged != null)
                        FileStateChanged(this, new FileStateChangedEventArgs(fileOpened, fileSaved));
                }
            }
        }        

        List<IExport> exporters;
        List<IImport> importers;
        List<IPlugin> plugins;

        private bool lineCreated = false;
        private bool trainsCreated = false;
        private bool fileSaved;
        private bool fileOpened;

        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            exporters = ExtensionManager.GetInstances<IExport>().ToList();
            foreach (var export in exporters)
                saveFileDialog.Filter += saveFileDialog.Filter == "" ? export.Filter : "|" + export.Filter;

            importers = ExtensionManager.GetInstances<IImport>().ToList();
            foreach (var import in importers)
                openFileDialog.Filter += openFileDialog.Filter == "" ? import.Filter : "|" + import.Filter;

            plugins = ExtensionManager.GetInstances<IPlugin>().ToList();
            foreach (var plugin in plugins)
                plugin.Init(this);

            UpdateButtonsEnabled();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
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
                    FileSaved = true;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                IImport import = importers[openFileDialog.FilterIndex - 1];
                logger.Log("Öffne Datei...");
                Timetable = import.Import(openFileDialog.FileName, logger);
                if (Timetable == null)
                    return;
                logger.Log("Datei erfolgeich geöffnet!");
                FileOpened = true;
                FileSaved = true;
                UpdateButtonsEnabled();
            }
        }

        private void UpdateButtonsEnabled()
        {
            if (Timetable != null)
            {
                lineCreated = Timetable.Stations.Count > 0;
                trainsCreated = Timetable.Trains.Count > 0;
            }
            else
            {
                lineCreated = false;
                trainsCreated = false;
            }

            saveToolStripMenuItem.Enabled = FileOpened;
            editLineToolStripMenuItem.Enabled = FileOpened;
            editToolStripMenuItem.Enabled = FileOpened & lineCreated;
            editTimetableToolStripMenuItem.Enabled = FileOpened & trainsCreated;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Timetable = new Timetable();
            FileOpened = true;
            UpdateButtonsEnabled();
            FileSaved = false;
        }

        #region InitEditDialogs

        private void editTrainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var trEdit = new TrainsEditForm();
            trEdit.Init(Timetable);
            trEdit.ShowDialog();
            FileSaved = false;
            UpdateButtonsEnabled();
        }

        private void editLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var liEdit = new LineEditForm();
            liEdit.Init(Timetable.Stations, Timetable.Trains);
            liEdit.ShowDialog();
            FileSaved = false;
            UpdateButtonsEnabled();
        }

        private void editTimetableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ttEdit = new TimetableEditForm();
            ttEdit.Init(Timetable);
            ttEdit.ShowDialog();
            FileSaved = false;
            UpdateButtonsEnabled();
        }
        #endregion

        #region IInfo
        dynamic IInfo.Menu
        {
            get
            {
                return menuStrip;
            }
        }

        public dynamic ShowDialog(dynamic form)
        {
            return form.ShowDialog();
        }
        #endregion
    }
}
