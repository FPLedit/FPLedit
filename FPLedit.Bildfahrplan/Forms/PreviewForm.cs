using Eto.Drawing;
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
        private AsyncDoubleBufferedGraph adbg;

        public PreviewForm(IInfo info, int route)
        {
            this.info = info;

            renderer = new Renderer(info.Timetable, route);
            ShowInTaskbar = false;
            Title = "Bildfahrplan";
            Maximizable = false;
            Resizable = Platform.IsWpf;
            Width = 800;
            Height = 800;

            var stackLayout = new TableLayout();
            Content = stackLayout;

            dtc = new DateControl(info);
            dtc.ValueChanged += Dtc_ValueChanged;
            stackLayout.Rows.Add(dtc);

            panel = new Drawable
            {
                Height = renderer.GetHeight(true),
            };
            adbg = new AsyncDoubleBufferedGraph(panel);
            panel.Paint += Panel_Paint;

            scrollable = new Scrollable
            {
                ExpandContentWidth = true,
                ExpandContentHeight = false,
                Content = panel,
            };
            var scrollableRow = new TableRow(scrollable)
            {
                ScaleHeight = true,
            };
            stackLayout.Rows.Add(scrollableRow);
        }

        private void Dtc_ValueChanged(object sender, EventArgs e)
        {
            renderer = new Renderer(info.Timetable, Timetable.LINEAR_ROUTE_ID);
            panel.Height = renderer.GetHeight(true);
            adbg.Invalidate();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            adbg.Render(renderer, e.Graphics, true);
        }
    }
}
