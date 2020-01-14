using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using System.Diagnostics;

namespace FPLedit.Aushangfahrplan.Forms
{
    public class Preview : IPreviewable
    {
        public string DisplayName => "Aushangfahrplan";

        public void Show(IPluginInterface pluginInterface)
        {
            HtmlExport exp = new HtmlExport();
            string path = pluginInterface.GetTemp("afpl.html");

            bool tryoutConsole = pluginInterface.Settings.Get<bool>("afpl.console");
            bool success = exp.Exp(pluginInterface.Timetable, path, pluginInterface, tryoutConsole);

            if (success)
                OpenHelper.Open(path);
        }
    }
}
