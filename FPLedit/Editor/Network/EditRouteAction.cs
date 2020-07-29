using Eto.Forms;
using FPLedit.Shared;

namespace FPLedit.Editor.Network
{
    internal sealed class EditRouteAction : IRouteAction
    {
        public string DisplayName => T._("Stationen dieser Strecke b&earbeiten");

        public bool IsEnabled(IPluginInterface pluginInterface)
            => pluginInterface.FileState.Opened;

        public void Invoke(IPluginInterface pluginInterface, Route route)
        {
            pluginInterface.StageUndoStep();
            using (var lef = new LineEditForm(pluginInterface, route.Index))
                if (lef.ShowModal(pluginInterface.RootForm) == DialogResult.Ok)
                    pluginInterface.SetUnsaved();
        }
    }
}
