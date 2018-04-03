using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Network
{
    internal class NewLineStationForm : Dialog<DialogResult>
    {
        Timetable _parent;

#pragma warning disable CS0649
        private TextBox nameTextBox, positionTextBox;
#pragma warning restore CS0649
        private NotEmptyValidator nameValidator;
        private NumberValidator positionValidator;

        public Station Station { get; set; }

        public float Position { get; private set; }

        public NewLineStationForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            positionValidator = new NumberValidator(positionTextBox, false, false);
            positionValidator.ErrorMessage = "Bitte eine Zahl als Position eingeben!";
            nameValidator = new NotEmptyValidator(nameTextBox);
            nameValidator.ErrorMessage = "Bitte einen Bahnhofsnamen eingeben!";
        }

        public NewLineStationForm(Timetable tt) : this()
        {
            _parent = tt;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            string name = nameTextBox.Text;

            if (!positionValidator.Valid || !nameValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            Station = new Station(_parent);
            Station.SName = name;
            Position = float.Parse(positionTextBox.Text);

            Result = DialogResult.Ok;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
