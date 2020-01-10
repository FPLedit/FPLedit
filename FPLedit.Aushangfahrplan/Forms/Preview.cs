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

        public void Show(IInfo info)
        {
            HtmlExport exp = new HtmlExport();
            string path = info.GetTemp("afpl.html");

            bool tryoutConsole = info.Settings.Get<bool>("afpl.console");
            bool success = exp.Exp(info.Timetable, path, info, tryoutConsole);

            if (success)
                OpenHelper.Open(path);
        }
    }
}
