using FPLedit.Aushangfahrplan.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Aushangfahrplan
{
    [Plugin("Modul für Aushangfahrpläne", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem showItem, filterItem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.Register<IExport>(new HtmlExport());
            info.Register<IAfplTemplate>(new Templates.AfplTemplate());
            info.Register<IAfplTemplate>(new Templates.SvgAfplTemplate());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());

            var item = new ToolStripMenuItem("Aushangfahrplan");
            info.Menu.Items.Add(item);
            showItem = item.DropDownItems.Add("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

            filterItem = item.DropDownItems.Add("Filterregeln");
            filterItem.Enabled = false;
            filterItem.Click += FilterItem_Click;
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
            FilterForm ff = new FilterForm(info.Timetable);
            if (ff.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            filterItem.Enabled = e.FileState.Opened;
            showItem.Enabled = e.FileState.LineCreated;
        }
    }
}
