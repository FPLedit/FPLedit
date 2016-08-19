using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Buchfahrplan.BildfahrplanExport
{
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem showItem, configItem, trainColorItem, printItem;
        private Renderer renderer;
        private Form frm;
        private Panel panel;
        private PrintDocument doc;
        private TimeSpan? last;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.RegisterExport(new BitmapExport());

            ToolStripMenuItem item = new ToolStripMenuItem("Bildfahrplan");
            info.Menu.Items.AddRange(new[] { item });

            showItem = item.DropDownItems.Add("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

            configItem = item.DropDownItems.Add("Darstellung ändern");
            configItem.Enabled = false;
            configItem.Click += ConfigItem_Click;

            printItem = item.DropDownItems.Add("Drucken");
            printItem.Enabled = false;
            printItem.Click += PrintItem_Click;

            trainColorItem = item.DropDownItems.Add("Zugdarstellung ändern");
            trainColorItem.Enabled = false;
            trainColorItem.Click += TrainColorItem_Click;
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            showItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            configItem.Enabled = e.FileState.Opened;
            printItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
            trainColorItem.Enabled = e.FileState.Opened && e.FileState.TrainsCreated;
        }

        private void TrainColorItem_Click(object sender, EventArgs e)
        {
            TrainColorForm tcf = new TrainColorForm();
            tcf.Init(info);
            if (tcf.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();            
        }

        private void PrintItem_Click(object sender, EventArgs e)
        {            
            doc = new PrintDocument();
            doc.PrintPage += Doc_PrintPage;
            PrintDialog dlg = new PrintDialog();
            dlg.AllowCurrentPage = false;
            dlg.AllowPrintToFile = false;
            dlg.UseEXDialog = true;            
            dlg.Document = doc;
            if (dlg.ShowDialog() == DialogResult.OK)
                doc.Print();
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            renderer = new Renderer(info.Timetable);
            int height = e.PageBounds.Height;
            last = renderer.GetTimeByHeight(true, last.HasValue ? last.Value : info.Timetable.GetMeta("StartTime", new TimeSpan(0, 0, 0), TimeSpan.Parse), height);
            renderer.Draw(e.Graphics);

            if (last.Value < info.Timetable.GetMeta("EndTime", new TimeSpan(1, 0, 0, 0), TimeSpan.Parse))
                e.HasMorePages = true;
            else
                last = null;
        }

        private void ConfigItem_Click(object sender, EventArgs e)
        {
            ConfigForm cnf = new ConfigForm();
            cnf.Init(info.Timetable);
            if (info.ShowDialog(cnf) == DialogResult.OK)
                info.SetUnsaved();
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
