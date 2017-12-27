using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FPLedit.Buchfahrplan
{
    public partial class VelocityEditForm : Form
    {
        public IStation Station { get; set; }

        private bool isPoint = false;
        private int route;

        public VelocityEditForm()
        {
            InitializeComponent();

            for (int i = 0; i < 4; i++)
                wellenComboBox.Items.Add(i.ToString());
            wellenComboBox.SelectedIndex = 0;
        }

        public VelocityEditForm(Timetable tt, int route) : this()
        {
            Station = new BfplPoint(tt);
            // Add route if it's a network timetbale
            if (tt.Type == TimetableType.Network)
                Station.Routes = new int[] { route };
            isPoint = true;
            this.route = route;
        }

        public VelocityEditForm(IStation sta, int route) : this()
        {
            Station = sta;
            this.route = route;

            velocityTextBox.Text = sta.Vmax;
            positionTextBox.Text = sta.Positions.GetPosition(route).ToString();
            nameTextBox.Text = sta.SName;
            wellenComboBox.SelectedItem = sta.Wellenlinien.ToString();

            isPoint = true;
            if (sta is Station)
            {
                positionTextBox.Enabled = false;
                nameTextBox.Enabled = false;
                isPoint = false;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!velocityValidator.Valid || !positionValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            DialogResult = DialogResult.OK;

            Station.SetAttribute("fpl-vmax", velocityTextBox.Text);
            Station.Wellenlinien = int.Parse((string)wellenComboBox.SelectedItem);

            if (isPoint)
            {
                Station.Positions.SetPosition(route, float.Parse(positionTextBox.Text));
                Station.SName = nameTextBox.Text;
            }
            Close();
        }
    }
}
