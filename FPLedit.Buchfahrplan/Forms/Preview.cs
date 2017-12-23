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

        public void Show(IInfo info)
        {
            HtmlExport exp = new HtmlExport();
            string path = info.GetTemp("buchfahrplan.html");

            bool tryoutConsole = info.Settings.Get<bool>("bfpl.console");
            bool success = false;
            if (tryoutConsole)
                success = exp.ExportTryoutConsole(info.Timetable, path, info);
            else
                success = exp.Export(info.Timetable, path, info);

            if (success)
                Process.Start(path);
        }
    }
}
