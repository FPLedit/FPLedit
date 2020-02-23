using System;
using System.IO;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;

namespace FPLedit.DebugDump
{
    internal sealed class DebugListener : IDisposable
    {
        private FileSystemWatcher watcher;
        private DumpWriter writer;
        private string session;
        private XMLEntity lastTimetableNode;

        public void StartSession(IPluginInterface pluginInterface, string basePath)
        {
            session = Guid.NewGuid().ToString();
            var fn = Path.Combine(basePath, $"fpledit-dump-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.fpldmp");
            writer = new DumpWriter(fn);

            // Output some messages before starting so that user is informed better (and does not forget to turn it off!)
            pluginInterface.Logger.Warning("Debug Dump ist aktiviert! Dies kann dazu führen, dass große Datenmengen aufgezeichnet werden.\nSession file: " + fn + "\n----------");

            var l = pluginInterface.Logger;
            if (l.CanAttach)
                l.AttachLogger(new DumpLogger(writer));

            AddDumpUiInteraction();
            AddTimetableListener(pluginInterface);

            pluginInterface.ExtensionsLoaded += (s, e) =>
            {
                writer.WriteEvent(DumpEventType.DebugDumpInternal, "Session started", session);
                writer.WriteEvent(DumpEventType.DebugDumpInternal, "Enabled extensions", pluginInterface.Settings.Get("extmgr.enabled", ""));
            };
            pluginInterface.AppClosing += (s, e) => { writer.WriteEvent(DumpEventType.DebugDumpInternal, "Gracefully terminating session", session); };

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
            var text = "<Error reading file>";
            try
            {
                text = File.ReadAllText(orig.FullName);
            }
            catch
            {
            }

            writer.WriteEvent(DumpEventType.TempFile, orig.FullName, text);
        }

        private void AddDumpUiInteraction()
        {
            // Log UI interaction
            FFormHandler.Init += (se, a) =>
            {
                var w = (Window) se;
                var n = w.GetType().FullName;

                w.Shown += (s, e) =>
                {
                    writer.WriteEvent(DumpEventType.UiInteraction, "Form", n, "event: Shown");
                    InstrumentControlEventsRecursive(w, n);
                };
                w.Closed += (s, e) => writer.WriteEvent(DumpEventType.UiInteraction, "Form", n, "event: Closed");
            };
        }

        private void InstrumentControlEventsRecursive(Container container, string path)
        {
            foreach (var control in container.Controls)
            {
                switch (control)
                {
                    case Button btn:
                        btn.Click += (sender, args) => writer.WriteEvent(DumpEventType.UiInteraction, "Button", path + "/" + btn.Text, "event: Click");
                        break;
                    case TextBox box:
                        box.TextChanged += (sender, args) => writer.WriteEvent(DumpEventType.UiInteraction, "TextBox", path + "/" + box.ID, "event: TextChanged", box.Text);
                        break;
                    case DropDown dd:
                        dd.SelectedIndexChanged += (sender, args) => writer.WriteEvent(DumpEventType.UiInteraction, "DropDown", path + "/" + dd.ID, "event: SelectedIndexChanged", dd.SelectedIndex.ToString());
                        break;
                    case RadioButton rb:
                        rb.CheckedChanged += (sender, args) => writer.WriteEvent(DumpEventType.UiInteraction, "RadioButton", path + "/" + rb.Text, "event: CheckedChanged", rb.Checked.ToString());
                        break;
                    case CheckBox rb:
                        rb.CheckedChanged += (sender, args) => writer.WriteEvent(DumpEventType.UiInteraction, "CheckBox", path + "/" + rb.Text, "event: CheckedChanged", rb.Checked.ToString());
                        break;
                    case Container ct:
                        InstrumentControlEventsRecursive(ct, path + "/" + ct.GetType().Name);
                        break;
                }
            }
        }

        private void AddTimetableListener(IPluginInterface pluginInterface)
        {
            lastTimetableNode = new XMLEntity("dummy");

            // Log Timetable changes
            pluginInterface.FileStateChanged += (s, e) =>
            {
                try
                {
                    if (pluginInterface.Timetable == null || lastTimetableNode.XDiff(pluginInterface.Timetable.XMLEntity))
                        return;

                    var clone = pluginInterface.Timetable.Clone();

                    var x = new XMLEntity("dump-info");
                    x.SetAttribute("session", session);
                    x.SetAttribute("date", DateTime.Now.ToShortDateString());
                    x.SetAttribute("time", DateTime.Now.ToShortTimeString());
                    x.SetAttribute("pf", Environment.OSVersion.Platform.ToString());

                    clone.Children.Add(x);

                    using (var ms = new MemoryStream())
                    {
                        var exp = new XMLExport();
                        exp.Export(clone, ms, pluginInterface, new[] {XMLExport.FLAG_INDENT_XML});
                        ms.Seek(0, SeekOrigin.Begin);
                        using (var tr = new StreamReader(ms))
                        {
                            var text = tr.ReadToEnd();
                            writer.WriteEvent(DumpEventType.TimetableChange, text);
                        }
                    }

                    var clone2 = pluginInterface.Timetable.Clone();
                    lastTimetableNode = clone2.XMLEntity;
                }
                catch (Exception ex)
                {
                    pluginInterface.Logger.LogException(ex);
                }
            };
        }

        public void Dispose()
        {
            watcher?.Dispose();
            writer?.Dispose();
        }
    }
}