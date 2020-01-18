using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;
using System;
using System.IO;

namespace FPLedit.DebugDump
{
    [Plugin("DebugDump", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin, IDisposable
    {
        private string basePath;
        private DirectoryInfo dir;
        private int sessionCounter = 0;
        private string session;
        private IPluginInterface pluginInterface;
        private FileSystemWatcher watcher;

        public void Init(IPluginInterface pluginInterface)
        {
            XMLEntity last = new XMLEntity("dummy");

            this.pluginInterface = pluginInterface;

            session = Guid.NewGuid().ToString();
            basePath = Path.Combine(pluginInterface.ExecutableDir, "ttDumps", session);
            dir = new DirectoryInfo(basePath);

            if (!dir.Exists)
                dir.Create();

            // Log UI interaction
            FFormHandler.Init += (se, a) =>
            {
                var w = (Window)se;
                var n = w.GetType().FullName;
                w.Shown += (s, e) => pluginInterface.Logger.Debug("Form shown: " + n);
                w.Closed += (s, e) => pluginInterface.Logger.Debug("Form closed: " + n);
            };

            var logPath = Path.Combine(basePath, "session.log");
            dynamic l = pluginInterface.Logger;
            l.Loggers.Add(new FileLogger(logPath));

            // Log Timetable changes
            pluginInterface.FileStateChanged += (s, e) =>
            {
                try
                {
                    if (pluginInterface.Timetable == null || last.XDiff(pluginInterface.Timetable.XMLEntity))
                        return;

                    var clone = pluginInterface.Timetable.Clone();
                    var exp = new XMLExport();

                    var fn = Path.Combine(basePath, $"fpldump-{sessionCounter++}.fpl");

                    var x = new XMLEntity("dump-info");
                    x.SetAttribute("session", session);
                    x.SetAttribute("date", DateTime.Now.ToShortDateString());
                    x.SetAttribute("time", DateTime.Now.ToShortTimeString());
                    x.SetAttribute("pf", Environment.OSVersion.Platform.ToString());

                    clone.Children.Add(x);
                    exp.Export(clone, fn, pluginInterface);
                    pluginInterface.Logger.Debug("Dump erstellt: " + fn);

                    var clone2 = pluginInterface.Timetable.Clone();
                    last = clone2.XMLEntity;
                }
                catch {}
            };

            pluginInterface.ExtensionsLoaded += (s, e) =>
            {
                pluginInterface.Logger.Info("DebugDump aktiviert. Session: " + session);
                pluginInterface.Logger.Debug("Enabled extensions: " + pluginInterface.Settings.Get("extmgr.enabled", ""));
            };

            var tmpDir = pluginInterface.GetTemp("");
            watcher = new FileSystemWatcher(tmpDir, "*.*")
            {
                NotifyFilter = NotifyFilters.LastWrite
            };
            watcher.Changed += WatcherEvent;
            watcher.Created += WatcherEvent;
            watcher.EnableRaisingEvents = true;
        }

        private void WatcherEvent(object sender, FileSystemEventArgs e)
        {
            var orig = new FileInfo(e.FullPath);
            if (!orig.Exists)
                return;
            var fn = Path.Combine(basePath, $"tmpdump-{sessionCounter++}-{Path.GetFileNameWithoutExtension(orig.Name)}{orig.Extension}");
            pluginInterface.Logger.Debug("Dump erstellt: " + fn);
            orig.CopyTo(fn);
        }

        public void Dispose() => watcher?.Dispose();
    }
}
