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
    internal sealed class Bootstrapper : IPluginInterface, IReducedPluginInterface, IDisposable
    {
        private const string DEFAULT_TEMPLATE_PATH = "templates";

        private readonly RegisterStore registry;
        private readonly UndoManager undo;
        private readonly Settings settings;
        private readonly Dictionary<object, Timetable> timetableBackup;

        public UpdateManager Update { get; }
        public FileHandler FileHandler { get; }
        public ExtensionManager ExtensionManager { get; }
        ISettings IPluginInterface.Settings => settings;
        IReadOnlySettings IReducedPluginInterface.Settings => settings;
        public ISettings FullSettings => settings;
        public ICacheFile Cache => FileHandler.Cache;
        public ITemplateManager TemplateManager { get; private set; }
        public ILog Logger { get; private set; }
        public dynamic RootForm { get; }
        public dynamic Menu { get; }
        public dynamic HelpMenu { get; }

        public Timetable Timetable => FileHandler.Timetable;
        public IFileState FileState => FileHandler.FileState;

        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        public event EventHandler ExtensionsLoaded;
        public event EventHandler AppClosing;
        public event EventHandler FileOpened;

        public Bootstrapper(Window rootForm, LastFileHandler lfh)
        {
            timetableBackup = new Dictionary<object, Timetable>();
            
            var configPath = Path.Combine(PathManager.Instance.AppDirectory, "fpledit.conf");
            
            RootForm = rootForm;
            Menu = rootForm.Menu;
            HelpMenu = rootForm.Menu.GetItem(MainForm.LocHelpMenu);
            settings = new Settings(GetConfigStream(configPath));
            registry = new RegisterStore();
            Update = new UpdateManager(settings);
            undo = new UndoManager();
            ExtensionManager = new ExtensionManager(this, Update);
            FileHandler = new FileHandler(this, lfh, undo);
            
            FileHandler.FileOpened += (o, args) => FileOpened?.Invoke(o, args);
            FileHandler.FileStateChanged += (o, args) => FileStateChanged?.Invoke(o, args);
        }

        public void BootstrapExtensions()
        {
            if (Logger == null)
                throw new InvalidOperationException("Bootstrapper was not fully initialized before attempted to load extensions!");
            
            // Extensions laden & initialisieren (=> Initialisiert Importer/Exporter)
            ExtensionManager.LoadExtensions();
            ExtensionManager.InitActivatedExtensions(registry);
            
            // Initialize Export/Import
            var exporters = GetRegistered<IExport>();
            var importers = GetRegistered<IImport>();
            FileHandler.InitializeExportImport(exporters, importers);
            
            // Vorlagen laden
            var templatePath = settings.Get("tmpl.root", DEFAULT_TEMPLATE_PATH);
            var templateManager = new TemplateManager(registry, this, templatePath);
            templateManager.LoadTemplates(templatePath);
            if (OptionsParser.TemplateDebug)
                Task.Run(() => Application.Instance.Invoke(() => templateManager.DebugCompileAll()));
            TemplateManager = templateManager;

            ExtensionsLoaded?.Invoke(this, new EventArgs());
            
            (RootForm as Window).Closing += (s, e) => AppClosing?.Invoke(this, null);
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

        public string ExecutablePath => PathManager.Instance.AppFilePath;

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
            var config = new ConfigFile(stream, true); // create a temporay ConfigFile
            var userPath = config.Get("config.path_redirect");
            config.Dispose();
            if (userPath != null && File.Exists(userPath))
            {
                stream.Close(); // We don't need the old stream any more.
                stream.Dispose();
                stream = GetFileStreamWithMaxPermissions(userPath);
            }

            return stream;
        }
        
        #endregion
        
        public void InjectLogger(ILog logger)
        {
            Logger = logger;
        }

        public void Dispose()
        {
            FileHandler?.Dispose();
            registry?.Dispose();
            settings?.Dispose();
        }
    }
}