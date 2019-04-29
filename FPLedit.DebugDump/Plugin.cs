using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.DebugDump
{
    [Plugin("DebugDump", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private string basePath;
        private DirectoryInfo dir;
        private int sessionCounter = 0;
        private string session;
        private IInfo info;

        public void Init(IInfo info)
        {
            XMLEntity last = new XMLEntity("dummy");

            this.info = info;

            session = Guid.NewGuid().ToString();
            basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "ttDumps", session);
            dir = new DirectoryInfo(basePath);

            if (!dir.Exists)
                dir.Create();

            // Log UI interaction
            FFormHandler.Init += (se, a) =>
            {
                var w = (Window)se;
                var n = w.GetType().FullName;
                w.Shown += (s, e) => info.Logger.Debug("Form shown: " + n);
                w.Closed += (s, e) => info.Logger.Debug("Form closed: " + n);
            };

            var logPath = Path.Combine(basePath, "session.log");
            dynamic l = info.Logger;
            l.Loggers.Add(new FileLogger(logPath));

            // Log Timetable changes
            info.FileStateChanged += (s, e) =>
            {
                try
                {
                    if (info.Timetable == null || XDiff(last, info.Timetable.XMLEntity))
                        return;

                    var clone = info.Timetable.Clone();
                    var exp = new XMLExport();

                    var fn = Path.Combine(basePath, $"fpldump-{sessionCounter++}.fpl");

                    var x = new XMLEntity("dump-info");
                    x.SetAttribute("session", session);
                    x.SetAttribute("date", DateTime.Now.ToShortDateString());
                    x.SetAttribute("time", DateTime.Now.ToShortTimeString());
                    x.SetAttribute("pf", Environment.OSVersion.Platform.ToString());

                    clone.Children.Add(x);
                    exp.Export(clone, fn, info);
                    info.Logger.Debug("Dump erstellt: " + fn);

                    var clone2 = info.Timetable.Clone();
                    last = clone2.XMLEntity;
                }
                catch { }
            };

            info.ExtensionsLoaded += (s, e) =>
            {
                info.Logger.Info("DebugDump aktiviert. Session: " + session);
                info.Logger.Debug("Enabled extensions: " + info.Settings.Get("extmgr.enabled", ""));
            };

            var tmpDir = info.GetTemp("");
            var watcher = new FileSystemWatcher(tmpDir, "*.*");
            watcher.NotifyFilter = NotifyFilters.LastWrite;
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
            info.Logger.Debug("Dump erstellt: " + fn);
            orig.CopyTo(fn);
        }

        private bool XDiff(XMLEntity x1, XMLEntity x2)
        {
            if (x1.XName != x2.XName)
                return false;
            if (x1.Value != x2.Value)
                return false;
            if (x1.Attributes.Count != x2.Attributes.Count)
                return false;
            foreach (var a in x1.Attributes)
            {
                if (a.Value != x2.GetAttribute<string>(a.Key, null))
                    return false;
            }
            if (x1.Children.Count != x2.Children.Count)
                return false;
            foreach (var c in x1.Children)
            {
                if (!XDiff(c, x2.Children[x1.Children.IndexOf(c)]))
                    return false;
            }
            return true;
        }
    }
}
