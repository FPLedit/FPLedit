using Eto.Forms;
using FPLedit.NonDefaultFiletypes;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;
using System;
using System.IO;
using System.Linq;

namespace FPLedit
{
    internal class FileHandler
    {
        private readonly SaveFileDialog saveFileDialog, exportFileDialog;
        private readonly OpenFileDialog openFileDialog, importFileDialog;

        private readonly IImport open;
        private readonly IExport save;

        private readonly Window parent;
        private readonly IInfo info;
        private readonly ILastFileHandler lfh;
        private readonly UndoManager undo;

        public Timetable Timetable { get; set; }
        public FileState FileState { get; }

        public event EventHandler FileOpened;
        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;

        public FileHandler(Window parent, IInfo info, ILastFileHandler lfh, UndoManager undo)
        {
            this.parent = parent;
            this.info = info;
            this.lfh = lfh;
            this.undo = undo;

            FileState = new FileState();
            FileState.FileStateInternalChanged += (s, e) => OnFileStateChanged();

            open = new XMLImport();
            save = new XMLExport();

            saveFileDialog = new SaveFileDialog();
            openFileDialog = new OpenFileDialog();
            exportFileDialog = new SaveFileDialog();
            importFileDialog = new OpenFileDialog();

            saveFileDialog.AddLegacyFilter(save.Filter);
            openFileDialog.AddLegacyFilter(open.Filter);

            FileOpened += (s, e) => MaybeUpgradeTtVersion();

            SetLastPath(info.Settings.Get("files.lastpath", ""));
        }

        #region FileState
        public void SetUnsaved()
        {
            undo.AddUndoStep();
            FileState.Saved = false;
        }

        private void OnFileStateChanged()
        {
            FileState.UpdateMetaProperties(Timetable, undo);
            FileStateChanged?.Invoke(this, new FileStateChangedEventArgs(FileState));
        }
        #endregion

        #region Export / Import
        internal void Import()
        {
            var importers = info.GetRegistered<IImport>();

            if (importers.Length == 0)
            {
                info.Logger.Error("Keine Importer gefunden, Import nicht möglich!");
                return;
            }

            if (!NotifyIfUnsaved())
                return;
            if (importFileDialog.ShowDialog(parent) == DialogResult.Ok)
            {
                IImport import = importers[importFileDialog.CurrentFilterIndex - 1];
                info.Logger.Info("Öffne Datei " + importFileDialog.FileName);
                Timetable = import.Import(importFileDialog.FileName, info);
                if (Timetable == null)
                    return;
                info.Logger.Info("Datei erfolgeich geöffnet!");
                FileState.Opened = true;
                FileState.Saved = true;
                FileState.FileName = importFileDialog.FileName;
                undo.ClearHistory();
            }
        }

        internal void Export()
        {
            if (exportFileDialog.ShowDialog(parent) == DialogResult.Ok)
            {
                var exporters = info.GetRegistered<IExport>();
                IExport export = exporters[exportFileDialog.CurrentFilterIndex];
                string filename = exportFileDialog.FileName;

                info.Logger.Info("Exportiere in Datei " + filename);
                bool ret = export.Export(Timetable, filename, info);
                if (ret == false)
                {
                    info.Logger.Error("Exportieren fehlgeschlagen!");
                    return;
                }
                info.Logger.Info("Exportieren erfolgreich abgeschlossen!");
                info.Settings.Set("exporter.last", exportFileDialog.CurrentFilterIndex);
            }
        }

        internal void InitializeExportImport(IExport[] exporters, IImport[] importers)
        {
            exportFileDialog.AddLegacyFilter(exporters.Select(ex => ex.Filter).ToArray());
            importFileDialog.AddLegacyFilter(importers.Select(im => im.Filter).ToArray());

            // Letzten Exporter auswählen
            int exporter_idx = info.Settings.Get("exporter.last", -1);
            if (exporter_idx > -1 && exporters.Length > exporter_idx)
                exportFileDialog.CurrentFilterIndex = exporter_idx + 1;
        }
        #endregion

        #region Open / Save / New
        public void Open()
        {
            if (!NotifyIfUnsaved())
                return;
            if (openFileDialog.ShowDialog(parent) == DialogResult.Ok)
            {
                InternalOpen(openFileDialog.FileName);
                UpdateLastPath(openFileDialog);
                lfh.AddLastFile(openFileDialog.FileName);
            }
        }

        public void Reload()
            => InternalOpen(FileState.FileName);

        internal void InternalOpen(string filename)
        {
            info.Logger.Info("Öffne Datei " + filename);
            Timetable = open.Import(filename, info);
            if (Timetable == null)
                info.Logger.Error("Fehler beim Öffnen der Datei!");
            else
                info.Logger.Info("Datei erfolgeich geöffnet!");
            if (Timetable?.UpgradeMessage != null)
                info.Logger.Warning(Timetable.UpgradeMessage);
            FileState.Opened = Timetable != null;
            FileState.Saved = Timetable != null;
            FileState.FileName = Timetable != null ? filename : null;
            undo.ClearHistory();
            if (Timetable != null)
                FileOpened?.Invoke(this, null);
        }

        public void Save(bool forceSaveAs)
        {
            string filename = FileState.FileName;

            bool saveAs = forceSaveAs || filename == null || filename == "" || Path.GetExtension(filename) != ".fpl";

            if (saveAs)
            {
                if (saveFileDialog.ShowDialog(parent) != DialogResult.Ok)
                    return;
                filename = saveFileDialog.FileName;
                UpdateLastPath(saveFileDialog);
                lfh.AddLastFile(saveFileDialog.FileName);
            }
            InternalSave(filename);
        }

        private void InternalSave(string filename)
        {
            if (!filename.EndsWith(".fpl"))
                filename += ".fpl";

            info.Logger.Info("Speichere Datei " + filename);
            bool ret = save.Export(Timetable, filename, info);
            if (ret == false)
                return;
            info.Logger.Info("Speichern erfolgreich abgeschlossen!");
            FileState.Saved = true;
            FileState.FileName = filename;
        }

        internal void New(TimetableType type)
        {
            if (!NotifyIfUnsaved())
                return;
            Timetable = new Timetable(type);
            FileState.Opened = true;
            FileState.Saved = false;
            FileState.FileName = null;
            undo.ClearHistory();
            info.Logger.Info("Neue Datei erstellt");
            FileOpened?.Invoke(this, null);
        }
        #endregion

        #region Last Directory Helper
        private void UpdateLastPath(FileDialog dialog) => SetLastPath(Path.GetDirectoryName(dialog.FileName));

        private void SetLastPath(string lastPath)
        {
            if (lastPath != "" && Uri.TryCreate(lastPath, UriKind.Absolute, out Uri uri))
            {
                openFileDialog.Directory = uri;
                saveFileDialog.Directory = uri;
            }
            info.Settings.Set("files.lastpath", lastPath);
        }
        #endregion

        #region Conversion Helpers
        internal void ConvertTimetable()
        {
            IExport exp = (Timetable.Type == TimetableType.Linear) ? (IExport)new NetworkExport() : new LinearExport();
            string orig = (Timetable.Type == TimetableType.Linear) ? "Linear-Fahrplan" : "Netzwerk-Fahrplan";
            string dest = (Timetable.Type == TimetableType.Linear) ? "Netzwerk-Fahrplan" : "Linear-Fahrplan";

            if (MessageBox.Show($"Die aktuelle Datei ist ein {orig}. Es wird zu einem {dest} konvertiert.", "FPLedit", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            var sfd = new SaveFileDialog();
            sfd.AddLegacyFilter(exp.Filter);
            if (sfd.ShowDialog(parent) == DialogResult.Ok)
            {
                info.Logger.Info("Konvertiere Datei...");
                bool ret = exp.Export(Timetable, sfd.FileName, info);
                if (ret == false)
                    return;
                info.Logger.Info("Konvertieren erfolgreich abgeschlossen!");
                InternalOpen(sfd.FileName);
            }
        }

        private void MaybeUpgradeTtVersion()
        {
            if (Timetable == null || Timetable.Version.Compare(Timetable.DefaultLinearVersion) >= 0)
                return;

            var res = MessageBox.Show("Diese Fahrplandatei ist im Dateiformat von jTrainGraph 2.x bzw. 3.0x erstellt worden. Im Format von jTrainGraph 3.1x stehen mehr Funktionen zur Verfügung." +
                " Soll das Format jetzt aktualisiert werden? ACHTUNG: Die Datei kann danach nicht mehr mit jTrainGraph 2.x oder 3.0x berabeitet werden!", "FPLedit",
                MessageBoxButtons.YesNo, MessageBoxType.Question);
            if (res != DialogResult.Yes)
                return;

            var exp = new UpgradeJTG3Export();
            var sfd = new SaveFileDialog();
            sfd.AddLegacyFilter(exp.Filter);
            if (sfd.ShowDialog(parent) == DialogResult.Ok)
            {
                info.Logger.Info("Konvertiere Datei...");
                bool ret = exp.Export(Timetable, sfd.FileName, info);
                if (ret == false)
                    return;
                info.Logger.Info("Konvertieren erfolgreich abgeschlossen!");
                InternalOpen(sfd.FileName);
            }
        }
        #endregion

        internal bool NotifyIfUnsaved()
        {
            if (!FileState.Saved && FileState.Opened)
            {
                DialogResult res = MessageBox.Show("Wollen Sie die noch nicht gespeicherten Änderungen speichern?",
                    "FPLedit", MessageBoxButtons.YesNoCancel);

                if (res == DialogResult.Yes)
                    Save(false);
                return res != DialogResult.Cancel;
            }
            return true;
        }
    }

    internal interface ILastFileHandler
    {
        void AddLastFile(string fn);
    }
}
