using Eto.Forms;
using FPLedit.BildfahrplanExport.Forms;
using FPLedit.BildfahrplanExport.Model;
using FPLedit.BildfahrplanExport.Render;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.BildfahrplanExport
{
    [Plugin("Bildfahrplanmodul", "2.0.0", "2.0", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;
        private ButtonMenuItem showItem, configItem, trainColorItem, printItem;
        private Renderer renderer;
        private Dialog frm;
        private Drawable panel;
        private Scrollable scrollable;
        private DateControl dtc;

        private TimetableStyle attrs;

        public void Init(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            info.Register<IExport>(new BitmapExport());

            ButtonMenuItem item = ((MenuBar)info.Menu).CreateItem("Bildfahrplan");

            showItem = item.CreateItem("Anzeigen");
            showItem.Enabled = false;
            showItem.Click += ShowItem_Click;

            configItem = item.CreateItem("Darstellung ändern");
            configItem.Enabled = false;
            configItem.Click += ConfigItem_Click;

            printItem = item.CreateItem("Drucken");
            printItem.Enabled = false;
            printItem.Click += PrintItem_Click;

            trainColorItem = item.CreateItem("Zugdarstellung ändern");
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
            if (tcf.ShowModal(info.RootForm) == DialogResult.Ok)
                info.SetUnsaved();
        }

        private void ConfigItem_Click(object sender, EventArgs e)
        {
            info.StageUndoStep();
            ConfigForm cnf = new ConfigForm(info.Timetable, info.Settings);
            if (cnf.ShowModal(info.RootForm) == DialogResult.Ok)
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
            frm = new Dialog
            {
                ShowInTaskbar = false,
                Title = "Bildfahrplan",
                Maximizable = false,
            };

            var stackLayout = new StackLayout();
            frm.Content = stackLayout;

            dtc = new DateControl(info);
            dtc.ValueChanged += Dtc_ValueChanged;
            stackLayout.Items.Add(dtc);

            panel = new Drawable
            {
                Height = renderer.GetHeight(),
                Width = 800
            };
            panel.Paint += Panel_Paint;

            scrollable = new Scrollable
            {
                ExpandContentWidth = false,
                ExpandContentHeight = false,
                Height = 800,
                Content = panel,
            };
            stackLayout.Items.Add(scrollable);

            frm.ShowModal(info.RootForm);
        }

        private void Dtc_ValueChanged(object sender, EventArgs e)
        {
            renderer = new Renderer(info.Timetable, Timetable.LINEAR_ROUTE_ID);
            panel.Height = renderer.GetHeight();
            panel.Invalidate();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            renderer.width = panel.Width;

            using (var bmp = new Bitmap(panel.Width, renderer.GetHeight()))
            using (var g = Graphics.FromImage(bmp))
            using (var ms = new MemoryStream())
            {
                renderer.Draw(g);
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                using (var eto = new Eto.Drawing.Bitmap(ms.ToArray()))
                    e.Graphics.DrawImage(eto, new Eto.Drawing.PointF(0, 0));
            }
        }
        #endregion
    }
}
