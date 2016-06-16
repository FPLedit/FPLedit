using System;
using System.IO;
using System.Windows.Forms;
using Buchfahrplan.Properties;
using System.Linq;
using System.Collections.Generic;
using Buchfahrplan.Shared;

namespace Buchfahrplan
{
    public partial class Form1 : Form
    {
        private Timetable tt;

        private TrainEditForm trEdit;
        private LineEditForm liEdit;
        private TimetableEditForm ttEdit;

        private bool fileOpened = false;
        private bool fileSaved = false;
        private bool lineCreated = false;
        private bool trainsCreated = false;

        List<IExport> exporters;
        List<IImport> importers;

        public Form1()
        {
            InitializeComponent();

            this.Icon = Resources.programm;

            trEdit = new TrainEditForm();
            liEdit = new LineEditForm();
            ttEdit = new TimetableEditForm();

            string[] args = Environment.GetCommandLineArgs();

            /*if (args.Length == 2)
            {
                string filename = args[1];
                OpenFile(filename);
            }*/

            exporters = new List<IExport>();
            importers = new List<IImport>();
        }       

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (var export in ExtensionManager.GetInstances<IExport>())
            {
                exportFileDialog.Filter += exportFileDialog.Filter == "" ? export.Filter : "|" + export.Filter;
                exporters.Add(export);
            }

            foreach (var import in ExtensionManager.GetInstances<IImport>())
            {
                openFileDialog.Filter += openFileDialog.Filter == "" ? import.Filter : "|" + import.Filter;
                importers.Add(import);
            }

            UpdateButtonsEnabled();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = exportFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                IExport export = exporters[exportFileDialog.FilterIndex - 1];
                logger.Log("Speichere...");
                bool ret = export.Export(tt, exportFileDialog.FileName, logger);
                if (ret == false)
                    return;
                logger.Log("Speichern erfolgreich abgeschlossen!");
                if (export.Reoppenable)
                    fileSaved = true;
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                IImport import = importers[openFileDialog.FilterIndex - 1];
                logger.Log("Öffne Datei...");
                tt = import.Import(openFileDialog.FileName, logger);
                if (tt == null)
                    return;
                logger.Log("Datei erfolgeich geöffnet!");
                fileOpened = true;
                fileSaved = true;
                UpdateButtonsEnabled();
            }
        }

        private void UpdateButtonsEnabled()
        {
            if (tt != null)
            {
                lineCreated = tt.Stations.Count > 0;
                trainsCreated = tt.Trains.Count > 0;
            }
            else
            {
                lineCreated = false;
                trainsCreated = false;
            }

            saveToolStripMenuItem.Enabled = fileOpened;

            editLineToolStripMenuItem.Enabled = fileOpened;

            editToolStripMenuItem.Enabled = fileOpened;
            editToolStripMenuItem.Enabled &= lineCreated;

            editTimetableToolStripMenuItem.Enabled = fileOpened;
            editTimetableToolStripMenuItem.Enabled &= trainsCreated;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tt = new Timetable();
            fileOpened = true;
            UpdateButtonsEnabled();
            fileSaved = false;
        }

#region InitEditDialogs

        private void editTrainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trEdit.Init(tt);
            trEdit.ShowDialog();
            fileSaved = false;
            UpdateButtonsEnabled();
        }

        private void editLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            liEdit.Init(tt.Stations, tt.Trains);
            liEdit.ShowDialog();
            fileSaved = false;
            UpdateButtonsEnabled();
        }

        private void editTimetableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ttEdit.Init(tt);
            ttEdit.ShowDialog();
            fileSaved = false;
            UpdateButtonsEnabled();
        }

#endregion
    }
}
