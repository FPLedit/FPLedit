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
        private readonly Dictionary<object, Timetable> timetableBackup;

        public UpdateManager Update { get; }
        public FileHandler FileHandler { get; }
        public ExtensionManager ExtensionManager { get; }
        public ISettings Settings { get; }
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
            
            RootForm = rootForm;
            Menu = rootForm.Menu;
            HelpMenu = rootForm.Menu.GetItem(MainForm.LocHelpMenu);
            Settings = new Settings();
            registry = new RegisterStore();
            Update = new UpdateManager(Settings);
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
            ExtensionManager.InitActivatedExtensions();
            
            // Initialize Export/Import
            var exporters = GetRegistered<IExport>();
            var importers = GetRegistered<IImport>();
            FileHandler.InitializeExportImport(exporters, importers);
            
            // Vorlagen laden
            var templatePath = Settings.Get("tmpl.root", DEFAULT_TEMPLATE_PATH);
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
        
        public void Register<T>(T obj)
            => registry.Register(obj);

        public T[] GetRegistered<T>()
            => registry.GetRegistered<T>();

        public string GetTemp(string filename)
        {
            var dirpath = Path.Combine(Path.GetTempPath(), "fpledit");
            if (!Directory.Exists(dirpath))
                Directory.CreateDirectory(dirpath);
            return Path.Combine(dirpath, filename);
        }

        internal void ClearTemp()
        {
            var dirpath = Path.Combine(Path.GetTempPath(), "fpledit");
            if (Directory.Exists(dirpath))
                Directory.Delete(dirpath, true);
        }

        public void SetUnsaved() => FileHandler.SetUnsaved();

        public void Open() => FileHandler.Open();

        public void Save(bool forceSaveAs) => FileHandler.Save(forceSaveAs);

        public void Reload() => FileHandler.Reload();
        
        #endregion
        
        public void InjectLogger(ILog logger)
        {
            Logger = logger;
        }

        public void Dispose()
        {
            FileHandler?.Dispose();
            registry?.Dispose();
        }
    }
}