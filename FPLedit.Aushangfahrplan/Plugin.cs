using FPLedit.AushangfahrplanExport.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.AushangfahrplanExport
{
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem showItem, settingsItem;

        public string Name => "Exporter für Aushangfahrpläne";

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.RegisterExport(new HtmlExport());

            ToolStripMenuItem item = new ToolStripMenuItem("Aushangfahrplan");
            info.Menu.Items.AddRange(new[] { item });
            showItem = item.DropDownItems.Add("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

            settingsItem = item.DropDownItems.Add("Aushangfahrplan-Darstellung");
            settingsItem.Enabled = false;
            settingsItem.Click += SettingsItem_Click;
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            HtmlExport exp = new HtmlExport();
            string path = info.GetTemp("aushangfahrplan.html");

            exp.Export(info.Timetable, path, info);
            Process.Start(path);
        }

        private void SettingsItem_Click(object sender, EventArgs e)
        {
            SettingsForm sf = new SettingsForm(info.Timetable);
            if (sf.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            settingsItem.Enabled = e.FileState.Opened;
            showItem.Enabled = e.FileState.LineCreated;
        }
    }
}
