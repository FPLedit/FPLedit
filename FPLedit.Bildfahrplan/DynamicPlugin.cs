using FPLedit.Bildfahrplan.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan
{
    [Plugin("Dynamische Bildfahrplan-Vorschau", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class DynamicPlugin : IPlugin, IDisposable
    {
        private DynamicPreview dpf;
        public void Init(IPluginInterface pluginInterface)
        {
            Style.pluginInterface = pluginInterface;

            dpf = new DynamicPreview();
            pluginInterface.Register<IPreviewAction>(dpf);
            pluginInterface.AppClosing += (s, e) => dpf.Close();
        }

        public void Dispose() => dpf?.Dispose();
    }

    public sealed class DynamicPreview : IPreviewAction, IDisposable
    {
        private bool opened = false;
        private PreviewForm dpf;

        public string DisplayName => "Dynamischer Bildfahrplan";

        public void Show(IPluginInterface pluginInterface)
        {
            if (!opened)
            {
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
