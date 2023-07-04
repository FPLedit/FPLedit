#nullable enable
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

        private Timetable? timetable;

        private readonly BinaryCacheFile cache;

        private bool asyncBlockingOperation;

        private bool AsyncBlockingOperation
        {
            get => asyncBlockingOperation;
            set
            {
                if (value != asyncBlockingOperation)
                {
                    asyncBlockingOperation = value;
                    Application.Instance.InvokeAsync(() => AsyncOperationStateChanged?.Invoke(this, value));
                }
            }
        }

        public Timetable? Timetable
        {
            get => timetable;
            set
            {
                timetable = value;
                if (value != null)
                    OnFileStateChanged();
            }
        }

        public FileState FileState { get; }

        public ICacheFile Cache => cache;

        public event EventHandler? FileOpened;
        public event EventHandler<FileStateChangedEventArgs>? FileStateChanged;
        public event EventHandler<bool>? AsyncOperationStateChanged;

        public FileHandler(IPluginInterface pluginInterface, ILastFileHandler lfh, UndoManager undo)
        {
            this.pluginInterface = pluginInterface;
            this.lfh = lfh;
            this.undo = undo;

            FileState = new FileState();
            FileState.FileStateInternalChanged += (_, _) => OnFileStateChanged();

            open = new XMLImport();
            save = new XMLExport();

            saveFileDialog = new SaveFileDialog { Title = T._("Fahrplandatei speichern") };
            openFileDialog = new OpenFileDialog { Title = T._("Fahrplandatei öffnen") };
            exportFileDialog = new SaveFileDialog { Title = T._("Fahrplandatei exportieren") };
            importFileDialog = new OpenFileDialog { Title = T._("Datei importieren") };

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

        // Bubble up FileState change event to global callers.
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
                pluginInterface.Logger.Error(T._("Keine Importer gefunden, Import nicht möglich!"));
                return;
            }

            if (!NotifyIfUnsaved())
                return;

            var (rev, frev) = (FileState.RevisionCounter, FileState.FileNameRevisionCounter);

            if (importFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
            {
                IImport import = importers[importFileDialog.CurrentFilterIndex];
                pluginInterface.Logger.Info(T._("Importiere Datei {0}", importFileDialog.FileName));

                InternalCloseFile();

                var tsk = import.GetAsyncSafeImport(importFileDialog.FileName, pluginInterface);
                tsk.ContinueWith((t, _) =>
                {
                    Application.Instance.InvokeAsync(() =>
                    {
                        if (t.Result == null)
                        {
                            pluginInterface.Logger.Error(T._("Importieren fehlgeschlagen!"));
                            return;
                        }

                        pluginInterface.Logger.Info(T._("Datei erfolgreich importiert!"));

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

                pluginInterface.Logger.Info(T._("Exportiere in Datei {0}", filename));
                var tsk = export.GetAsyncSafeExport(Timetable!.Clone(), filename, pluginInterface);
                tsk.ContinueWith((t, _) =>
                {
                    if (t.Result == false)
                        pluginInterface.Logger.Error(T._("Exportieren fehlgeschlagen!"));
                    else
                        pluginInterface.Logger.Info(T._("Exportieren erfolgreich abgeschlossen!"));
                }, null, TaskScheduler.Default);
                tsk.Start(); // Non-blocking operation.
            }
        }

        internal void InitializeExportImport(IExport[] exporters, IImport[] importers)
        {
            exportFileDialog.AddLegacyFilter(exporters.Select(ex => ex.Filter).ToArray());
            importFileDialog.AddLegacyFilter(importers.Select(im => im.Filter).ToArray());

            // Letzten Exporter auswählen
            int exporterIdx = pluginInterface.Settings.Get("exporter.last", -1);
            if (exporterIdx > -1 && exporters.Length > exporterIdx)
                exportFileDialog.CurrentFilterIndex = exporterIdx + 1;
        }

        #endregion

        #region Open / Save / New

        public void Open()
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;
            if (!NotifyIfUnsaved())
                return;
            if (openFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok && openFileDialog.FileName != null)
            {
                InternalOpen(openFileDialog.FileName!, true);
                UpdateLastPath(openFileDialog);
                lfh.AddLastFile(openFileDialog.FileName!);
            }
        }

        public void Reload()
            => InternalOpen(FileState.FileName!, false);

        internal void InternalOpen(string filename, bool doAsync)
        {
            InternalCloseFile();

            pluginInterface.Logger.Info(T._("Öffne Datei {0}", filename));

            // Load sidecar cache file.
            cache.Clear();
            var cacheFile = filename + "-cache";
            if (File.Exists(cacheFile))
            {
                var bytes = File.ReadAllBytes(filename);
                using var sha256 = SHA256.Create();
                var hash = string.Join("", sha256.ComputeHash(bytes).Select(b => b.ToString("X2")));

                using var stream = File.Open(cacheFile, FileMode.Open);
                cache.Read(stream, hash);
            }

            var (rev, frev) = (FileState.RevisionCounter, FileState.FileNameRevisionCounter);

            var tsk = open.GetAsyncSafeImport(filename, pluginInterface);
            tsk.ContinueWith((t, _) =>
            {
                var tsk2 = Application.Instance.InvokeAsync(() =>
                {
                    if (t.Result == null)
                    {
                        pluginInterface.Logger.Error(T._("Fehler beim Öffnen der Datei!"));
                        return;
                    }

                    pluginInterface.Logger.Info(T._("Datei erfolgeich geöffnet!"));

                    if (FileState.RevisionCounter != rev || FileState.FileNameRevisionCounter != frev)
                        if (!NotifyIfUnsaved(true)) // Ask again
                            return;

                    undo.ClearHistory();

                    OpenWithVersionCheck(t.Result, filename);
                });
                tsk2.ContinueWith((t1, _) =>
                {
                    if (t1.Exception == null) return;
                    pluginInterface.Logger.Error(t1.Exception.Message);
                    pluginInterface.Logger.Error(T._("Fehler beim Öffnen der Datei!"));

                    AsyncBlockingOperation = false; // Exit file actions that would spin forever.
                    Application.Instance.InvokeAsync(() => CloseFile());
                }, null, TaskContinuationOptions.OnlyOnFaulted);
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

            string? filename = FileState.FileName;

            bool saveAs = forceSaveAs || filename == null || filename == "" || Path.GetExtension(filename) != ".fpl";

            if (saveAs)
            {
                if (saveFileDialog.ShowDialog(pluginInterface.RootForm) != DialogResult.Ok)
                    return;
                filename = saveFileDialog.FileName!;
                UpdateLastPath(saveFileDialog);
                lfh.AddLastFile(saveFileDialog.FileName);
            }

            InternalSave(filename!, doAsync);
        }

        private void InternalSave(string filename, bool doAsync)
        {
            if (!filename.EndsWith(".fpl", StringComparison.InvariantCulture))
                filename += ".fpl";

            var (rev, frev) = (FileState.RevisionCounter, FileState.FileNameRevisionCounter);

            pluginInterface.Logger.Info(T._("Speichere Datei {0}", filename));

            var clone = Timetable!.Clone();

            var tsk = save.GetAsyncSafeExport(clone, filename, pluginInterface);
            tsk.ContinueWith((t, _) =>
            {
                Application.Instance.InvokeAsync(() =>
                {
                    if (t.Result == false)
                    {
                        pluginInterface.Logger.Error(T._("Speichern fehlgeschlagen!"));
                        return;
                    }

                    pluginInterface.Logger.Info(T._("Speichern erfolgreich abgeschlossen!"));

                    var nrev = FileState.RevisionCounter;
                    var nfrev = FileState.FileNameRevisionCounter;

                    if (nrev == rev)
                        FileState.Saved = true;
                    if (nfrev == frev)
                        FileState.FileName = filename;
                    if (nrev != rev || nfrev != frev)
                        pluginInterface.Logger.Warning(T._("Während dem Speichern wurde der Zustand verändert, daher ist Datei auf der Festplatte nicht mehr aktuell."));

                    // Create or delete sidecar file.
                    var cacheFile = filename + "-cache";
                    var bytes = File.ReadAllBytes(filename);
                    if (cache.Any() && cache.ShouldWriteCacheFile(clone, pluginInterface.Settings))
                    {
                        using var sha256 = SHA256.Create();
                        var hash = string.Join("", sha256.ComputeHash(bytes).Select(b => b.ToString("X2")));

                        using var stream = File.Open(cacheFile, FileMode.OpenOrCreate, FileAccess.Write);
                        stream.SetLength(0);
                        cache.Write(stream, hash);
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

            pluginInterface.Logger.Info(T._("Neue Datei erstellt"));
            FileOpened?.Invoke(this, EventArgs.Empty);
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

        public void CloseFile(bool manualAction = false)
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;
            if (!NotifyIfUnsaved())
                return;

            InternalCloseFile();

            if (manualAction)
                pluginInterface.Logger.Info(T._("Datei geschlossen"));
        }

        #endregion

        #region Last Directory Helper

        private void UpdateLastPath(FileDialog dialog) => SetLastPath(Path.GetDirectoryName(dialog.FileName));

        private void SetLastPath(string? lastPath)
        {
            if (!string.IsNullOrEmpty(lastPath) && Uri.TryCreate(lastPath, UriKind.Absolute, out Uri? uri))
            {
                openFileDialog.Directory = uri;
                saveFileDialog.Directory = uri;
            }

            pluginInterface.Settings.Set("files.lastpath", lastPath ?? "");
        }

        #endregion

        #region Conversion Helpers

        internal void ConvertTimetable()
        {
            if (!NotifyIfAsyncOperationInProgress())
                return;

            IExport exp = (Timetable!.Type == TimetableType.Linear) ? new NetworkExport() : new LinearExport();
            string orig = (Timetable!.Type == TimetableType.Linear) ? T._("Linear-Fahrplan") : T._("Netzwerk-Fahrplan");
            string dest = (Timetable!.Type == TimetableType.Linear) ? T._("Netzwerk-Fahrplan") : T._("Linear-Fahrplan");

            if (MessageBox.Show($"Die aktuelle Datei ist ein {orig}. Es wird zu einem {dest} konvertiert.", "FPLedit", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return;

            using var sfd = new SaveFileDialog();
            sfd.Title = T._("Zieldatei für Konvertierung wählen");
            sfd.AddLegacyFilter(exp.Filter);
            if (sfd.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
            {
                pluginInterface.Logger.Info(T._("Konvertiere Datei..."));
                bool ret = exp.SafeExport(Timetable, sfd.FileName, pluginInterface);
                if (ret == false)
                    return;
                pluginInterface.Logger.Info(T._("Konvertieren erfolgreich abgeschlossen!"));
                InternalOpen(sfd.FileName, true);
            }
        }

        private void OpenWithVersionCheck(Timetable? tt, string filename)
        {
            var compat = tt?.Version.GetVersionCompat().Compatibility;
            if (tt == null || compat == TtVersionCompatType.ReadWrite)
            {
                // Get default version based on timetable type.
                var defaultVersion = Timetable.DefaultLinearVersion;
                if (tt?.Type == TimetableType.Network)
                    defaultVersion = Timetable.DefaultNetworkVersion;
                // There is an optional timetable version update available.
                if (tt != null && defaultVersion.CompareTo(tt.Version) > 0 && PerformTimetableUpgrade(tt, true))
                    return;

                // this file is in a supported version (or non-existant).
                Timetable = tt;

                FileState.Opened = tt != null;
                FileState.Saved = tt != null;
                FileState.FileName = tt != null ? filename : null;

                AsyncBlockingOperation = false;

                if (tt != null)
                    FileOpened?.Invoke(this, EventArgs.Empty);
                return;
            }

            if (compat == TtVersionCompatType.None)
            {
                AsyncBlockingOperation = false;
                pluginInterface.Logger.Error(T._("Diese Dateiformatsversion ({0}) wird nicht unterstützt.", tt.Version.ToNumberString()));
                return;
            }

            PerformTimetableUpgrade(tt, false);
        }

        private bool PerformTimetableUpgrade(Timetable tt, bool optional)
        {
            string? newFn = null;
            // Exporter based on timetable type.
            IExport exp;
            if (tt.Type == TimetableType.Linear)
                exp = new BackwardCompat.LinearUpgradeExport();
            else
                exp = new BackwardCompat.NetworkUpgradeExport();

            Application.Instance.Invoke(() =>
            {
                var text = optional ? T._("In einer neueren Dateiformatversion stehen mehr Funktionen zur Verfügung, die Aktualisierung ist aber nicht zwingend.") : T._("Dieses Dateiformat kann von FPLedit nur noch auf neuere Versionen aktualisiert werden.");
                text = T._("Diese Fahrplandatei ist in einem alten Dateinformat erstellt worden. {0} Soll die Datei jetzt aktualisiert werden?", text);
                if (tt.Type == TimetableType.Linear)
                    text += T._("ACHTUNG: Die Datei kann danach nur noch mit der aktuellsten Version von jTrainGraph bearbeitet werden!");
                var res = MessageBox.Show(text, "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Question);
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
                pluginInterface.Logger.Info(T._("Konvertiere Datei..."));
                var ret = exp.SafeExport(tt, newFn, pluginInterface);
                if (ret)
                {
                    pluginInterface.Logger.Info(T._("Konvertieren erfolgreich abgeschlossen!"));
                    InternalOpen(newFn, false); // We are already in an async context, so we can execute synchronously.
                    return true; // Do not exit async operation.
                }

                pluginInterface.Logger.Error(T._("Dateikonvertierung fehlgeschlagen!"));
            }
            else
            {
                if (optional)
                    return false; // We asked for an optional update, so we do not exit the async operation.
                pluginInterface.Logger.Warning(T._("Dateikonvertierung abgebrochen!"));
            }

            AsyncBlockingOperation = false;
            return true;
        }

        #endregion

        internal bool NotifyIfUnsaved(bool asyncChange = false)
        {
            if (!FileState.Saved && FileState.Opened)
            {
                DialogResult res = MessageBox.Show(asyncChange ? T._("Während dem Ladevorgang wurden Daten geändert. Sollen diese noch einmal in die letzte Datei geschrieben werden?") : T._("Wollen Sie die noch nicht gespeicherten Änderungen speichern?"),
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
                pluginInterface.Logger.Error(T._("Eine andere Dateioperation ist bereits im Gange!"));
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            if (!openFileDialog.IsDisposed)
                openFileDialog.Dispose();
            if (!saveFileDialog.IsDisposed)
                saveFileDialog.Dispose();
            if (!exportFileDialog.IsDisposed)
                exportFileDialog.Dispose();
            if (!importFileDialog.IsDisposed)
                importFileDialog.Dispose();
        }
    }
}