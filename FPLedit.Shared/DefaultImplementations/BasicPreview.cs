using System;
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

            bool tryoutConsole = pluginInterface.Settings.Get<bool>(templateType + ".console");
            bool success = export.Export(pluginInterface.Timetable, path, pluginInterface, tryoutConsole ? new [] { "tryout_console" } : Array.Empty<string>());

            if (success)
                OpenHelper.Open(path);
        }
    }
}