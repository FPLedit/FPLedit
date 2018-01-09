using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.Editor
{
    public partial class EditStationForm : Form
    {
        Timetable _parent;
        int route;

        public EditStationForm()
        {
            InitializeComponent();
        }

        public EditStationForm(Timetable tt, int route) : this()
        {
            _parent = tt;
            this.route = route;
        }

        public EditStationForm(Station station, int route) : this()
        {
            Text = "Station bearbeiten";
            nameTextBox.Text = station.SName;
            positionTextBox.Text = station.Positions.GetPosition(route).Value.ToString("0.0");
            Station = station;
            this.route = route;
        }

        public Station Station { get; set; }

        private void closeButton_Click(object sender, EventArgs e)
        {
            string name = nameTextBox.Text;

            if (!positionValidator.Valid || !nameValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            var newPos = float.Parse(positionTextBox.Text);
            bool resetArdep = false;

            if (Station == null)
                Station = new Station(_parent);
            else
            {
                var tt = Station._parent;
                var rt = tt.GetRoute(route).GetOrderedStations();
                var idx = rt.IndexOf(Station);

                float? min = null, max = null;
                if (idx != rt.Count - 1)
                    max = rt[idx + 1].Positions.GetPosition(route);
                if (idx != 0)
                    min = rt[idx - 1].Positions.GetPosition(route);

                if ((min.HasValue && newPos < min) || (max.HasValue && newPos > max))
                {
                    var res = MessageBox.Show("ACHTUNG: Sie versuchen eine Station durch eine Positionsänderung auf der Strecke zwischen zwei andere Bahnhöfe zu verschieben. Dies wird, wenn Züge über diese Strecke angelegt wurden, zu unerwarteten Effekten führen. Trotzdem fortfahren?",
                        "FPLedit", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (res == DialogResult.No)
                    {
                        DialogResult = DialogResult.None; // Schließen abbrechen
                        return;
                    }
                    else
                        resetArdep = true;
                }
            }

            Dictionary<Train, ArrDep> ardeps = new Dictionary<Train, ArrDep>();
            if (resetArdep)
            {
                foreach (var tra in Station._parent.Trains)
                {
                    ardeps[tra] = tra.GetArrDep(Station);
                    tra.RemoveArrDep(Station);
                }
            }

            Station.Positions.SetPosition(route, newPos);
            Station.SName = name;

            if (resetArdep)
                foreach (var ardp in ardeps)
                    ardp.Key.AddArrDep(Station, ardp.Value, route);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
