using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FPLedit.Shared;

namespace FPLedit.Editor.Network
{
    public partial class LineEditingControl : UserControl
    {
        private IInfo info;
        private int selectedRoute = 0; //TODO: Use actual index

        public LineEditingControl()
        {
            InitializeComponent();
        }

        private IndexedItem[] GetRouteNames(Timetable tt)
        {
            var routes = new List<IndexedItem>();

            if (tt.Type == TimetableType.Network)
            {
                var rt = tt.GetRoutes();
                foreach (var route in rt)
                    routes.Add(new IndexedItem(route.GetRouteName(), route.Index));
            }
            else
                routes.Add(new IndexedItem("<Standard>", 0));
            return routes.ToArray();
        }

        private void ReloadRouteNames()
        {
            var routes = GetRouteNames(info.Timetable);
            routesComboBox.Items.Clear();
            routesComboBox.Items.AddRange(routes);
            routesComboBox.SelectedIndex = 0;
        }

        public void Initialize(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += (s, e) =>
            {
                lineRenderer.SetTimetable(info.Timetable);
                newButton.Enabled = routesComboBox.Enabled = newLineButton.Enabled = e.FileState.Opened;

                if (e.FileState.LineCreated)
                    ReloadRouteNames();

                newLineButton.Visible = routesComboBox.Visible = info.Timetable.Type == TimetableType.Network;
            };

            routesComboBox.SelectedIndexChanged += (s, e) =>
            {
                selectedRoute = ((IndexedItem)routesComboBox.SelectedItem).Index;
                lineRenderer.SelectedRoute = selectedRoute;
            };

            lineRenderer.StationDoubleClicked += (s, e) =>
            {
                info.StageUndoStep();
                Editor.EditStationForm nsf = new Editor.EditStationForm((Station)s, selectedRoute);
                if (nsf.ShowDialog() == DialogResult.OK)
                {
                    lineRenderer.SetTimetable(info.Timetable);
                    info.SetUnsaved();
                }
            };
            lineRenderer.StationClicked += (s, e) =>
            {
                if (e.Button != MouseButtons.Right)
                    return;

                var strip = new ContextMenuStrip();
                var itm = strip.Items.Add("Löschen");
                strip.Show(MousePosition);
                itm.Click += (se, ar) => {
                    info.StageUndoStep();
                    info.Timetable.RemoveStation((Station)s);
                    lineRenderer.SetTimetable(info.Timetable);
                    info.SetUnsaved();
                };
            };
            lineRenderer.NewRouteAdded += (s, args) => ReloadRouteNames();
            newButton.Click += (s, e) =>
            {
                info.StageUndoStep();
                var nsf = new Editor.EditStationForm(info.Timetable, selectedRoute);
                if (nsf.ShowDialog() == DialogResult.OK)
                {
                    Station sta = nsf.Station;
                    if (info.Timetable.Type == TimetableType.Network)
                    {
                        var handler = new StaPosHandler();
                        handler.SetMiddlePos(selectedRoute, sta, info.Timetable);
                        var r = sta.Routes.ToList();
                        r.Add(lineRenderer.SelectedRoute);
                        sta.Routes = r.ToArray();
                    }
                    info.Timetable.AddStation(sta);
                    info.SetUnsaved();
                    lineRenderer.SetTimetable(info.Timetable);
                }
            };
            newLineButton.Click += (s, e) =>
            {
                info.StageUndoStep();
                var nlf = new NewLineStationForm(info.Timetable);
                if (nlf.ShowDialog() == DialogResult.OK)
                {
                    lineRenderer.StartAddStation(nlf.Station, nlf.Position);
                    info.SetUnsaved();
                }
            };
        }
    }

    internal class IndexedItem
    {
        public string Text { get; set; }
        public int Index { get; set; }

        public IndexedItem(string text, int idx)
        {
            Text = text;
            Index = idx;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
