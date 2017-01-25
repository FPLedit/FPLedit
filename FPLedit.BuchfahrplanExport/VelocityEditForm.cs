using FPLedit.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FPLedit.BuchfahrplanExport
{
    public partial class VelocityEditForm : Form
    {
        public Station Station { get; set; }

        public VelocityEditForm()
        {
            InitializeComponent();
        }

        public VelocityEditForm(Station station) : this()
        {
            Station = station;

            velocityTextBox.Text = station.GetAttribute("fpl-vmax", velocityTextBox.Text);

            positionTextBox.Text = station.Kilometre.ToString();
            positionTextBox.Enabled = false;
            nameTextBox.Text = station.SName;
            nameTextBox.Enabled = false;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            //TODO: Allow empty velocity
            if (!velocityValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            DialogResult = DialogResult.OK;

            Station.SetAttribute("fpl-vmax", velocityTextBox.Text);
            Close();
        }
    }
}
