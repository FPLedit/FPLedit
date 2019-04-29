using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor
{
    internal class EditStationForm : FDialog<DialogResult>
    {
        private readonly int route;

#pragma warning disable CS0649
        private readonly TextBox nameTextBox, positionTextBox;
        private readonly StationRenderer stationRenderer;
#pragma warning restore CS0649
        private readonly NotEmptyValidator nameValidator;
        private readonly NumberValidator positionValidator;
        private readonly ValidatorCollection validators;

        public Station Station { get; set; }

        public float Position { get; private set; }

        private readonly bool existingStation;
        private readonly Station trackStation;

        private int stationRendererHeight, stationRendererWidth;

        private EditStationForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            positionValidator = new NumberValidator(positionTextBox, false, false);
            positionValidator.ErrorMessage = "Bitte eine Zahl als Position eingeben!";
            nameValidator = new NotEmptyValidator(nameTextBox);
            nameValidator.ErrorMessage = "Bitte einen Bahnhofsnamen eingeben!";
            validators = new ValidatorCollection(positionValidator, nameValidator);

            var shown = false;
            this.Shown += (s, e) =>
            {
                shown = true;
                stationRendererHeight = stationRenderer.Height;
                stationRendererWidth = stationRenderer.Width;
            };
            stationRenderer.SizeChanged += (s, e) =>
            {
                if (shown && stationRenderer.Height > stationRendererHeight)
                {
                    var diff = stationRenderer.Height - stationRendererHeight;
                    this.Height += diff;
                    stationRendererHeight = stationRenderer.Height;
                }
                if (shown && stationRenderer.Width > stationRendererWidth)
                {
                    var diff = stationRenderer.Width - stationRendererWidth;
                    this.Width += diff;
                    stationRendererWidth = stationRenderer.Width;
                }
            };
        }

        public EditStationForm(Timetable tt) : this(tt, -1) // Neue Station ohne Routenangabe
        {
        }

        public EditStationForm(Timetable tt, int route) : this() // Neue Station mit Routenangabe
        {
            Title = "Neue Station erstellen";
            this.route = route;
            existingStation = false;
            Station = new Station(tt);

            trackStation = Station.Clone<Station>();
            stationRenderer.Station = trackStation;
            stationRenderer.Route = route;

            if (tt.Version.Compare(TimetableVersion.JTG3_1) < 0)
                stationRenderer.Visible = false;
        }

        public EditStationForm(Station station, int route) : this() // Station editieren
        {
            Title = "Station bearbeiten";
            nameTextBox.Text = station.SName;
            positionTextBox.Text = station.Positions.GetPosition(route).Value.ToString("0.0");
            Station = station;
            this.route = route;

            existingStation = true;

            trackStation = Station.Clone<Station>();
            stationRenderer.Station = trackStation;
            stationRenderer.Route = route;

            if (station._parent.Version.Compare(TimetableVersion.JTG3_1) < 0)
                stationRenderer.Visible = false;
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

            if (existingStation)
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

            var ardeps = new Dictionary<Train, ArrDep>();
            if (resetArdep)
            {
                foreach (var tra in Station._parent.Trains)
                {
                    ardeps[tra] = tra.GetArrDep(Station).Clone<ArrDep>();
                    tra.RemoveArrDep(Station);
                }
            }

            if (route != -1)
                Station.Positions.SetPosition(route, newPos);
            else
                Position = newPos;
            Station.SName = name;

            if (resetArdep)
            {
                foreach (var ardp in ardeps)
                {
                    ardp.Key.AddArrDep(Station, route);
                    ardp.Key.GetArrDep(Station).ApplyCopy(ardp.Value);
                }
            }

            // Trackdaten aktualisieren
            if (!stationRenderer.CommitNameEdit())
                return;

            Station.DefaultTrackLeft.SetValue(route, trackStation.DefaultTrackLeft.GetValue(route));
            Station.DefaultTrackRight.SetValue(route, trackStation.DefaultTrackRight.GetValue(route));
            Station.Tracks.Clear();
            foreach (var track in trackStation.Tracks)
                Station.Tracks.Add(track);
            foreach (var rename in stationRenderer.TrackRenames)
                Station._parent.InternalRenameAllTrainTracksAtStation(Station, rename.Key, rename.Value);

            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
