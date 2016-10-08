using System;
using System.IO;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using FPLedit.Shared;
using System.ComponentModel;
using System.Net;
using System.Xml;
using System.Diagnostics;
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
            foreach (var plugin in ExtensionManager.Plugins)
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

        private void VersionCheck()
        {
            WebClient wc = new WebClient();
            try
            {
                string ret = wc.DownloadString(string.Format(SettingsManager.Get("CheckUrl"), GetVersion()));
                if (ret != "")
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(ret);

                    XmlNode ver = doc.DocumentElement.SelectSingleNode("/info/version");
                    XmlNode url = doc.DocumentElement.SelectSingleNode("/info/url");

                    string nl = Environment.NewLine;
                    DialogResult res = MessageBox.Show($"Eine neue Programmversion ({ver.InnerText}) ist verfügbar!{nl}{nl}Jetzt zur Download-Seite wechseln, um die neue Version herunterzuladen?",
                        "Neue FPLedit-Version verfügbar", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                    if (res == DialogResult.Yes)
                        Process.Start(url.InnerText);
                }
                else
                {
                    MessageBox.Show($"Sie benutzen bereits die aktuelle Version!",
                        "Auf neue Version prüfen");
                }
            }
            catch
            {
                MessageBox.Show($"Verbindung mit dem Server fehlgeschlagen!",
                    "Auf neue Version prüfen");
            }
        }

        private string GetVersion()
        {
            return "alpha";
        }

        private void Info()
        {            
            string nl = Environment.NewLine;
            string version = GetVersion() + " (Zur Überprüfung auf aktuellere Programmversionen bitte den Menüpunkt \"Auf neue Version prüfen\" verwenden)"; //TODO
            MessageBox.Show($"FPLedit{nl}{nl}Version {version}{nl}{nl}© 2015-2016 Manuel Huber{nl}https://www.manuelhu.de{nl}https://github.com/ManuelHu",
                "FPLedit Info",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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

        private void checkVersionToolStripMenuItem_Click(object sender, EventArgs e)
            => VersionCheck();

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
            => Save(true);
    }
}
