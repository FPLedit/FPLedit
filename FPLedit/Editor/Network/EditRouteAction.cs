using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Editor.Network
{
    public class EditRouteAction : IRouteAction
    {
        public string DisplayName => "Stationen dieser Strecke bearbeiten";

        public bool IsEnabled(IPluginInterface pluginInterface)
            => pluginInterface.FileState.Opened;

        public void Show(IPluginInterface pluginInterface, Route route)
        {
            pluginInterface.StageUndoStep();
            using (var lef = new LineEditForm(pluginInterface, route.Index))
                if (lef.ShowModal(pluginInterface.RootForm) == DialogResult.Ok)
                    pluginInterface.SetUnsaved();
        }
    }
}
