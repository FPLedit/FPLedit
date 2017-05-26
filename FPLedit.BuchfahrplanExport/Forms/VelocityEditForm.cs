using FPLedit.BuchfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FPLedit.BuchfahrplanExport
{
    public partial class VelocityEditForm : Form
    {
        public Station Station { get; set; }

        public BfplPoint Point { get; set; }

        private bool isPoint = false;

        public VelocityEditForm()
        {
            InitializeComponent();
        }

        public VelocityEditForm(Timetable tt) : this()
        {
            Point = new BfplPoint(tt);
            isPoint = true;
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

        public VelocityEditForm(BfplPoint point) : this()
        {
            Point = point;
            isPoint = true;

            velocityTextBox.Text = point.GetAttribute("fpl-vmax", velocityTextBox.Text);

            positionTextBox.Text = point.Kilometre.ToString();
            nameTextBox.Text = point.PName;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!velocityValidator.Valid || !positionValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            DialogResult = DialogResult.OK;

            if (isPoint)
            {
                Point.SetAttribute("fpl-vmax", velocityTextBox.Text);
                Point.Kilometre = float.Parse(positionTextBox.Text);
                Point.PName = nameTextBox.Text;
            }
            else
            {
                Station.SetAttribute("fpl-vmax", velocityTextBox.Text);
            }
            Close();
        }
    }
}
