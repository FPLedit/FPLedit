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

        public void Initialize(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += (s, e) =>
            {
                lineRenderer.SetLine(info.Timetable);
                newButton.Enabled = e.FileState.Opened;
            };

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
                        r.Add(0);
                        sta.Routes = r.ToArray();
                        lineRenderer.SetLine(info.Timetable);
                    }
                    info.Timetable.AddStation(sta);
                    info.SetUnsaved();
                }
            };
        }
    }
}
