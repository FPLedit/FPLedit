using Buchfahrplan.FileModel;
using System;
using System.IO;
using System.Windows.Forms;
using Buchfahrplan.Export;
using Buchfahrplan.Properties;

namespace Buchfahrplan
{
    public partial class Form1 : Form
    {
        private Timetable tt;

        private NewEditForm newEdit;
        private TrainEditForm trEdit;
        private LineEditForm liEdit;
        private TimetableEditForm ttEdit;

        private bool fileOpened = false;
        private bool fileSaved = false;

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
            DialogResult result = excelSaveFileDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                statusLabel.Text = "Exportiere...";
                logTextBox.Log("Exportiere...");
                ExcelExport.Export(tt, excelSaveFileDialog.FileName, ExportFileType.XlFile);
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
                tt = FplImport.Import(filename);

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
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            statusLabel.Text = "";
            logTextBox.Log("Datei erfolgeich geöffnet!");
            fileOpened = true;
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
            saveToolStripMenuItem.Enabled = fileOpened;
            exportToolStripMenuItem.Enabled = fileOpened;
            editToolStripMenuItem.Enabled = fileOpened;
            editLineToolStripMenuItem.Enabled = fileOpened;
            editTimetableToolStripMenuItem.Enabled = fileOpened;
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

#region InitEditDialogs

        private void editTrainsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            trEdit.Init(tt.Trains);
            trEdit.ShowDialog();
        }

        private void editLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            liEdit.Init(tt.Stations, tt.Trains);
            liEdit.ShowDialog();
        }

        private void editTimetableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ttEdit.Init(tt);
            ttEdit.ShowDialog();
        }

#endregion
    }
}
