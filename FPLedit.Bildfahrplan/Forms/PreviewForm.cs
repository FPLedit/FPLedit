using Eto.Forms;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Bildfahrplan.Forms
{
    internal class PreviewForm : FDialog
    {
        private readonly IInfo info;
        private Drawable panel;
        private Scrollable scrollable;
        private DateControl dtc;
        private Renderer renderer;

        public PreviewForm(IInfo info, int route)
        {
            this.info = info;

            renderer = new Renderer(info.Timetable, route);
            ShowInTaskbar = false;
            Title = "Bildfahrplan";
            Maximizable = false;

            var stackLayout = new StackLayout();
            Content = stackLayout;

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
            renderer.Draw(e.Graphics);
        }
    }
}
