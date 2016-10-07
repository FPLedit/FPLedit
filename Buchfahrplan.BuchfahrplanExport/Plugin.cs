using Buchfahrplan.BildfahrplanExport;
using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Buchfahrplan.BuchfahrplanHtmlExport
{
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem showItem, velocityItem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.RegisterExport(new HtmlExport());

            ToolStripMenuItem item = new ToolStripMenuItem("Buchfahrplan");
            info.Menu.Items.AddRange(new[] { item });
            showItem = item.DropDownItems.Add("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

            velocityItem = item.DropDownItems.Add("Höchstgeschwindigkeiten ändern");
            velocityItem.Enabled = false;
            velocityItem.Click += VelocityItem_Click;
        }

        private void VelocityItem_Click(object sender, EventArgs e)
        {
            StationVelocityForm svf = new StationVelocityForm();
            svf.Init(info);
            if (info.ShowDialog(svf) == DialogResult.OK)
                info.SetUnsaved();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            showItem.Enabled = velocityItem.Enabled = e.FileState.Opened;
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            HtmlExport exp = new HtmlExport();
            string path = Path.Combine(Path.GetTempPath(), "buchfahrplan.html");
            exp.Export(info.Timetable, path, new ConsoleLogger());
            Process.Start(path);
        }
    }
}
