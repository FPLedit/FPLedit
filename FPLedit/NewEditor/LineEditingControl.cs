using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using FPLedit.Shared;

namespace FPLedit.NewEditor
{
    public partial class LineEditingControl : UserControl
    {
        private IInfo info;

        public LineEditingControl()
        {
            InitializeComponent();
        }

        private IndexedItem[] GetLineNames(Timetable tt)
        {
            var routes = new List<IndexedItem>();

            if (tt.Type == TimetableType.Network)
            {
                var routesIndices = tt.Stations.SelectMany(s => s.Routes).Distinct();
                foreach (var ri in routesIndices)
                {
                    var rt = tt.Stations.Where(s => s.Routes.Contains(ri)).OrderBy(s => s.Kilometre).ToList();
                    var text = (rt.FirstOrDefault()?.SName ?? "") + " - " + (rt.LastOrDefault()?.SName ?? "");
                    routes.Add(new IndexedItem(text, ri));
                }
            }
            else
                routes.Add(new IndexedItem("<Standard>", 0));
            return routes.ToArray();
        }

        public void Initialize(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += (s, e) =>
            {
                lineRenderer.SetLine(info.Timetable);
                newButton.Enabled = e.FileState.Opened;

                if (e.FileState.LineCreated)
                {
                    var routes = GetLineNames(info.Timetable);
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(routes);
                    comboBox1.SelectedIndex = 0;
                }
            };

            comboBox1.SelectedIndexChanged += (s, e)
                => lineRenderer.SelectedRoute = ((IndexedItem)comboBox1.SelectedItem).Index;

            lineRenderer.StationDoubleClicked += (s, e) =>
            {
                info.StageUndoStep();
                Editor.EditStationForm nsf = new Editor.EditStationForm((Station)s);
                if (nsf.ShowDialog() == DialogResult.OK)
                {
                    lineRenderer.SetLine(info.Timetable);
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
                    lineRenderer.SetLine(info.Timetable);
                    info.SetUnsaved();
                };
            };
            newButton.Click += (s, e) =>
            {
                info.StageUndoStep();
                Editor.EditStationForm nsf = new Editor.EditStationForm(info.Timetable);
                if (nsf.ShowDialog() == DialogResult.OK)
                {
                    Station sta = nsf.Station;
                    if (info.Timetable.Type == TimetableType.Network)
                    {
                        var handler = new StaPosHandler();
                        handler.SetMiddlePos(0, sta, info.Timetable);
                        var r = sta.Routes.ToList();
                        r.Add(lineRenderer.SelectedRoute);
                        sta.Routes = r.ToArray();
                        lineRenderer.SetLine(info.Timetable);
                    }
                    info.Timetable.AddStation(sta);
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
