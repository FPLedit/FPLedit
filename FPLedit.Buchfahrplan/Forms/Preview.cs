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
            bool success = exp.Exp(info.Timetable, path, info, tryoutConsole);

            if (success)
                Process.Start(path);
        }
    }
}
