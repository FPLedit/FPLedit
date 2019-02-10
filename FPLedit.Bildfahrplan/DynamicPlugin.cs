using FPLedit.Bildfahrplan.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan
{
    [Plugin("Dynamische Bildfahrplan-Vorschau", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class DynamicPlugin : IPlugin
    {
        private IInfo info;

        public void Init(IInfo info)
        {
            this.info = info;
            Style.info = info;

            info.Register<IPreviewable>(new DynamicPreview());
        }
    }

    public class DynamicPreview : IPreviewable
    {
        private bool opened = false;
        private DynamicPreviewForm dpf;

        public string DisplayName => "Dynamischer Bildfahrplan";

        public void Show(IInfo info)
        {
            if (!opened)
            {
                var route = (info.Timetable.Type == TimetableType.Network) ? info.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
                dpf = new DynamicPreviewForm(info);
                dpf.Closed += (s, e) => opened = false;
                dpf.Show();
                opened = true;
            }
            else
                dpf.Focus();
        }
    }
}
