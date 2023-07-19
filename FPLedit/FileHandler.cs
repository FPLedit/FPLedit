using Eto.Forms;
using FPLedit.NonDefaultFiletypes;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FPLedit;

internal sealed class FileHandler : IDisposable
{
    #region Lazy-loaded file dialogs
    private string? lastPath;
    private SaveFileDialog? saveFileDialog;
    private SaveFileDialog SaveFileDialog
    {
        get
        {
            if (saveFileDialog == null)
            {
                saveFileDialog = new SaveFileDialog { Title = T._("Fahrplandatei speichern") };
                saveFileDialog.AddLegacyFilter(save.Filter);
                SetLastPath(lastPath);
            }
            return saveFileDialog;
        }
    }

    private SaveFileDialog? exportFileDialog;
    private SaveFileDialog ExportFileDialog
    {
        get
        {
            if (exportFileDialog == null)
            {
                if (exporters == null)
                    throw new Exception("Exporters not initialized!");
                exportFileDialog = new SaveFileDialog { Title = T._("Fahrplandatei exportieren") };
                exportFileDialog.AddLegacyFilter(exporters.Select(ex => ex.Filter).ToArray());
                    
                // Letzten Exporter auswählen
                int exporterIdx = pluginInterface.Settings.Get("exporter.last", -1);
                if (exporterIdx > -1 && exporters.Length > exporterIdx)
                    exportFileDialog.CurrentFilterIndex = exporterIdx + 1;
            }
            return exportFileDialog;
        }
    }

    private OpenFileDialog? openFileDialog;
    private OpenFileDialog OpenFileDialog
    {
        get
        {
            if (openFileDialog == null)
            {
                openFileDialog = new OpenFileDialog { Title = T._("Fahrplandatei öffnen") };
                openFileDialog.AddLegacyFilter(open.Filter);
                SetLastPath(lastPath);
            }
            return openFileDialog;
        }
    }

    private OpenFileDialog? importFileDialog;
    private OpenFileDialog ImportFileDialog
    {
        get
        {
            if (importFileDialog == null)
            {
                if (importers == null)
                    throw new Exception("Importers not initialized!");
                importFileDialog = new OpenFileDialog { Title = T._("Datei importieren") };
                importFileDialog.AddLegacyFilter(importers.Select(ex => ex.Filter).ToArray());
            }

            return importFileDialog;
        }
    }

    #endregion

    private readonly IImport open;
    private readonly IExport save;
    private IImport[]? importers;
    private IExport[]? exporters;

    private readonly IPluginInterface pluginInterface;
    private readonly ILastFileHandler lfh;
    private readonly UndoManager undo;

    private Timetable? timetable;

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

        lastPath = pluginInterface.Settings.Get("files.lastpath", "");
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
        if (importers == null)
            throw new Exception("Importers not initialized!");

        if (!NotifyIfAsyncOperationInProgress())
            return;

        if (importers.Length == 0)
        {
            pluginInterface.Logger.Error(T._("Keine Importer gefunden, Import nicht möglich!"));
            return;
        }

        if (!NotifyIfUnsaved())
            return;

        var (rev, frev) = (FileState.RevisionCounter, FileState.FileNameRevisionCounter);

        if (ImportFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
        {
            IImport import = importers[ImportFileDialog.CurrentFilterIndex];
            pluginInterface.Logger.Info(T._("Importiere Datei {0}", ImportFileDialog.FileName));

            InternalCloseFile();

            var tsk = import.GetAsyncSafeImport(ImportFileDialog.FileName, pluginInterface);
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
                    OpenWithVersionCheck(t.Result, ImportFileDialog.FileName);
                });
            }, null, TaskScheduler.Default);

            // Start async operation, but we should not be able to start any other file operation in between.
            AsyncBlockingOperation = true;
            tsk.Start();
        }
    }

    internal void Export()
    {
        if (exporters == null)
            throw new Exception("Exporters not initialized!");

        if (!NotifyIfAsyncOperationInProgress())
            return;
        if (ExportFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok)
        {
            var export = exporters[ExportFileDialog.CurrentFilterIndex];
            var filename = ExportFileDialog.FileName;
            pluginInterface.Settings.Set("exporter.last", ExportFileDialog.CurrentFilterIndex);

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
        if (this.exporters != null || this.importers != null)
            throw new Exception("Exporters and importers already initialized!");
        this.exporters = exporters;
        this.importers = importers;
    }

    #endregion

    #region Open / Save / New

    public void Open()
    {
        if (!NotifyIfAsyncOperationInProgress())
            return;
        if (!NotifyIfUnsaved())
            return;
        if (OpenFileDialog.ShowDialog(pluginInterface.RootForm) == DialogResult.Ok && OpenFileDialog.FileName != null)
        {
            var fn = OpenFileDialog.FileName!;
            InternalOpen(fn, true);
            UpdateLastPath(fn);
            lfh.AddLastFile(fn);
        }
    }

    public void Reload()
        => InternalOpen(FileState.FileName!, false);

    internal void InternalOpen(string filename, bool doAsync)
    {
        InternalCloseFile();

        pluginInterface.Logger.Info(T._("Öffne Datei {0}", filename));

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
            if (SaveFileDialog.ShowDialog(pluginInterface.RootForm) != DialogResult.Ok)
                return;
            filename = SaveFileDialog.FileName!;
            UpdateLastPath(filename);
            lfh.AddLastFile(filename);
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

        pluginInterface.Logger.Info(T._("Neue Datei erstellt"));
        FileOpened?.Invoke(this, EventArgs.Empty);
    }

    private void InternalCloseFile()
    {
        Timetable = null;
        FileState.Opened = false;
        FileState.Saved = false;
        FileState.FileName = null;

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

    private void UpdateLastPath(string fn) => SetLastPath(Path.GetDirectoryName(fn));

    private void SetLastPath(string? newLastPath)
    {
        if (!string.IsNullOrEmpty(newLastPath) && (openFileDialog != null || saveFileDialog != null) && Uri.TryCreate(newLastPath, UriKind.Absolute, out Uri? uri))
        {
            if (openFileDialog != null)
                openFileDialog.Directory = uri;
            if (saveFileDialog != null)
                saveFileDialog.Directory = uri;
        }

        pluginInterface.Settings.Set("files.lastpath", newLastPath ?? "");
        this.lastPath = newLastPath;
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