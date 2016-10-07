using Buchfahrplan.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Buchfahrplan.BildfahrplanExport
{
    public partial class StationVelocityEditForm : Form
    {
        public Station Station { get; set; }

        public StationVelocityEditForm()
        {
            InitializeComponent();
        }

        public void Initialize(Station station)
        {
            Station = station;

            velocityTextBox.Text = station.GetMeta("MaxVelocity", velocityTextBox.Text);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Station.Metadata["MaxVelocity"] = velocityTextBox.Text;
            Close();
        }
    }
}
