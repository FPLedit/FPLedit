using System;
using System.Threading.Tasks;
using Eto.Forms;

namespace FPLedit.Shared.DefaultImplementations;

public sealed class DefaultPreview : IPreviewAction
{
    private readonly string templateType;
    private readonly IExport export;

    public string DisplayName { get; }

    public string MenuName { get; }

    public DefaultPreview(string templateType, string display, IExport export, string? menu = null)
    {
        this.templateType = templateType;
        this.export = export;
        DisplayName = display;
        MenuName = menu ?? "&" + DisplayName;
    }

    public void Show(IPluginInterface pluginInterface)
    {
        string path = pluginInterface.GetTemp(templateType + ".html");

        var tryoutConsole = pluginInterface.Settings.Get<bool>(templateType + ".console");

        var clone = pluginInterface.Timetable.Clone();

        var tsk = export.GetAsyncSafeExport(clone, path, pluginInterface, tryoutConsole ? new[] {DefaultTemplateExport.FLAG_TYROUT_CONSOLE} : Array.Empty<string>());
        tsk.ContinueWith((t, o) =>
        {
            if (t.Result)
                Application.Instance.Invoke(() => pluginInterface.OpenUrl(path, true));
        }, null, TaskScheduler.Default);
        tsk.Start();
    }
}