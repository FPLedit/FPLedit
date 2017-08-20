using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.Kursbuch
{
    [Plugin("Modul für Tabellenfahrpläne", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem showItem, filterItem, settingsItem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            var item = new ToolStripMenuItem("Kursbuch");
            info.Menu.Items.Add(item);
            showItem = item.DropDownItems.Add("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

            filterItem = item.DropDownItems.Add("Filterregeln");
            filterItem.Enabled = false;
            filterItem.Click += FilterItem_Click;

            settingsItem = item.DropDownItems.Add("Darstellung");
            settingsItem.Enabled = false;
            settingsItem.Click += SettingsItem_Click;
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            HtmlExport exp = new HtmlExport();
            string path = info.GetTemp("afpl.html");

            bool tryoutConsole = info.Settings.Get<bool>("afpl.console");
            if (tryoutConsole)
                exp.ExportTryoutConsole(info.Timetable, path, info);
            else
                exp.Export(info.Timetable, path, info);

            Process.Start(path);
        }

        private void FilterItem_Click(object sender, EventArgs e)
        {
            //FilterForm ff = new FilterForm(info.Timetable);
            //if (ff.ShowDialog() == DialogResult.OK)
            //    info.SetUnsaved();
        }

        private void SettingsItem_Click(object sender, EventArgs e)
        {
            //SettingsForm sf = new SettingsForm(info.Timetable, info);
            //if (sf.ShowDialog() == DialogResult.OK)
            //    info.SetUnsaved();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            filterItem.Enabled = settingsItem.Enabled = e.FileState.Opened;
            showItem.Enabled = e.FileState.LineCreated;
        }
    }
}
