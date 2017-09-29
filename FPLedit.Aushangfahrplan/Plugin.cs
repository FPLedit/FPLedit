using FPLedit.Aushangfahrplan.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Aushangfahrplan
{
    [Plugin("Modul für Aushangfahrpläne", "1.5.0", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;

        public void Init(IInfo info)
        {
            this.info = info;

            info.Register<IExport>(new HtmlExport());
            info.Register<IAfplTemplate>(new Templates.AfplTemplate());
            info.Register<IAfplTemplate>(new Templates.SvgAfplTemplate());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());
            info.Register<IFilterableUi>(new FilterableHandler());
            info.Register<IPreviewable>(new Preview());
        }
    }
}
