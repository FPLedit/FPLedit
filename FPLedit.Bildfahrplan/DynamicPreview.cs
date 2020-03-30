using System;
using FPLedit.Bildfahrplan.Forms;
using FPLedit.Shared;

namespace FPLedit.Bildfahrplan
{
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

        public void Dispose()
        {
            if (dpf != null && !dpf.IsDisposed)
                dpf.Dispose();
        }
    }
}