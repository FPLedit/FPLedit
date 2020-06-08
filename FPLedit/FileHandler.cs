using Eto.Forms;
using FPLedit.NonDefaultFiletypes;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FPLedit
{
    internal sealed class FileHandler : IDisposable
    {
        private readonly SaveFileDialog saveFileDialog, exportFileDialog;
        private readonly OpenFileDialog openFileDialog, importFileDialog;

        private readonly IImport open;
        private readonly IExport save;

        private readonly IPluginInterface pluginInterface;
        private readonly ILastFileHandler lfh;
        private readonly UndoManager undo;

        private Timetable timetable;

        private readonly BinaryCacheFile cache;
        
        private bool asyncBlockingOperation;

        public bool AsyncBlockingOperation
        {
            get => asyncBlockingOperation;
            private set
            {
                if (value != asyncBlockingOperation)
                {
                    asyncBlockingOperation = value;
                    Application.Instance.InvokeAsync(() => AsyncOperationStateChanged?.Invoke(this, value));
                }
            }
        }

        public Timetable Timetable
        {
            get => timetable;
            set
            {
                timetable = value;
                if (value != null)
                    FileStateChanged?.Invoke(this, new FileStateChangedEventArgs(FileState));
            }
        }
        public FileState FileState { get; }

        public ICacheFile Cache => cache;

        public event EventHandler FileOpened;
        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        public event EventHandler<bool> AsyncOperationStateChanged; 

        public FileHandler(IPluginInterface pluginInterface, ILastFileHandler lfh, UndoManager undo)
        {
            this.pluginInterface = pluginInterface;
            this.lfh = lfh;
            this.undo = undo;

            FileState = new FileState();
            FileState.FileStateInternalChanged += (s, e) => OnFileStateChanged();

            open = new XMLImport();
            save = new XMLExport();

            saveFileDialog = new SaveFileDialog { Title = "Fahrplandatei speichern" };
            openFileDialog = new OpenFileDialog { Title = "Fahrplandatei öffnen" };
            exportFileDialog = new SaveFileDialog { Title = "Fahrplandatei exportieren" };
            importFileDialog = new OpenFileDialog { Title = "Datei importieren" };

            saveFileDialog.AddLegacyFilter(save.Filter);
            openFileDialog.AddLegacyFilter(open.Filter);

            SetLastPath(pluginInterface.Settings.Get("files.lastpath", ""));
            
            cache = new BinaryCacheFile();
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
            if (!NotifyIfAsyncOperationInProgress())
                return;
            
            var importers = pluginInterface.GetRegistered<IImport>();

            if (importers.Length == 0)
            {
                pluginInterface.Logger.Error("Keine Importer gefunden, Import nicht möglich!");
                return;
            }

            if (!NotifyIfUnsaved(false))
                return;

            var (rev, frev) = (FileState.RevisionCounter, FileState.FileNameRevisionCounter);

            if (importFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
            {
                IImport import = importers[importFileDialog.CurrentFilterIndex];
                pluginInterface.Logger.Info("Importiere Datei " + importFileDialog.FileName);
                
                InternalCloseFile();

                var tsk = import.GetAsyncSafeImport(importFileDialog.FileName, pluginInterface);
                tsk.ContinueWith((Task<Timetable> t, object o) =>
                {
                    Application.Instance.InvokeAsync(() =>
                    {
                        if (t.Result == null)
                        {
                            pluginInterface.Logger.Error("Importieren fehlgeschlagen!");
                            return;
                        }

                        pluginInterface.Logger.Info("Datei erfolgreich importiert!");

                        if (FileState.RevisionCounter != rev || FileState.FileNameRevisionCounter != frev)
                            if (!NotifyIfUnsaved(true)) // Ask again
                                return;
                        
                        undo.ClearHistory();
                        OpenWithVersionCheck(t.Result, importFileDialog.FileName);
                    });
                }, null, TaskScheduler.Default);
                
                // Start async operation, but we should not be able to start any other file operation in between.
                AsyncBlockingOperation = true;
                tsk.Start();
            }
        }

        internal void Export()
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;
            if (exportFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
            {
                var exporters = pluginInterface.GetRegistered<IExport>();
                var export = exporters[exportFileDialog.CurrentFilterIndex];
                var filename = exportFileDialog.FileName;
                pluginInterface.Settings.Set("exporter.last", exportFileDialog.CurrentFilterIndex);

                pluginInterface.Logger.Info("Exportiere in Datei " + filename);
                var tsk = export.GetAsyncSafeExport(Timetable.Clone(), filename, pluginInterface);
                tsk.ContinueWith((t, o) =>
                {
                    if (t.Result == false)
                        pluginInterface.Logger.Error("Exportieren fehlgeschlagen!");
                    else
                        pluginInterface.Logger.Info("Exportieren erfolgreich abgeschlossen!");
                }, null, TaskScheduler.Default);
                tsk.Start(); // Non-blocking operation.
            }
        }

        internal void InitializeExportImport(IExport[] exporters, IImport[] importers)
        {
            exportFileDialog.AddLegacyFilter(exporters.Select(ex => ex.Filter).ToArray());
            importFileDialog.AddLegacyFilter(importers.Select(im => im.Filter).ToArray());

            // Letzten Exporter auswählen
            int exporter_idx = pluginInterface.Settings.Get("exporter.last", -1);
            if (exporter_idx > -1 && exporters.Length > exporter_idx)
                exportFileDialog.CurrentFilterIndex = exporter_idx + 1;
        }

        #endregion

        #region Open / Save / New
        
        public void Open()
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;
            if (!NotifyIfUnsaved())
                return;
            if (openFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
            {
                InternalOpen(openFileDialog.FileName, true);
                UpdateLastPath(openFileDialog);
                lfh.AddLastFile(openFileDialog.FileName);
            }
        }

        public void Reload()
            => InternalOpen(FileState.FileName, false);

        internal void InternalOpen(string filename, bool doAsync)
        {
            InternalCloseFile();

            pluginInterface.Logger.Info("Öffne Datei " + filename);
            
            // Load sidecar cache file.
            cache.Clear();
            var cacheFile = filename + "-cache";
            if (File.Exists(cacheFile))
            {
                var bytes = File.ReadAllBytes(filename);
                using (var sha256 = SHA256.Create())
                {
                    var hash = string.Join("", sha256.ComputeHash(bytes).Select(b => b.ToString("X2")));

                    using (var stream = File.Open(cacheFile, FileMode.Open))
                        cache.Read(stream, hash);
                }
            }
            
            var (rev, frev) = (FileState.RevisionCounter, FileState.FileNameRevisionCounter);
            
            var tsk = open.GetAsyncSafeImport(filename, pluginInterface);
            tsk.ContinueWith((Task<Timetable> t, object o) =>
            {
                Application.Instance.InvokeAsync(() =>
                {
                    if (t.Result == null)
                    {
                        pluginInterface.Logger.Error("Fehler beim Öffnen der Datei!");
                        return;
                    }

                    pluginInterface.Logger.Info("Datei erfolgeich geöffnet!");

                    if (FileState.RevisionCounter != rev || FileState.FileNameRevisionCounter != frev)
                        if (!NotifyIfUnsaved(true)) // Ask again
                            return;

                    undo.ClearHistory();

                    OpenWithVersionCheck(t.Result, filename);
                });
            }, null, TaskScheduler.Default);
                
            // Start async operation, but we should not be able to start any other file operation in between.
            AsyncBlockingOperation = true;
            if (doAsync)
                tsk.Start();
            else
                tsk.RunSynchronously();
        }

        public void Save(bool forceSaveAs)
            => Save(forceSaveAs, true);
        
        private void Save(bool forceSaveAs, bool doAsync)
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;
            
            string filename = FileState.FileName;

            bool saveAs = forceSaveAs || filename == null || filename == "" || Path.GetExtension(filename) != ".fpl";

            if (saveAs)
            {
                if (saveFileDialog.ShowDialog(pluginInterface.RootForm) != DialogResult.Ok)
                    return;
                filename = saveFileDialog.FileName;
                UpdateLastPath(saveFileDialog);
                lfh.AddLastFile(saveFileDialog.FileName);
            }

            InternalSave(filename, doAsync);
        }

        private void InternalSave(string filename, bool doAsync)
        {
            if (!filename.EndsWith(".fpl"))
                filename += ".fpl";

            var (rev, frev) = (FileState.RevisionCounter, FileState.FileNameRevisionCounter);

            pluginInterface.Logger.Info("Speichere Datei " + filename);
            
            var clone = Timetable.Clone();
            
            var tsk = save.GetAsyncSafeExport(clone, filename, pluginInterface);
            tsk.ContinueWith((t, o) =>
            {
                Application.Instance.InvokeAsync(() =>
                {
                    if (t.Result == false)
                    {
                        pluginInterface.Logger.Error("Speichern fehlgeschlagen!");
                        return;
                    }

                    pluginInterface.Logger.Info("Speichern erfolgreich abgeschlossen!");

                    if (FileState.RevisionCounter == rev)
                        FileState.Saved = true;
                    if (FileState.FileNameRevisionCounter == frev)
                        FileState.FileName = filename;
                    if (FileState.RevisionCounter != rev || FileState.FileNameRevisionCounter != frev)
                        pluginInterface.Logger.Warning("Während dem Speichern wurde der Zustand verändert, daher ist Datei auf der Festplatte nicht mehr aktuell.");

                    // Create or delete sidecar file.
                    var cacheFile = filename + "-cache";
                    var bytes = File.ReadAllBytes(filename);
                    if (cache.Any() && cache.ShouldWriteCacheFile(clone, pluginInterface.Settings))
                    {
                        using (var sha256 = SHA256.Create())
                        {
                            var hash = string.Join("", sha256.ComputeHash(bytes).Select(b => b.ToString("X2")));

                            using (var stream = File.Open(cacheFile, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                stream.SetLength(0);
                                cache.Write(stream, hash);
                            }
                        }
                    }
                    else if (File.Exists(cacheFile)) // Delete cache file if we don't need one.
                        File.Delete(cacheFile);
                });
            }, null, TaskScheduler.Default);
            if (doAsync)
                tsk.Start();
            else
                tsk.RunSynchronously();
        }

        internal void New(TimetableType type)
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;
            if (!NotifyIfUnsaved())
                return;
            
            Timetable = new Timetable(type);
            FileState.Opened = true;
            FileState.Saved = false;
            FileState.FileName = null;
            undo.ClearHistory();
            cache.Clear();
            
            pluginInterface.Logger.Info("Neue Datei erstellt");
            FileOpened?.Invoke(this, null);
        }
        
        private void InternalCloseFile()
        {
            Timetable = null;
            FileState.Opened = false;
            FileState.Saved = false;
            FileState.FileName = null;
            
            cache.Clear();
            undo.ClearHistory();
        }
        
        public void CloseFile()
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;
            if (!NotifyIfUnsaved())
                return;
            
            InternalCloseFile();
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

            pluginInterface.Settings.Set("files.lastpath", lastPath);
        }

        #endregion

        #region Conversion Helpers

        internal void ConvertTimetable()
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;
            
            IExport exp = (Timetable.Type == TimetableType.Linear) ? (IExport) new NetworkExport() : new LinearExport();
            string orig = (Timetable.Type == TimetableType.Linear) ? "Linear-Fahrplan" : "Netzwerk-Fahrplan";
            string dest = (Timetable.Type == TimetableType.Linear) ? "Netzwerk-Fahrplan" : "Linear-Fahrplan";

            if (MessageBox.Show($"Die aktuelle Datei ist ein {orig}. Es wird zu einem {dest} konvertiert.", "FPLedit", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Zieldatei für Konvertierung wählen";
                sfd.AddLegacyFilter(exp.Filter);
                if (sfd.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
                {
                    pluginInterface.Logger.Info("Konvertiere Datei...");
                    bool ret = exp.SafeExport(Timetable, sfd.FileName, pluginInterface);
                    if (ret == false)
                        return;
                    pluginInterface.Logger.Info("Konvertieren erfolgreich abgeschlossen!");
                    InternalOpen(sfd.FileName, true);
                }
            }
        }

        private void OpenWithVersionCheck(Timetable tt, string filename)
        {
            //TODO: Implement user-chosen updates, if a newer version exists and both are supported.
            var compat = tt?.Version.GetCompat();
            if (tt == null || compat == TtVersionCompatType.ReadWrite)
            {
                // this file is in a supported version (or non-existant)
                Timetable = tt;
                
                FileState.Opened = tt != null;
                FileState.Saved = tt != null;
                FileState.FileName = tt != null ? filename : null;

                AsyncBlockingOperation = false;

                if (tt != null)
                    FileOpened?.Invoke(this, null);
                return;
            }

            if (compat == TtVersionCompatType.None)
            {
                AsyncBlockingOperation = false;
                pluginInterface.Logger.Error($"Diese Dateiformatsversion ({tt.Version.ToNumberString()}) wird nicht unterstützt.");
                return;
            }

            string newFn = null;
            var exp = new UpgradeExport();

            Application.Instance.Invoke(() =>
            {
                var res = MessageBox.Show("Diese Fahrplandatei ist im Dateiformat von jTrainGraph 2.x bzw. 3.0x erstellt worden. Dieses Dateiformat kann von FPLedit nur noch auf neuere Versionen aktualisiert werden." +
                                          " Soll die Datei jetzt aktualisiert werden? ACHTUNG: Die Datei kann danach nicht mehr mit jTrainGraph 2.x oder 3.0x berabeitet werden!", "FPLedit",
                    MessageBoxButtons.YesNo, MessageBoxType.Question);
                if (res == DialogResult.Yes)
                {
                    using var sfd = new SaveFileDialog();
                    sfd.Title = "Zieldatei für Konvertierung wählen";
                    sfd.AddLegacyFilter(exp.Filter);
                    if (sfd.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
                        newFn = sfd.FileName;
                }
            });
            
            if (!string.IsNullOrEmpty(newFn))
            {
                pluginInterface.Logger.Info("Konvertiere Datei...");
                var ret = exp.SafeExport(tt, newFn, pluginInterface);
                if (ret)
                {
                    pluginInterface.Logger.Info("Konvertieren erfolgreich abgeschlossen!");
                    InternalOpen(newFn, false); // We are already in an async context, so we can execute synchronously.
                    return; // Do not exit async operation.
                }
                
                pluginInterface.Logger.Error("Dateikonvertierung fehlgeschlagen!");
            }
            else
                pluginInterface.Logger.Warning("Dateikonvertierung abgebrochen!");

            AsyncBlockingOperation = false;
        }

        #endregion

        internal bool NotifyIfUnsaved(bool asyncChange = false)
        {
            if (!FileState.Saved && FileState.Opened)
            {
                DialogResult res = MessageBox.Show(asyncChange ? "Während dem Ladevorgang wurden Daten geändert. Sollen diese noch einmal in die letzte Datei geschrieben werden?" : "Wollen Sie die noch nicht gespeicherten Änderungen speichern?",
                    "FPLedit", MessageBoxButtons.YesNoCancel);

                if (res == DialogResult.Yes)
                    Save(false, doAsync: false);
                return res != DialogResult.Cancel;
            }

            return true;
        }

        private bool NotifyIfAsyncOperationInProgress()
        {
            if (AsyncBlockingOperation)
            {
                pluginInterface.Logger.Error("Eine andere Dateioperation ist bereits im Gange!");
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (openFileDialog != null && !openFileDialog.IsDisposed)
                openFileDialog.Dispose();
            if (saveFileDialog != null && !saveFileDialog.IsDisposed)
                saveFileDialog.Dispose();
            if (exportFileDialog != null && !exportFileDialog.IsDisposed)
                exportFileDialog.Dispose();
            if (importFileDialog != null && !importFileDialog.IsDisposed)
                importFileDialog.Dispose();
        }
    }
}