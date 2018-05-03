using FPLedit.Shared.Ui;
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

        public void Show(IInfo info, Route route)
        {
            info.StageUndoStep();
            VelocityForm svf = new VelocityForm(info, route);
            if (svf.ShowModal() == DialogResult.Ok)
                info.SetUnsaved();
        }

        public bool IsEnabled(IInfo info)
            => info.FileState.Opened;
    }
}
