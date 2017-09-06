﻿using FPLedit.Shared;
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
        private ToolStripItem showItem, velocityItem;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.Register<IExport>(new HtmlExport());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());
            info.Register<IBfplTemplate>(new Templates.BuchfahrplanTemplate());
            info.Register<IBfplTemplate>(new Templates.ZLBTemplate());

            var item = new ToolStripMenuItem("Buchfahrplan");
            info.Menu.Items.Add(item);

            showItem = item.DropDownItems.Add("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

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
            showItem.Enabled = e.FileState.LineCreated;
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            HtmlExport exp = new HtmlExport();
            string path = info.GetTemp("buchfahrplan.html");

            bool tryoutConsole = info.Settings.Get<bool>("bfpl.console");
            if (tryoutConsole)
                exp.ExportTryoutConsole(info.Timetable, path, info);
            else
                exp.Export(info.Timetable, path, info);
            Process.Start(path);
        }
    }
}
