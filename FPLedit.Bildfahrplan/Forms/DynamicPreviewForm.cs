using Eto.Forms;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EtoPoint = Eto.Drawing.Point;

namespace FPLedit.Bildfahrplan.Forms
{
    internal class DynamicPreviewForm : FForm
    {
        private readonly IInfo info;
        private Drawable panel;
        private Scrollable scrollable;
        private Renderer renderer;
        private RoutesDropDown routesDropDown;
        private EtoPoint scrollPosition = new EtoPoint(0, 0);

        public DynamicPreviewForm(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            ShowInTaskbar = true;
            Title = "Dynamische Bildfahrplan-Vorschau";
            Maximizable = false;
            Resizable = false;

            var mainForm = (FForm)info.RootForm;
            if (Screen != null && mainForm.Bounds.TopRight.X + 500 < Screen.Bounds.Width)
                Location = mainForm.Bounds.TopRight + new EtoPoint(10, 0);

            var stackLayout = new StackLayout();
            Content = stackLayout;

            var nStack = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 5,
                Padding = new Eto.Drawing.Padding(5),
            };
            stackLayout.Items.Add(nStack);

            routesDropDown = new RoutesDropDown();
            routesDropDown.SelectedRouteChanged += (s, e) => ResetRenderer();
            nStack.Items.Add(routesDropDown);

            var preferencesButton = new Button
            {
                Text = "Darstellung ändern",
            };
            preferencesButton.Click += (s, e) =>
            {
                info.StageUndoStep();
                ConfigForm cnf = new ConfigForm(info.Timetable, info.Settings);
                cnf.ShowModal(this);
                ResetRenderer();
                info.SetUnsaved();
            };
            nStack.Items.Add(preferencesButton);

            panel = new Drawable
            {
                Height = 0,
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

            // Initialisierung der Daten
            routesDropDown.Initialize(info);
        }

        private void ResetRenderer()
        {
            renderer = new Renderer(info.Timetable, routesDropDown.SelectedRoute);
            scrollPosition.Y = 0;
            panel.Height = renderer.GetHeight();
            panel.Invalidate();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            scrollPosition = scrollable.ScrollPosition;
            panel.Invalidate();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            if (renderer == null)
                return;

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

            scrollable.ScrollPosition = scrollPosition;
        }
    }
}
