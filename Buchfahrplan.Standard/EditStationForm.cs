using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Buchfahrplan.Standard
{
    public partial class EditStationForm : Form
    {
        public EditStationForm()
        {
            InitializeComponent();
        }

        public EditStationForm(Station station) : this()
        {
            Text = "Station bearbeiten";
            nameTextBox.Text = station.Name;
            positionTextBox.Text = station.Kilometre.ToString();
            Station = station;
        }

        public Station Station { get; set; }

        private void closeButton_Click(object sender, EventArgs e)
        {
            string name = nameTextBox.Text;
            float pos;
            if (!float.TryParse(positionTextBox.Text, out pos))
                MessageBox.Show("Position (km): FEHLER Die eingegebene Zeichenfolge ist kein valider Wert für eine Kommazahl!");

            if (Station == null)
            {
                Station = new Station()
                {
                    Name = name,
                    Kilometre = pos,
                };
            }
            else
            {
                Station.Name = name;
                Station.Kilometre = pos;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
