using System;
using System.Collections.Generic;
using System.IO;
using Eto.Forms;
using FPLedit.Config;
using FPLedit.Extensibility;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;
using FPLedit.Templating;

namespace FPLedit;

internal sealed class Bootstrapper : IPluginInterface, IDisposable
{
    private const string DEFAULT_TEMPLATE_PATH = "templates";

    private readonly RegisterStore registry;
    private readonly UndoManager undo;
    private readonly Settings settings;
    private readonly Dictionary<object, Timetable> timetableBackup;

    private bool extensionsPreLoaded;

    public UpdateManager Update { get; }
    public FileHandler FileHandler { get; }
    public ExtensionManager ExtensionManager { get; }
    ISettings IPluginInterface.Settings => settings;
    IReadOnlySettings IReducedPluginInterface.Settings => settings;
    public ISettings FullSettings => settings;
    public ITemplateManager TemplateManager { get; private set; } = null!;
    public ILog Logger { get; private set; } = null!;
    public dynamic RootForm { get; private set; } = null!;
    public dynamic Menu { get; private set; } = null!;

    public Timetable Timetable => FileHandler.Timetable ?? throw new NullReferenceException("Current timetable instance is null!");
    public Timetable? TimetableMaybeNull => FileHandler.Timetable;
    public IFileState FileState => FileHandler.FileState;

    public event EventHandler<FileStateChangedEventArgs>? FileStateChanged;
    public event EventHandler? ExtensionsLoaded;
    public event EventHandler? AppClosing;
    public event EventHandler? FileOpened;

    public List<string> PreBootstrapWarnings { get; } = new List<string>();

    public Bootstrapper(ILastFileHandler lfh)
    {
        timetableBackup = new Dictionary<object, Timetable>();

        var configPath = Path.Combine(PathManager.Instance.SettingsDirectory, "fpledit.conf");
        settings = new Settings(GetConfigStream(configPath));

        var lang = settings.Get("lang", "de-DE");
        T.SetLocale(Path.Combine(PathManager.Instance.AppDirectory, "Languages"), lang);

        registry = new RegisterStore();
        Update = new UpdateManager(settings);
        undo = new UndoManager();
        ExtensionManager = new ExtensionManager(this);
        FileHandler = new FileHandler(this, lfh, undo);

        FileHandler.FileOpened += (o, args) => FileOpened?.Invoke(o, args);
        FileHandler.FileStateChanged += (o, args) => FileStateChanged?.Invoke(o, args);
    }

    public void InitializeUi(Window rootForm)
    {
        RootForm = rootForm;
        Menu = rootForm.Menu;
    }

    public void BootstrapExtensions()
    {
        if (Logger == null || RootForm == null)
            throw new InvalidOperationException("Bootstrapper was not fully initialized before attempted to bootstrap extensions!");

        if (!extensionsPreLoaded)
            throw new InvalidOperationException("Extensions have not been preloaded before attempting to initializing them!");

        // Initialize (already loaded) extensions (==> base for the next steps)
        ExtensionManager.InitActivatedExtensions(registry);

        // Initialize Export/Import
        var exporters = GetRegistered<IExport>();
        var importers = GetRegistered<IImport>();
        FileHandler.InitializeExportImport(exporters, importers);

        // Load templates from files & extensions
        var templatePath = settings.Get("tmpl.root", DEFAULT_TEMPLATE_PATH);
        var templateManager = new TemplateManager(registry, this, templatePath);
        templateManager.LoadTemplates(templatePath);
        TemplateManager = templateManager;

        ExtensionsLoaded?.Invoke(this, EventArgs.Empty);

        (RootForm as Window)!.Closing += (_, _) => AppClosing?.Invoke(this, EventArgs.Empty);
    }

    public void PreBootstrapExtensions()
    {
        var warns = ExtensionManager.LoadExtensions();
        PreBootstrapWarnings.AddRange(warns);

        extensionsPreLoaded = true;
    }

    #region Backup & Undo

    public void Undo()
    {
        if (undo.CanGoBack)
            FileHandler.Timetable = undo.Undo();
        FileHandler.FileState.Saved = false;
    }

    public void StageUndoStep()
        => undo.StageUndoStep(Timetable);

    public void ClearBackup(object backupHandle)
    {
        timetableBackup.Remove(backupHandle);
    }

    public object BackupTimetable()
    {
        var backupHandle = new object();
        timetableBackup[backupHandle] = Timetable.Clone();
        return backupHandle;
    }

    public void RestoreTimetable(object backupHandle)
    {
        if (!timetableBackup.TryGetValue(backupHandle, out var backupTt))
            throw new ArgumentException(nameof(backupHandle) + " is invalid!");
        FileHandler.Timetable = backupTt;
        ClearBackup(backupHandle);
    }

    #endregion

    #region IPluginInterface Misc Implementations

    public string ExecutableDir => PathManager.Instance.AppDirectory;

    public T[] GetRegistered<T>() => registry.GetRegistered<T>();

    public string GetTemp(string filename)
    {
        try
        {
            return Path.Combine(PathManager.CreateTempDir(), filename);
        }
        catch (Exception ex)
        {
            Logger.LogException(ex);
            Logger.Warning(T._("Temp-Verzeichnis konnte nicht angelegt werden. Fallback-Dateipfad wird verwendet. Dies wird Probleme bereiten!"));
            return Path.GetTempFileName();
        }
    }

    internal void ClearTemp()
    {
        var dirpath = Path.Combine(Path.GetTempPath(), "fpledit");
        try
        {
            if (Directory.Exists(dirpath))
                Directory.Delete(dirpath, true);
        }
        catch
        {
            Logger.Warning(T._("Temp-Verzeichnis konnte nicht geleert werden."));
        }
    }

    public void SetUnsaved() => FileHandler.SetUnsaved();

    public void Open() => FileHandler.Open();

    public void Save(bool forceSaveAs) => FileHandler.Save(forceSaveAs);

    public void Reload() => FileHandler.Reload();

    public void OpenUrl(string address, bool isInternal = false) => OpenHelper.Open(address);

    #endregion

    #region Config stream initialization

    private FileStream? GetFileStreamWithMaxPermissions(string filename)
    {
        // try getting write access
        try { return File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read); }
        catch {}

        // try again with just read access
        try { return File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); }
        catch {}

        return null;
    }

    private Stream? GetConfigStream(string path)
    {
        var stream = GetFileStreamWithMaxPermissions(path); // try to open normal settings file

        var createInUserDirectory = false;
        string? userPath = null;
        if (stream != null)
        {
            using var config = new ConfigFile(stream, true); // create a temporary ConfigFile instance from the file in the app directory.
            userPath = config.Get("config.path_redirect");
        }

        var userDirectory = Path.GetDirectoryName(userPath);
        if (!string.IsNullOrEmpty(userPath) && Directory.Exists(userDirectory))
        {
            stream?.Close(); // We don't need the old stream any more.
            stream?.Dispose();
            stream = GetFileStreamWithMaxPermissions(userPath);
            createInUserDirectory = true;

            if (stream == null)
                PreBootstrapWarnings.Add(T._("Anlegen der angeforderten Einstellungsdatei {0} ist fehlgeschlagen.", userPath));
        }
        else if (!string.IsNullOrEmpty(userPath))
            PreBootstrapWarnings.Add(T._("Der als config.path_redirect angegebene Ordnerpfad {0} existiert nicht! Verwende die Einstellungsdatei im Programmverzeichnis.", userDirectory!));

        // Extract default configuration file from application reosurces if no config file exists or it is empty.
        if (stream != null && stream.Length == 0 && stream.CanWrite)
        {
            if (!createInUserDirectory)
                using (var defaultConfigFile = ResourceHelper.GetResource("Resources.appdir.conf"))
                    defaultConfigFile.CopyTo(stream);
            using (var defaultConfigFile = ResourceHelper.GetResource("Resources.fpledit.conf"))
                defaultConfigFile.CopyTo(stream);
        }

        if (!stream?.CanWrite ?? true)
            PreBootstrapWarnings.Add(T._("Keine Einstellungsdatei zum Schreiben gefunden. Änderungen an Programmeinstellungen werden verworfen."));

        return stream;
    }

    #endregion

    public void InjectLogger(ILog logger) => Logger = logger;

    public void Dispose()
    {
        FileHandler?.Dispose();
        registry?.Dispose();
        settings?.Dispose();
    }
}