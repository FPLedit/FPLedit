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
        private ToolStripItem showItem;
        private Renderer renderer;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;
            ToolStripMenuItem item = new ToolStripMenuItem("Bildfahrplan");
            info.Menu.Items.AddRange(new[] { item });
            showItem = item.DropDownItems.Add("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            showItem.Enabled = e.Opened;
            if (e.Opened)
                renderer = new Renderer(info.Timetable);
            else
                renderer = null;
        }

        private void ShowItem_Click(object sender, EventArgs e)
        {
            Form frm = new Form();
            frm.Height = 1000;
            frm.Width = 1000;
            frm.Show();
            frm.AutoScroll = true;

            Panel panel = new Panel();
            panel.Width = frm.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            panel.Height = renderer.GetHeight();
            frm.Controls.Add(panel);

            panel.Paint += Frm_Paint;
        }

        private void Frm_Paint(object sender, PaintEventArgs e)
        {
            renderer.Draw(e.Graphics);
        }        
    }
}
