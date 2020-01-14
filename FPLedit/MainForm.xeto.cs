using Eto.Forms;
using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.UI;
using FPLedit.Logger;
using FPLedit.Templating;
using FPLedit.Editor.Rendering;
using FPLedit.Config;
using FPLedit.Shared.Rendering;
using System.Threading.Tasks;
using FPLedit.Extensibility;

namespace FPLedit
{
    public class MainForm : FForm, IInfo, IRestartable, ILastFileHandler
    {
        #region Controls
#pragma warning disable CS0649
        private readonly LogControl logTextBox;
        private readonly ButtonMenuItem saveMenu, saveAsMenu, exportMenu, importMenu, lastMenu, fileMenu, convertMenu;
        private readonly NetworkEditingControl networkEditingControl;
#pragma warning restore CS0649
        #endregion

        private string templatePath = "templates";

        private Dictionary<object, Timetable> timetableBackup;

        private TemplateManager templateManager;
        private readonly UndoManager undo;
        private readonly RegisterStore registry;
        private readonly UpdateManager update;
        private readonly FileHandler fileHandler;
        internal readonly ExtensionManager extensionManager;
        internal readonly CrashReporting.CrashReporter crashReporter;
        private TimetableChecks.TimetableCheckRunner checkRunner;


        private List<string> lastFiles;
        private bool enable_last = true;

        public Timetable Timetable => fileHandler.Timetable;

        public ILog Logger { get; private set; }

        public ISettings Settings { get; private set; }

        #region FileState

        public IFileState FileState => fileHandler.FileState;

        public event EventHandler<FileStateChangedEventArgs> FileStateChanged;
        public event EventHandler ExtensionsLoaded;
        public event EventHandler AppClosing;
        public event EventHandler FileOpened;

        #endregion

        public MainForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            Icon = new Icon(this.GetResource("Resources.programm.ico"));

            timetableBackup = new Dictionary<object, Timetable>();

            Settings = new Settings();
            undo = new UndoManager();
            registry = new RegisterStore();
            update = new UpdateManager(Settings);
            extensionManager = new ExtensionManager(this, update);
            crashReporter = new CrashReporting.CrashReporter(this);
            fileHandler = new FileHandler(this, this, this, undo);

            networkEditingControl.Initialize(this);

            var logger = new MultipleLogger(logTextBox);
            if (Settings.Get("log.enable-file", false))
                logger.Loggers.Add(new TempLogger(this));
            if (OptionsParser.MPCompatLog)
                logger.Loggers.Add(new ConsoleLogger());
            Logger = logger;

            KeyDown += (s, e) =>
            {
                if (e.Key == Keys.R || e.Key == Keys.Escape)
                    networkEditingControl.DispatchKeystroke(e);
            };

            fileHandler.FileStateChanged += FileHandler_FileStateChanged;
            fileHandler.FileOpened += FileHandler_FileOpened;

            Init();
        }

        private void FileHandler_FileOpened(object sender, EventArgs e)
        {
            networkEditingControl.ResetPan();

            FileOpened?.Invoke(sender, e);
        }

        private void FileHandler_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            Title = "FPLedit" + (e.FileState.Opened ? " - "
                + (e.FileState.FileName != null ? (Path.GetFileName(e.FileState.FileName) + " ") : "")
                + (e.FileState.Saved ? "" : "*") : "");

            saveMenu.Enabled = saveAsMenu.Enabled = exportMenu.Enabled = convertMenu.Enabled = e.FileState.Opened;

            FileStateChanged?.Invoke(sender, e);
        }

        #region Initialization

        private void Init()
        {
            Timetable.DefaultLinearVersion = Settings.GetEnum("core.default-file-format", Timetable.DefaultLinearVersion);
            FontCollection.InitAsync(); // Asynchron Liste von verfügbaren Schriftarten laden
            EtoExtensions.Initialize(this); // UI-Erweiterungen initialiseren
            this.AddSizeStateHandler();

            // Extensions laden & initialisieren (=> Initialisiert Importer/Exporter)
            extensionManager.LoadExtensions();
            extensionManager.InitActivatedExtensions();

            InitializeExportImport();
            InitializeMenus();

            // Vorlagen laden
            templatePath = Settings.Get("tmpl.root", templatePath);
            templateManager = new TemplateManager(registry, this);
            templateManager.LoadTemplates(templatePath);
            if (OptionsParser.TemplateDebug)
                Task.Run(() => Application.Instance.Invoke(() => templateManager.DebugCompileAll()));

            ExtensionsLoaded?.Invoke(this, new EventArgs());

            Shown += (s, e) => LoadStartFile();
            Shown += (s, e) => update.AutoUpdateCheck(Logger);

            checkRunner = new TimetableChecks.TimetableCheckRunner(this); // CheckRunner initialisieren

            // Hatten wir einen Crash beim letzten Mal?
            if (crashReporter.HasCurrentReport)
            {
                Shown += (s, e) =>
                {
                    try
                    {
                        var cf = new CrashReporting.CrashForm(crashReporter);
                        if (cf.ShowModal(this) == DialogResult.Ok)
                            crashReporter.Restore(fileHandler);
                        crashReporter.RemoveCrashFlag();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Fehlermeldung des letzten Absturzes konnte nicht angezeigt werden: " + ex.Message);
                        crashReporter.RemoveCrashFlag(); // Der Crash crasht sogar noch die Fehlerbehandlung...
                    }
                };
            }
        }

        private void LoadStartFile()
        {
            // Parameter: Fpledit.exe [Dateiname] ODER Datei aus Restart
            var fn = OptionsParser.OpenFilename;
            fn = Settings.Get("restart.file", fn);
            if (fn != null && File.Exists(fn))
            {
                fileHandler.InternalOpen(fn);
                AddLastFile(fn);
            }
            Settings.Remove("restart.file");
        }

        private void InitializeExportImport()
        {
            var exporters = registry.GetRegistered<IExport>();
            var importers = registry.GetRegistered<IImport>();

            fileHandler.InitializeExportImport(exporters, importers);

            // Ggf. Import bzw. Export-Menü entfernen
            if (importers.Length == 0)
                fileMenu.Items.Remove(importMenu);
        }

        private void InitializeMenus()
        {
            // Zuletzt geöffnete Dateien anzeigen
            enable_last = Settings.Get("files.save-last", true);
            if (enable_last)
            {
                lastFiles = Settings.Get("files.last", "").Split(';').Where(s => s != "").Reverse().ToList();
                UpdateLastFilesMenu();
            }
            else
                lastMenu.Enabled = false;

            // Hilfe Menü nach den Erweiterungen zusammenbasteln
            var helpItem = Menu.CreateItem("Hilfe");
            helpItem.CreateItem("Erweiterungen", clickHandler: (s, ev) => new ExtensionsForm(extensionManager, this).ShowModal(this));
            helpItem.CreateItem("Vorlagen", clickHandler: (s, ev) => new TemplatesForm(templateManager, templatePath).ShowModal(this));
            helpItem.Items.Add(new SeparatorMenuItem());
            helpItem.CreateItem("Fenstergößen löschen", clickHandler: (s, ev) => SizeManager.Reset());
            helpItem.Items.Add(new SeparatorMenuItem());
            helpItem.CreateItem("Online Hilfe", clickHandler: (s, ev) => OpenHelper.Open("https://fahrplan.manuelhu.de/"));
            helpItem.CreateItem("Info", clickHandler: (s, ev) => new InfoForm(Settings).ShowModal(this));
        }

        private void UpdateLastFilesMenu()
        {
            lastMenu.Items.Clear();
            foreach (var lf in lastFiles)
            {
                var itm = lastMenu.CreateItem(lf);
                itm.Click += (s, a) =>
                {
                    if (fileHandler.NotifyIfUnsaved())
                        fileHandler.InternalOpen(lf);
                };
            }
        }

        public void AddLastFile(string filename)
        {
            if (enable_last)
            {
                if (!filename.EndsWith(".fpl"))
                    filename += ".fpl";

                lastFiles.RemoveAll(s => s == filename); // Doppelte Dateinamen verhindern
                lastFiles.Insert(0, filename);
                if (lastFiles.Count > 3) // Überlauf
                    lastFiles.RemoveAt(lastFiles.Count - 1);

                UpdateLastFilesMenu();
            }
        }

        #endregion

        public void RestartWithCurrentFile()
        {
            if (!fileHandler.NotifyIfUnsaved())
                return;
            if (FileState.Opened)
                Settings.Set("restart.file", FileState.FileName);

            Process.Start(System.Reflection.Assembly.GetEntryAssembly().Location);
            Program.App.Quit();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Settings.KeyExists("restart.file") && !Program.ExceptionQuit)
            {
                if (enable_last)
                    Settings.Set("files.last", string.Join(";", lastFiles));
                if (!fileHandler.NotifyIfUnsaved())
                    e.Cancel = true;

                ClearTemp();
            }

            AppClosing?.Invoke(this, null);

            base.OnClosing(e);
        }

        #region Drag'n'Drop
        protected override void OnDragEnter(DragEventArgs e)
        {
            e.Effects = DragEffects.None;

            if (!e.Data.Types.Contains("FileDrop"))
                return;

            Uri[] files = e.Data.Uris;
            if (files.Length == 1 && files[0].AbsolutePath.EndsWith(".fpl"))
                e.Effects = DragEffects.Copy;

            base.OnDragEnter(e);
        }

        protected override void OnDragDrop(DragEventArgs e)
        {
            Uri[] files = e.Data.Uris;
            if (files == null || files.Length != 1 || !files[0].AbsolutePath.EndsWith(".fpl"))
                return;

            if (!fileHandler.NotifyIfUnsaved())
                return;
            fileHandler.InternalOpen(files[0].LocalPath);

            base.OnDragDrop(e);
        }
        #endregion

        #region Backup & Undo
        public void Undo()
        {
            if (undo.CanGoBack)
                fileHandler.Timetable = undo.Undo();
            fileHandler.FileState.Saved = false;
        }

        public void StageUndoStep()
            => undo.StageUndoStep(Timetable);

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
            fileHandler.Timetable = backupTt;
            ClearBackup(backupHandle);
        }

        public void ClearBackup(object backupHandle)
        {
            timetableBackup.Remove(backupHandle);
        }
        #endregion

        #region IInfo
        dynamic IInfo.Menu => Menu;

        public ITemplateManager TemplateManager => templateManager;

        public dynamic RootForm => this;

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

        private void ClearTemp()
        {
            var dirpath = Path.Combine(Path.GetTempPath(), "fpledit");
            if (Directory.Exists(dirpath))
                Directory.Delete(dirpath, true);
        }

        public void SetUnsaved() => fileHandler.SetUnsaved();

        public void Open() => fileHandler.Open();

        public void Save(bool forceSaveAs) => fileHandler.Save(forceSaveAs);

        public void Reload() => fileHandler.Reload();
        #endregion

        #region Events
        private void SaveMenu_Click(object sender, EventArgs e)
            => fileHandler.Save(false);

        private void OpenMenu_Click(object sender, EventArgs e)
            => fileHandler.Open();

        private void SaveAsMenu_Click(object sender, EventArgs e)
            => fileHandler.Save(true);

        private void ImportMenu_Click(object sender, EventArgs e)
            => fileHandler.Import();

        private void ExportMenu_Click(object sender, EventArgs e)
            => fileHandler.Export();

        private void QuitMenu_Click(object sender, EventArgs e)
            => Close();

        private void LinearNewMenu_Click(object sender, EventArgs e)
            => fileHandler.New(TimetableType.Linear);

        private void NetworkNewMenu_Click(object sender, EventArgs e)
            => fileHandler.New(TimetableType.Network);

        private void ConvertMenu_Click(object sender, EventArgs e)
            => fileHandler.ConvertTimetable();
        #endregion

        protected override void Dispose(bool disposing)
        {
            fileHandler?.Dispose();
            checkRunner?.Dispose();
            registry?.Dispose();
            base.Dispose(disposing);
        }
    }
}
