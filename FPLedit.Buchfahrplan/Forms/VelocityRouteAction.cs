using Eto.Drawing;
using FPLedit.Shared;
using Eto.Forms;

namespace FPLedit.Buchfahrplan.Forms
{
    internal sealed class VelocityRouteAction : IRouteAction
    {
        public string DisplayName => T._("Höchst&geschwindigkeiten ändern");

        public dynamic EtoIconBitmap => new Bitmap(ResourceHelper.GetResource("Buchfahrplan.Resources.toolbar-vmax.png"));

        public void Invoke(IPluginInterface pluginInterface, Route? route)
        {
            if (route == null) return;

            pluginInterface.StageUndoStep();
            using var svf = new VelocityForm(pluginInterface, route);
            if (svf.ShowModal() == DialogResult.Ok)
                pluginInterface.SetUnsaved();
        }

        public bool IsEnabled(IPluginInterface pluginInterface)
            => pluginInterface.FileState.Opened;
    }
}
