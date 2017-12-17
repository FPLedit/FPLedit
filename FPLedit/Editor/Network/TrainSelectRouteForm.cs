using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Editor.Network
{
    public partial class TrainSelectRouteForm : Form
    {
        private IInfo info;
        private Station staStart, staEnd;

        public List<Station> TrainRoute { get; private set; }

        private TrainSelectRouteForm()
        {
            InitializeComponent();
        }

        public TrainSelectRouteForm(IInfo info) : this()
        {
            this.info = info;

            lineRenderer.StationMovingEnabled = false;
            lineRenderer.FixedStatusString = "Startstation auswählen";
            lineRenderer.SelectedRoute = -1;
            lineRenderer.SetTimetable(info.Timetable);
            lineRenderer.StationClicked += LineRenderer1_StationClicked;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void LineRenderer1_StationClicked(object sender, MouseEventArgs e)
        {
            if (staStart == null)
            {
                staStart = (Station)sender;
                lineRenderer.HighlightedStations.Add(staStart);
                lineRenderer.FixedStatusString = "Zielstation auswählen";
            }
            else if (staEnd == null)
            {
                staEnd = (Station)sender;

                var pathfinder = new Pathfinder(info.Timetable);
                TrainRoute = pathfinder.GetFromAToB(staStart, staEnd);
                lineRenderer.HighlightedStations.AddRange(TrainRoute.Skip(1));
                closeButton.Enabled = true;

                lineRenderer.FixedStatusString = "";
            }
        }
    }
}
