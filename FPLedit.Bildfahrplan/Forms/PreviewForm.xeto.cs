using Eto.Forms;
using Eto.Drawing;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using FPLedit.Bildfahrplan.Model;

namespace FPLedit.Bildfahrplan.Forms
{
    internal sealed class PreviewForm : FForm
    {
#pragma warning disable CS0649
        private readonly Drawable panel, hpanel;
        private readonly Scrollable scrollable;
        private readonly DaysControlWide dtc;
        private readonly RoutesDropDown routesDropDown;
        private readonly CheckBox splitCheckBox;
#pragma warning restore CS0649

        private readonly IPluginInterface pluginInterface;
        private readonly AsyncDoubleBufferedGraph adbg;
        private Point? scrollPosition = new Point(0, 0);
        private Renderer renderer;
        
        public PreviewForm(IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.pluginInterface = pluginInterface;
            pluginInterface.FileStateChanged += PluginInterface_FileStateChanged;

            var mainForm = (FForm)pluginInterface.RootForm;
            if (Screen != null && mainForm.Bounds.TopRight.X + 500 < Screen.Bounds.Width)
                Location = mainForm.Bounds.TopRight + new Point(10, 0);

            this.SizeChanged += (s, e) => panel.Invalidate();

            routesDropDown.SelectedRouteChanged += (s, e) => ResetRenderer();
            dtc.SelectedDaysChanged += (s, e) =>
            {
                var attrs = new TimetableStyle(pluginInterface.Timetable);
                attrs.RenderDays = dtc.SelectedDays;
                ResetRenderer();
            };

            splitCheckBox.Checked = pluginInterface.Settings.Get<bool>("bifpl.lock-stations");
            splitCheckBox.CheckedChanged += (s, e) =>
            {
                ResetRenderer();
                pluginInterface.Settings.Set("bifpl.lock-stations", splitCheckBox.Checked.Value);
            };

            adbg = new AsyncDoubleBufferedGraph(panel, pluginInterface)
            {
                RenderingFinished = () =>
                {
                    if (scrollPosition.HasValue)
                        scrollable.ScrollPosition = scrollPosition.Value;
                    scrollPosition = null;
                }
            };

            // Initialisierung der Daten
            dtc.SelectedDays = new TimetableStyle(pluginInterface.Timetable).RenderDays;
            routesDropDown.Initialize(pluginInterface);
        }

        private void PreferencesButton_Click(object sender, EventArgs e)
        {
            pluginInterface.StageUndoStep();
            using (var cnf = new ConfigForm(pluginInterface.Timetable, pluginInterface))
                cnf.ShowModal(this);
            ResetRenderer();
            pluginInterface.SetUnsaved();
        }

        private void Hpanel_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (splitCheckBox.Checked.Value)
                {
                    using (var ib = new ImageBridge(hpanel.Width, hpanel.Height))
                    {
                        renderer.DrawHeader(ib.Graphics, (scrollable.ClientSize.Width + panel.Width) / 2);
                        ib.CoptyToEto(e.Graphics);
                    }
                }
            }
            catch
            { }
        }

        private void ResetRenderer()
        {
            renderer = new Renderer(pluginInterface.Timetable, routesDropDown.SelectedRoute);
            if (!scrollPosition.HasValue)
                scrollPosition = new Point(0, 0);
            panel.Height = renderer.GetHeightExternal(!splitCheckBox.Checked.Value);
            hpanel.Height = splitCheckBox.Checked.Value ? renderer.GetHeightExternal(default, default, true) : 0;

            adbg.Invalidate();
            hpanel.Invalidate();
        }

        private void PluginInterface_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            if (!Visible || scrollable == null || adbg == null || hpanel == null || IsDisposed)
                return;
            
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
