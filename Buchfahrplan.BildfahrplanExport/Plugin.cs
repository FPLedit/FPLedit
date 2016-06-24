using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Buchfahrplan.BildfahrplanExport
{
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem showItem, configItem;
        private Renderer renderer;
        private Form frm;
        private Panel panel;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            ToolStripMenuItem item = new ToolStripMenuItem("Bildfahrplan");
            info.Menu.Items.AddRange(new[] { item });

            showItem = item.DropDownItems.Add("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

            configItem = item.DropDownItems.Add("Darstellung ändern");
            configItem.Enabled = false;
            configItem.Click += ConfigItem_Click;
        }

        private void ConfigItem_Click(object sender, EventArgs e)
        {
            ConfigForm cnf = new ConfigForm();
            cnf.Init(info.Timetable);
            DialogResult res = info.ShowDialog(cnf);
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            showItem.Enabled = e.Opened;
            configItem.Enabled = e.Opened;
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            renderer = new Renderer(info.Timetable);
            frm = new Form();
            frm.Height = 1000;
            frm.Width = 1000;
            frm.ShowIcon = false;
            frm.ShowInTaskbar = false;
            frm.Text = "Bildfahrplan";
            frm.MaximizeBox = false;
            frm.AutoScroll = true;
            frm.AutoSize = false;

            DateControl dtc = new DateControl(info.Timetable);
            dtc.Dock = DockStyle.Top;
            dtc.ValueChanged += Dtc_ValueChanged;
            frm.Controls.Add(dtc);

            panel = new Panel();
            panel.Width = frm.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            panel.Height = renderer.GetHeight();
            panel.Top = dtc.Height;
            frm.Controls.Add(panel);
            panel.Paint += Panel_Paint;

            info.ShowDialog(frm);
        }

        private void Dtc_ValueChanged(object sender, EventArgs e)
        {
            renderer.Init();
            panel.Height = renderer.GetHeight();
            renderer.Draw(panel.CreateGraphics());
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            frm.Width = 1000;
            frm.Height = 1000;
            renderer.Draw(e.Graphics);
        }
    }
}
