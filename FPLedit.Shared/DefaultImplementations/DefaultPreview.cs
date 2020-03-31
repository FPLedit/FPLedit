using System;
using System.Threading.Tasks;
using Eto.Forms;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared.DefaultImplementations
{
    public sealed class DefaultPreview : IPreviewAction
    {
        private readonly string templateType;
        private readonly IExport export;

        public string DisplayName { get; }
        
        public string MenuName { get; }

        public DefaultPreview(string templateType, string display, IExport export, string menu = null)
        {
            this.templateType = templateType;
            this.export = export;
            DisplayName = display;
            if (menu == null)
                MenuName = "&" + DisplayName;
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
                    Application.Instance.Invoke(() => OpenHelper.Open(path));
            }, null, TaskScheduler.Default);
            tsk.Start();
        }
    }
}