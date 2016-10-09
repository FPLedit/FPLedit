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

namespace FPLedit.Standard
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

            if (!positionValidator.Valid || !nameValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            if (Station == null)
            {
                Station = new Station()
                {
                    Name = name,
                    Kilometre = float.Parse(positionTextBox.Text),
                };
            }
            else
            {
                Station.Name = name;
                Station.Kilometre = float.Parse(positionTextBox.Text);
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
