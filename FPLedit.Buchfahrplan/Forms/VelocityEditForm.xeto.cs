using Eto.Forms;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Buchfahrplan.Forms
{
    internal class VelocityEditForm : Dialog<DialogResult>
    {
#pragma warning disable CS0649
        private TextBox nameTextBox, positionTextBox, velocityTextBox;
        private DropDown wellenComboBox;
#pragma warning restore CS0649
        private NumberValidator velocityValidator, positionValidator;

        public IStation Station { get; set; }

        private bool isPoint = false;
        private int route;

        public VelocityEditForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            velocityValidator = new NumberValidator(velocityTextBox, true, true);
            velocityValidator.ErrorMessage = "Bitte eine Zahl als Geschwindikgeit eingeben!";
            positionValidator = new NumberValidator(positionTextBox, false, false);
            positionValidator.ErrorMessage = "Bitte eine Zahl als Position eingeben!";

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
            wellenComboBox.SelectedIndex = sta.Wellenlinien; // Achtung: Keine Datenbindung, Index!

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

            Result = DialogResult.Ok;

            Station.SetAttribute("fpl-vmax", velocityTextBox.Text);
            Station.Wellenlinien = int.Parse(((IListItem)wellenComboBox.SelectedValue).Text);

            if (isPoint)
            {
                Station.Positions.SetPosition(route, float.Parse(positionTextBox.Text));
                Station.SName = nameTextBox.Text;
            }
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            Close();
        }
    }
}
