using Eto.Forms;
using Eto.Drawing;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;

namespace FPLedit.Bildfahrplan.Forms
{
    internal class PreviewForm : FForm
    {
#pragma warning disable CS0649
        private readonly Drawable panel, hpanel;
        private readonly Scrollable scrollable;
        private readonly DateControl dtc;
        private readonly RoutesDropDown routesDropDown;
        private readonly CheckBox splitCheckBox;
#pragma warning restore CS0649

        private readonly IInfo info;
        private readonly AsyncDoubleBufferedGraph adbg;
        private Point? scrollPosition = new Point(0, 0);
        private Renderer renderer;

        public PreviewForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            Resizable = Platform.IsWpf;

            var mainForm = (FForm)info.RootForm;
            if (Screen != null && mainForm.Bounds.TopRight.X + 500 < Screen.Bounds.Width)
                Location = mainForm.Bounds.TopRight + new Point(10, 0);

            routesDropDown.SelectedRouteChanged += (s, e) => ResetRenderer();
            dtc.ValueChanged += (s, e) => ResetRenderer();

            splitCheckBox.Checked = info.Settings.Get<bool>("bifpl.lock-stations");
            splitCheckBox.CheckedChanged += (s, e) =>
            {
                ResetRenderer();
                info.Settings.Set("bifpl.lock-stations", splitCheckBox.Checked.Value);
            };

            adbg = new AsyncDoubleBufferedGraph(panel)
            {
                RenderingFinished = () =>
                {
                    if (scrollPosition.HasValue)
                        scrollable.ScrollPosition = scrollPosition.Value;
                    scrollPosition = null;
                }
            };

            // Initialisierung der Daten
            dtc.Initialize(info);
            routesDropDown.Initialize(info);
        }

        private void PreferencesButton_Click(object sender, EventArgs e)
        {
            info.StageUndoStep();
            using (var cnf = new ConfigForm(info.Timetable, info.Settings))
                cnf.ShowModal(this);
            ResetRenderer();
            info.SetUnsaved();
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

        protected override void Dispose(bool disposing)
        {
            adbg?.Dispose();
            base.Dispose(disposing);
        }
    }
}
