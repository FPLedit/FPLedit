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
        public void Init(IInfo info)
        {
            Style.info = info;

            var dpf = new DynamicPreview();
            info.Register<IPreviewable>(dpf);
            info.AppClosing += (s, e) => dpf.Close();
        }
    }

    public class DynamicPreview : IPreviewable
    {
        private bool opened = false;
        private PreviewForm dpf;

        public string DisplayName => "Dynamischer Bildfahrplan";

        public void Show(IInfo info)
        {
            if (!opened)
            {
                var route = (info.Timetable.Type == TimetableType.Network) ? info.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
                dpf = new PreviewForm(info);
                dpf.Closed += (s, e) => opened = false;
                dpf.Show();
                opened = true;
            }
            else
                dpf.Focus();
        }

        public void Close() => dpf?.Close();
    }
}
