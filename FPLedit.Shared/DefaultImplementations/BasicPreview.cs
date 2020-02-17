using System;
using System.Threading.Tasks;
using Eto.Forms;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared.DefaultImplementations
{
    public sealed class BasicPreview : IPreviewProxy
    {
        private readonly string templateType;
        private readonly IExport export;

        public string DisplayName { get; }

        public BasicPreview(string templateType, string display, IExport export)
        {
            this.templateType = templateType;
            this.export = export;
            DisplayName = display;
        }

        public void Show(IPluginInterface pluginInterface)
        {
            string path = pluginInterface.GetTemp(templateType + ".html");

            var tryoutConsole = pluginInterface.Settings.Get<bool>(templateType + ".console");

            var clone = pluginInterface.Timetable.Clone();

            var tsk = export.GetAsyncSafeExport(clone, path, pluginInterface, tryoutConsole ? new[] {BasicTemplateExport.FLAG_TYROUT_CONSOLE} : Array.Empty<string>());
            tsk.ContinueWith((t, o) =>
            {
                if (t.Result)
                    Application.Instance.Invoke(() => OpenHelper.Open(path));
            }, null, TaskScheduler.Default);
            tsk.Start();
        }
    }
}