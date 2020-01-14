using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using System.Diagnostics;

namespace FPLedit.Buchfahrplan.Forms
{
    public class Preview : IPreviewable
    {
        public string DisplayName => "Buchfahrplan";

        public void Show(IPluginInterface pluginInterface)
        {
            HtmlExport exp = new HtmlExport();
            string path = pluginInterface.GetTemp("buchfahrplan.html");

            bool tryoutConsole = pluginInterface.Settings.Get<bool>("bfpl.console");
            bool success = exp.Exp(pluginInterface.Timetable, path, pluginInterface, tryoutConsole);

            if (success)
                OpenHelper.Open(path);
        }
    }
}
