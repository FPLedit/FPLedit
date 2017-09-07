using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using System.Windows.Forms;

namespace FPLedit.Buchfahrplan.Forms
{
    public class VelocityDialogProxy : IEditingDialog
    {
        public string DisplayName => "Höchstgeschwindigkeiten ändern";

        public object GroupObject => new object();

        public void Show(IInfo info)
        {
            VelocityForm svf = new VelocityForm(info);
            if (svf.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        public bool IsEnabled(IInfo info)
            => info.FileState.Opened;
    }
}
