using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    internal class EditStationForm : Dialog<DialogResult>
    {
        Timetable _parent;
        int route;

        #pragma warning disable CS0649
        private TextBox nameTextBox, positionTextBox;
        #pragma warning restore CS0649
        private NotEmptyValidator nameValidator;
        private NumberValidator positionValidator;
        private ValidatorCollection validators;

        public Station Station { get; set; }

        private void Init()
        {
            positionValidator = new NumberValidator(positionTextBox, false, false);
            positionValidator.ErrorMessage = "Bitte eine Zahl als Position eingeben!";
            nameValidator = new NotEmptyValidator(nameTextBox);
            nameValidator.ErrorMessage = "Bitte einen Bahnhofsnamen eingeben!";
            validators = new ValidatorCollection(positionValidator, nameValidator);
        }

        public EditStationForm(Timetable tt, int route)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            Init();

            Title = "Neue Station erstellen";
            _parent = tt;
            this.route = route;
        }

        public EditStationForm(Station station, int route)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            Init();

            Title = "Station bearbeiten";
            nameTextBox.Text = station.SName;
            positionTextBox.Text = station.Positions.GetPosition(route).Value.ToString("0.0");
            Station = station;
            this.route = route;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            string name = nameTextBox.Text;

            if (!validators.IsValid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben:" + Environment.NewLine + validators.Message);
                return;
            }

            var newPos = float.Parse(positionTextBox.Text);
            bool resetArdep = false;

            if (Station == null)
                Station = new Station(_parent);
            else
            {
                var tt = Station._parent;
                var rt = tt.GetRoute(route).GetOrderedStations();
                var idx = rt.IndexOf(Station);

                float? min = null, max = null;
                if (idx != rt.Count - 1)
                    max = rt[idx + 1].Positions.GetPosition(route);
                if (idx != 0)
                    min = rt[idx - 1].Positions.GetPosition(route);

                if ((min.HasValue && newPos < min) || (max.HasValue && newPos > max))
                {
                    var res = MessageBox.Show("ACHTUNG: Sie versuchen eine Station durch eine Positionsänderung auf der Strecke zwischen zwei andere Bahnhöfe zu verschieben. Dies wird, wenn Züge über diese Strecke angelegt wurden, zu unerwarteten Effekten führen. Trotzdem fortfahren?",
                        "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);
                    if (res == DialogResult.No)
                        return;
                    else
                        resetArdep = true;
                }
            }

            Dictionary<Train, ArrDep> ardeps = new Dictionary<Train, ArrDep>();
            if (resetArdep)
            {
                foreach (var tra in Station._parent.Trains)
                {
                    ardeps[tra] = tra.GetArrDep(Station);
                    tra.RemoveArrDep(Station);
                }
            }

            Station.Positions.SetPosition(route, newPos);
            Station.SName = name;

            if (resetArdep)
                foreach (var ardp in ardeps)
                    ardp.Key.AddArrDep(Station, ardp.Value, route);

            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
