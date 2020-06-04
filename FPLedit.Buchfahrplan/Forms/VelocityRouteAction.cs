using FPLedit.Shared;
using Eto.Forms;

namespace FPLedit.Buchfahrplan.Forms
{
    internal sealed class VelocityRouteAction : IRouteAction
    {
        public string DisplayName => "Höchst&geschwindigkeiten ändern";

        public void Invoke(IPluginInterface pluginInterface, Route route)
        {
            pluginInterface.StageUndoStep();
            using (var svf = new VelocityForm(pluginInterface, route))
                if (svf.ShowModal() == DialogResult.Ok)
                    pluginInterface.SetUnsaved();
        }

        public bool IsEnabled(IPluginInterface pluginInterface)
            => pluginInterface.FileState.Opened;
    }
}
