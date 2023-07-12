using System;
using System.IO;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;

namespace FPLedit.DebugDump;

internal sealed class DebugListener : IDisposable
{
    private FileSystemWatcher? watcher;
    private readonly DumpWriter writer;
    private readonly string session;
    private XMLEntity? lastTimetableNode;
    private readonly string fn;

    public DebugListener(string basePath)
    {
        session = Guid.NewGuid().ToString();
        fn = Path.Combine(basePath, $"fpledit-dump-{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.fpldmp");
        writer = new DumpWriter(fn);
    }

    public void StartSession(IPluginInterface pluginInterface)
    {
        // Output some messages before starting so that user is informed better (and does not forget to turn it off!)
        pluginInterface.Logger.Warning(T._("Debug Dump ist aktiviert! Dies kann dazu führen, dass große Datenmengen aufgezeichnet werden.\nSession file: {0}\n----------", fn));

        var l = pluginInterface.Logger;
        if (l.CanAttach)
            l.AttachLogger(new DumpLogger(writer));

        AddDumpUiInteraction();
        AddTimetableListener(pluginInterface);

        pluginInterface.ExtensionsLoaded += (_, _) =>
        {
            writer.WriteEvent(DumpEventType.DebugDumpInternal, "Session started", session);
            writer.WriteEvent(DumpEventType.DebugDumpInternal, "Enabled extensions", pluginInterface.Settings.Get("extmgr.enabled", ""));
        };
        pluginInterface.AppClosing += (_, _) => { writer.WriteEvent(DumpEventType.DebugDumpInternal, "Gracefully terminating session", session); };

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
        FFormHandler.Init += (se, _) =>
        {
            var w = (Window) se!;
            var n = w.GetType().FullName ?? "null-Type";

            w.Shown += (_, _) =>
            {
                writer.WriteEvent(DumpEventType.UiInteraction, "Form", n, "event: Shown");
                InstrumentControlEventsRecursive(w, n);
            };
            w.Closed += (_, _) => writer.WriteEvent(DumpEventType.UiInteraction, "Form", n, "event: Closed");
        };
    }

    private void InstrumentControlEventsRecursive(Container container, string path)
    {
        foreach (var control in container.Controls)
        {
            switch (control)
            {
                case Button btn:
                    btn.Click += (_, _) => writer.WriteEvent(DumpEventType.UiInteraction, "Button", path + "/" + btn.Text, "event: Click");
                    break;
                case TextBox box:
                    box.TextChanged += (_, _) => writer.WriteEvent(DumpEventType.UiInteraction, "TextBox", path + "/" + box.ID, "event: TextChanged", box.Text);
                    break;
                case DropDown dd:
                    dd.SelectedIndexChanged += (_, _) => writer.WriteEvent(DumpEventType.UiInteraction, "DropDown", path + "/" + dd.ID, "event: SelectedIndexChanged", dd.SelectedIndex.ToString());
                    break;
                case RadioButton rb:
                    rb.CheckedChanged += (_, _) => writer.WriteEvent(DumpEventType.UiInteraction, "RadioButton", path + "/" + rb.Text, "event: CheckedChanged", rb.Checked.ToString());
                    break;
                case CheckBox rb:
                    rb.CheckedChanged += (_, _) => writer.WriteEvent(DumpEventType.UiInteraction, "CheckBox", path + "/" + rb.Text, "event: CheckedChanged", rb.Checked!.Value.ToString());
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
        pluginInterface.FileStateChanged += (_, _) =>
        {
            try
            {
                if (pluginInterface.TimetableMaybeNull == null || lastTimetableNode.XDiff(pluginInterface.Timetable.XMLEntity))
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
        writer.Dispose();
    }
}