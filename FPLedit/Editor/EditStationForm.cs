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

            if (Station == null)
                Station = new Station(_parent);
            Station.SName = name;
            Station.Positions.SetPosition(route, float.Parse(positionTextBox.Text));

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
