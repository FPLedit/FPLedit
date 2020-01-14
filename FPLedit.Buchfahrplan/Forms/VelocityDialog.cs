﻿using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using Eto.Forms;

namespace FPLedit.Buchfahrplan.Forms
{
    public class VelocityDialogProxy : IRouteAction
    {
        public string DisplayName => "Höchstgeschwindigkeiten ändern";

        public void Show(IPluginInterface pluginInterface, Route route)
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
