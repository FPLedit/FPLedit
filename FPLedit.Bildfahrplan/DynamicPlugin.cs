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
        public void Init(IPluginInterface pluginInterface)
        {
            Style.pluginInterface = pluginInterface;

            var dpf = new DynamicPreview();
            pluginInterface.Register<IPreviewable>(dpf);
            pluginInterface.AppClosing += (s, e) => dpf.Close();
        }
    }

    public sealed class DynamicPreview : IPreviewable, IDisposable
    {
        private bool opened = false;
        private PreviewForm dpf;

        public string DisplayName => "Dynamischer Bildfahrplan";

        public void Show(IPluginInterface pluginInterface)
        {
            if (!opened)
            {
                var route = (pluginInterface.Timetable.Type == TimetableType.Network) ? pluginInterface.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
                dpf = new PreviewForm(pluginInterface);
                dpf.Closed += (s, e) => opened = false;
                dpf.Show();
                opened = true;
            }
            else
                dpf.Focus();
        }

        public void Close() => dpf?.Close();

        public void Dispose() => dpf?.Dispose();
    }
}
