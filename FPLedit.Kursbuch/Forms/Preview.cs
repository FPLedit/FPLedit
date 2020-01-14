using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPLedit.Shared;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace FPLedit.Kursbuch.Forms
{
    public class Preview : IPreviewable
    {
        public string DisplayName => "Kursbuch";

        public void Show(IPluginInterface pluginInterface)
        {
            HtmlExport exp = new HtmlExport();
            string path = pluginInterface.GetTemp("kfpl.html");

            bool tryoutConsole = pluginInterface.Settings.Get<bool>("kfpl.console");
            bool success = exp.Exp(pluginInterface.Timetable, path, pluginInterface, tryoutConsole);

            if (success)
                OpenHelper.Open(path);
        }
    }
}
