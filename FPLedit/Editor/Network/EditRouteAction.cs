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

        public bool IsEnabled(IInfo info)
            => info.FileState.Opened;

        public void Show(IInfo info, Route route)
        {
            info.StageUndoStep();
            LineEditForm lef = new LineEditForm(info, route.Index);
            if (lef.ShowModal(info.RootForm) == DialogResult.Ok)
                info.SetUnsaved();
        }
    }
}
