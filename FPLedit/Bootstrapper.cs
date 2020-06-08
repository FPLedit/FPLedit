using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eto.Forms;
using FPLedit.Config;
using FPLedit.Extensibility;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;
using FPLedit.Templating;

namespace FPLedit
{
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
        public ICacheFile Cache => FileHandler.Cache;
        public ITemplateManager TemplateManager { get; private set; }
        public ILog Logger { get; private set; }
        public dynamic RootForm { get; private set; }
        public dynamic Menu { get; private set; }
        public dynamic HelpMenu { get; private set; }

        public Timetable Timetable => FileHandler.Timetable;
        public IFileState FileState => FileHandler.FileState;

        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        public event EventHandler ExtensionsLoaded;
        public event EventHandler AppClosing;
        public event EventHandler FileOpened;
        
        public List<string> PreBootstrapWarnings { get; } = new List<string>();

        public Bootstrapper(LastFileHandler lfh)
        {
            timetableBackup = new Dictionary<object, Timetable>();
            
            var configPath = Path.Combine(PathManager.Instance.AppDirectory, "fpledit.conf");

#pragma warning disable CA2000
            settings = new Settings(GetConfigStream(configPath));
#pragma warning restore CA2000
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
            HelpMenu = rootForm.Menu.GetItem(MainForm.LocHelpMenu);
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
            if (OptionsParser.TemplateDebug)
                Task.Run(() => Application.Instance.Invoke(() => templateManager.DebugCompileAll()));
            TemplateManager = templateManager;

            ExtensionsLoaded?.Invoke(this, new EventArgs());
            
            (RootForm as Window)!.Closing += (s, e) => AppClosing?.Invoke(this, null);
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
            if (!timetableBackup.TryGetValue(backupHandle, out Timetable backupTt))
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
            var dirpath = Path.Combine(Path.GetTempPath(), "fpledit");
            try
            {
                if (!Directory.Exists(dirpath))
                    Directory.CreateDirectory(dirpath);
                return Path.Combine(dirpath, filename);
            }
            catch
            {
                Logger.Warning("Temp-Datei konnte nicht angelegt werden. Fallback-Dateipfad wird verwendet. Dies wird Probleme bereiten!");
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
                Logger.Warning("Temp-Verzeichnis konnte nicht geleert werden.");
            }
        }

        public void SetUnsaved() => FileHandler.SetUnsaved();

        public void Open() => FileHandler.Open();

        public void Save(bool forceSaveAs) => FileHandler.Save(forceSaveAs);

        public void Reload() => FileHandler.Reload();

        public void OpenUrl(string address, bool isInternal = false) => OpenHelper.Open(address);

        #endregion
        
        #region Config stream initialization
        
        private FileStream GetFileStreamWithMaxPermissions(string filename)
        {
            try
            {
                // try getting write access
                return File.Open(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            }
            catch
            {
                try
                {
                    return File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite); // try again with just read access
                }
                catch
                {
                }
            }
            return null;
        }

        private Stream GetConfigStream(string path)
        {
            var stream = GetFileStreamWithMaxPermissions(path); // try to open normal settings file

            var createInUserDirectory = false;
            string userPath;
            using (var config = new ConfigFile(stream, true)) // create a temporay ConfigFile
                userPath = config.Get("config.path_redirect");
            var userDirectory = Path.GetDirectoryName(userPath);
            if (!string.IsNullOrEmpty(userPath) && Directory.Exists(userDirectory))
            {
                stream.Close(); // We don't need the old stream any more.
                stream.Dispose();
                stream = GetFileStreamWithMaxPermissions(userPath);
                createInUserDirectory = true;
                
                if (stream == null)
                    PreBootstrapWarnings.Add($"Anlegen der angeforderten Einstellungsdatei {userPath} ist fehlgeschlagen.");
            }
            else if (!string.IsNullOrEmpty(userPath))
                PreBootstrapWarnings.Add($"Der als config.path_redirect angegebene Ordnerpfad {userDirectory} existiert nicht! Verwende die Einstellungsdatei im Programmverzeichnis.");

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
                PreBootstrapWarnings.Add("Keine Einstellungsdatei zum Schreiben gefunden. Änderungen an Programmeinstellungen werden verworfen.");

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
}