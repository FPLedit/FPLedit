using Buchfahrplan.FileModel;
using System;
using System.IO;
using System.Windows.Forms;
using Buchfahrplan.Export;
using Buchfahrplan.Properties;
using Buchfahrplan.Import;
using Buchfahrplan.BuchfahrplanExport;

namespace Buchfahrplan
{
    public partial class Form1 : Form
    {
        private Timetable tt;

        private NewEditForm newEdit;
        private TrainEditForm trEdit;
        private LineEditForm liEdit;
        private TimetableEditForm ttEdit;

        private IExport export;

        private bool fileOpened = false;
        private bool fileSaved = false;
        private bool lineCreated = false;
        private bool trainsCreated = false;

        public Form1()
        {
            InitializeComponent();

            this.Icon = Resources.programm;

            newEdit = new NewEditForm();
            trEdit = new TrainEditForm();
            liEdit = new LineEditForm();
            ttEdit = new TimetableEditForm();

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length == 2)
            {
                string filename = args[1];
                OpenFile(filename);
            }
        }       

        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateButtonsEnabled();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
#if OFFICE
            export = new ExcelExport();
#else
            export = new HtmlExport();
#endif

            DialogResult result = excelSaveFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                statusLabel.Text = "Exportiere...";
                logTextBox.Log("Exportiere...");
                export.Export(tt, excelSaveFileDialog.FileName);
                statusLabel.Text = "";
                logTextBox.Log("Exportieren erfolgreich abgeschlossen!");
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = bfplSaveFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                SaveFile(bfplSaveFileDialog.FileName);
            }            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = fplOpenFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                OpenFile(fplOpenFileDialog.FileName);
            }
        }

        private void OpenFile(string filename)
        {
            statusLabel.Text = "Öffne Datei...";
            logTextBox.Log("Öffne Datei...");
            if (Path.GetExtension(filename) == ".fpl")
            {
                try
                {
                    tt = FplImport.Import(filename);
                }
                catch (ImportException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }

                newEdit.Init(tt.Trains);
                DialogResult res = newEdit.ShowDialog();
                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    tt.Trains = newEdit.trains;
                }
            }
            else
            {
                try
                {
                    tt = Timetable.OpenFromFile(filename);
                }
                catch (ExportException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
            statusLabel.Text = "";
            logTextBox.Log("Datei erfolgeich geöffnet!");
            fileOpened = true;
            fileSaved = true;
            UpdateButtonsEnabled();
        }

        private void SaveFile(string filename)
        {
            statusLabel.Text = "Speichere Datei...";
            logTextBox.Log("Speichere Datei...");
            try
            {
                tt.SaveToFile(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            statusLabel.Text = "";
            logTextBox.Log("Datei erfolgreich gespeichert!");
            fileSaved = true;
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
            saveToolStripMenuItem.Enabled &= fileSaved;

            exportToolStripMenuItem.Enabled = fileOpened;            

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
