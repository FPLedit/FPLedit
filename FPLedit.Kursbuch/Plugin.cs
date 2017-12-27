using FPLedit.Kursbuch.Forms;
using FPLedit.Kursbuch.Templates;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.Kursbuch
{
    [Plugin("Modul für Tabellenfahrpläne", "1.5.0", "2.0", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;

        public void Init(IInfo info)
        {
            this.info = info;

            info.Register<IPreviewable>(new Preview());
            info.Register<IFilterableUi>(new FilterableHandler());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());

            info.Register<ITemplateProxy>(new TemplateProxy());
        }
    }
}
