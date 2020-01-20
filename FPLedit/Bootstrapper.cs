using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Eto.Forms;
using FPLedit.Config;
using FPLedit.Extensibility;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Templating;

namespace FPLedit
{
    internal class Bootstrapper : IDisposable, IPluginInterface
    {
        private const string DEFAULT_TEMPLATE_PATH = "templates";

        private readonly RegisterStore registry;
        private readonly UndoManager undo;
        private readonly Dictionary<object, Timetable> timetableBackup;
        internal readonly UpdateManager update;
        
        public FileHandler FileHandler { get; }
        public ExtensionManager ExtensionManager { get; }
        public ISettings Settings { get; }
        public ITemplateManager TemplateManager { get; private set; }
        public ILog Logger { get; private set; }
        public dynamic RootForm { get; }
        public dynamic Menu { get; }

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
            Settings = new Settings();
            registry = new RegisterStore();
            update = new UpdateManager(Settings);
            undo = new UndoManager();
            ExtensionManager = new ExtensionManager(this, update);
            FileHandler = new FileHandler(this, lfh, undo);
            
            FileHandler.FileOpened += (o, args) => FileOpened?.Invoke(o, args);
            FileHandler.FileStateChanged += (o, args) => FileStateChanged?.Invoke(o, args);
        }

        public void BootstrapExtensions()
        {
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
                throw new Exception("Invalid timetable backup handle!");
            FileHandler.Timetable = backupTt;
            ClearBackup(backupHandle);
        }
        
        #endregion

        #region IPluginInterface Misc Implementations
        
        public string ExecutablePath => Assembly.GetEntryAssembly().Location;

        public string ExecutableDir => Path.GetDirectoryName(ExecutablePath);
        
        public void Register<T>(T obj)
            => registry.Register<T>(obj);

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