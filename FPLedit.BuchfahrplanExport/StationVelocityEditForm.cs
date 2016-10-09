using FPLedit.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FPLedit.BuchfahrplanExport
{
    public partial class StationVelocityEditForm : Form
    {
        public Station Station { get; set; }

        public StationVelocityEditForm()
        {
            InitializeComponent();
        }

        public StationVelocityEditForm(Station station) : this()
        {
            Station = station;

            velocityTextBox.Text = station.GetMeta("MaxVelocity", velocityTextBox.Text);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!velocityValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            DialogResult = DialogResult.OK;

            Station.Metadata["MaxVelocity"] = velocityTextBox.Text;
            Close();
        }
    }
}
