using FPLedit.BildfahrplanExport.Model;
using FPLedit.BildfahrplanExport.Render;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.BildfahrplanExport
{
    [Plugin("Bildfahrplanmodul", "2.0.0", "2.0", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ToolStripItem showItem, configItem, trainColorItem, printItem;
        private Renderer renderer;
        private Form frm;
        private Panel panel;
        private DateControl dtc;

        private TimetableStyle attrs;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.Register<IExport>(new BitmapExport());

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

            attrs = new TimetableStyle(info.Timetable);

            if (e.FileState.Opened && info.Timetable.Type == TimetableType.Network)
            {
                showItem.Text = "Anzeigen (aktuelle Route)";
                printItem.Text = "Drucken (aktuelle Route)";
            }
            else
            {
                showItem.Text = "Anzeigen";
                printItem.Text = "Drucken";
            }
        }

        private void TrainColorItem_Click(object sender, EventArgs e)
        {
            info.StageUndoStep();
            TrainColorForm tcf = new TrainColorForm(info);
            if (tcf.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        private void ConfigItem_Click(object sender, EventArgs e)
        {
            info.StageUndoStep();
            ConfigForm cnf = new ConfigForm(info.Timetable);
            if (cnf.ShowDialog() == DialogResult.OK)
                info.SetUnsaved();
        }

        #region Print
        private void PrintItem_Click(object sender, EventArgs e)
        {
            var route = (info.Timetable.Type == TimetableType.Network) ? info.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
            new PrintRenderer(info.Timetable, route).InitPrint();
        }
        #endregion

        #region Preview
        private void ShowItem_Click(object sender, EventArgs e)
        {
            var route = (info.Timetable.Type == TimetableType.Network) ? info.FileState.SelectedRoute : Timetable.LINEAR_ROUTE_ID;
            renderer = new Renderer(info.Timetable, route);
            frm = new Form
            {
                Height = 1000,
                Width = 1000,
                ShowIcon = false,
                ShowInTaskbar = false,
                Text = "Bildfahrplan",
                MaximizeBox = false,
                AutoScroll = true,
                AutoSize = false
            };

            dtc = new DateControl(info.Timetable);
            dtc.Dock = DockStyle.Top;
            dtc.ValueChanged += Dtc_ValueChanged;
            frm.Controls.Add(dtc);

            panel = new Panel();
            panel.Width = frm.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            panel.Height = renderer.GetHeight();
            panel.Top = dtc.Height;
            frm.Controls.Add(panel);
            panel.Paint += Panel_Paint;

            frm.ShowDialog();
        }

        private void Dtc_ValueChanged(object sender, EventArgs e)
        {
            renderer = new Renderer(info.Timetable, Timetable.LINEAR_ROUTE_ID);
            frm.Controls.Remove(panel);
            panel = new Panel();
            panel.Width = frm.ClientSize.Width - SystemInformation.VerticalScrollBarWidth;
            panel.Height = renderer.GetHeight();
            panel.Top = dtc.Height;
            frm.Controls.Add(panel);
            panel.Paint += Panel_Paint;
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            frm.Width = 1000;
            frm.Height = 1000;
            renderer.Draw(e.Graphics);
        }
        #endregion
    }
}
