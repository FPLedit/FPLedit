using FPLedit.BuchfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FPLedit.BuchfahrplanExport
{
    public partial class VelocityEditForm : Form
    {
        public IStation Station { get; set; }

        private bool isPoint = false;

        public VelocityEditForm()
        {
            InitializeComponent();

            for (int i = 0; i < 4; i++)
                wellenComboBox.Items.Add(i.ToString());
            wellenComboBox.SelectedIndex = 0;
        }

        public VelocityEditForm(Timetable tt) : this()
        {
            Station = new BfplPoint(tt);
            isPoint = true;
        }

        public VelocityEditForm(IStation sta) : this()
        {
            Station = sta;

            velocityTextBox.Text = sta.GetAttribute("fpl-vmax", velocityTextBox.Text);
            positionTextBox.Text = sta.Kilometre.ToString();
            nameTextBox.Text = sta.SName;
            wellenComboBox.SelectedItem = sta.Wellenlinien.ToString();

            isPoint = true;
            if (sta.GetType() == typeof(Station))
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
                Station.Kilometre = float.Parse(positionTextBox.Text);
                Station.SName = nameTextBox.Text;
            }
            Close();
        }
    }
}
