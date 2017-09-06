using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Buchfahrplan
{
    [Plugin("Modul für Buchfahrpläne", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem velocityItem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.Register<IExport>(new HtmlExport());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());
            info.Register<IBfplTemplate>(new Templates.BuchfahrplanTemplate());
            info.Register<IBfplTemplate>(new Templates.ZLBTemplate());
            info.Register<IFilterableUi>(new Forms.FilterableHandler());
            info.Register<IPreviewable>(new Forms.Preview());

            var item = new ToolStripMenuItem("Buchfahrplan");
            info.Menu.Items.Add(item);

            velocityItem = item.DropDownItems.Add("Höchstgeschwindigkeiten ändern");
            velocityItem.Enabled = false;
            velocityItem.Click += VelocityItem_Click;
        }

        private void VelocityItem_Click(object sender, EventArgs e)
        {
            VelocityForm svf = new VelocityForm(info);
            if (svf.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            velocityItem.Enabled = e.FileState.Opened;
        }
    }
}
