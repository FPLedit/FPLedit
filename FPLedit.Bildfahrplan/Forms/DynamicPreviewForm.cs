using Eto.Forms;
using Eto.Drawing;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan.Forms
{
    internal class DynamicPreviewForm : FForm
    {
        private readonly IInfo info;
        private Drawable panel, hpanel;
        private Scrollable scrollable;
        private Renderer renderer;
        private RoutesDropDown routesDropDown;
        private CheckBox splitCheckBox;
        private Point? scrollPosition = new Point(0, 0);
        private AsyncDoubleBufferedGraph adbg;

        public DynamicPreviewForm(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            ShowInTaskbar = true;
            Title = "Dynamische Bildfahrplan-Vorschau";
            Maximizable = false;
            Resizable = Platform.IsWpf;
            Height = 800;
            Width = 800;

            var mainForm = (FForm)info.RootForm;
            if (Screen != null && mainForm.Bounds.TopRight.X + 500 < Screen.Bounds.Width)
                Location = mainForm.Bounds.TopRight + new Point(10, 0);

            var stackLayout = new TableLayout();
            Content = stackLayout;

            var nStack = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 5,
                Padding = new Padding(5),
            };
            stackLayout.Rows.Add(nStack);

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
            splitCheckBox = new CheckBox
            {
                Text = "Stationen nicht mitscrollen",
                Checked = info.Settings.Get<bool>("bifpl.lock-stations"),
            };
            splitCheckBox.CheckedChanged += (s, e) =>
            {
                ResetRenderer();
                info.Settings.Set("bifpl.lock-stations", splitCheckBox.Checked.Value);
            };
            nStack.Items.Add(splitCheckBox);

            var dtc = new DateControl(info);
            dtc.ValueChanged += (s, e) => ResetRenderer();
            stackLayout.Rows.Add(dtc);

            hpanel = new Drawable();
            hpanel.Paint += Hpanel_Paint;
            stackLayout.Rows.Add(hpanel);

            panel = new Drawable();
            adbg = new AsyncDoubleBufferedGraph(panel);
            adbg.RenderingFinished = () =>
            {
                if (scrollPosition.HasValue)
                    scrollable.ScrollPosition = scrollPosition.Value;
                scrollPosition = null;
            };
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

            // Initialisierung der Daten
            routesDropDown.Initialize(info);
        }

        private void Hpanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (splitCheckBox.Checked.Value)
                    renderer.DrawHeader(e.Graphics, (scrollable.ClientSize.Width + panel.Width) / 2);
            }
            catch
            { }
        }

        private void ResetRenderer()
        {
            renderer = new Renderer(info.Timetable, routesDropDown.SelectedRoute);
            if (!scrollPosition.HasValue)
                scrollPosition = new Point(0, 0);
            panel.Height = renderer.GetHeight(!splitCheckBox.Checked.Value);
            hpanel.Height = splitCheckBox.Checked.Value ? renderer.GetHeight(default, default, true) : 0;

            adbg.Invalidate();
            hpanel.Invalidate();
        }

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            scrollPosition = scrollable.ScrollPosition;

            adbg.Invalidate();
            hpanel.Invalidate();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
            => adbg.Render(renderer, e.Graphics, !splitCheckBox.Checked.Value);
    }
}
