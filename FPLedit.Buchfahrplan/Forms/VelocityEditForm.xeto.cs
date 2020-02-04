using Eto.Forms;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Buchfahrplan.Forms
{
    internal class VelocityEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly TextBox nameTextBox, positionTextBox, velocityTextBox;
        private readonly DropDown wellenComboBox;
#pragma warning restore CS0649
        private readonly NumberValidator velocityValidator, positionValidator;
        private readonly ValidatorCollection validators;

        public IStation Station { get; set; }

        private readonly bool isPoint = false;
        private readonly int route;

        public VelocityEditForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            velocityValidator = new NumberValidator(velocityTextBox, true, true, errorMessage: "Bitte eine Zahl als Geschwindikgeit eingeben!");
            positionValidator = new NumberValidator(positionTextBox, false, false, errorMessage: "Bitte eine Zahl als Position eingeben!");
            validators = new ValidatorCollection(velocityValidator, positionValidator);

            for (int i = 0; i < 4; i++)
                wellenComboBox.Items.Add(i.ToString());
            wellenComboBox.SelectedIndex = 0;
        }

        public VelocityEditForm(Timetable tt, int route) : this()
        {
            var point = new BfplPoint(tt);
            // Add route if it's a network timetbale
            if (tt.Type == TimetableType.Network)
                point._InternalAddRoute(route);
            Station = point;
            isPoint = true;
            this.route = route;
        }

        public VelocityEditForm(IStation sta, int route) : this()
        {
            Station = sta;
            this.route = route;

            velocityTextBox.Text = sta.Vmax.GetValue(route);
            positionTextBox.Text = sta.Positions.GetPosition(route).ToString();
            nameTextBox.Text = sta.SName;
            wellenComboBox.SelectedIndex = sta.Wellenlinien.GetValue(route); // Achtung: Keine Datenbindung, Index!

            isPoint = true;
            if (sta is Station)
            {
                positionTextBox.Enabled = false;
                nameTextBox.Enabled = false;
                isPoint = false;
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (!validators.IsValid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben:" + Environment.NewLine + validators.Message);
                return;
            }

            Station.Vmax.SetValue(route, velocityTextBox.Text);
            Station.Wellenlinien.SetValue(route, int.Parse(((IListItem)wellenComboBox.SelectedValue).Text));

            if (isPoint)
            {
                Station.Positions.SetPosition(route, float.Parse(positionTextBox.Text));
                Station.SName = nameTextBox.Text;
            }
            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
