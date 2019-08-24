using Eto.Forms;
using FPLedit.Editor.Rendering;
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

            positionValidator = new NumberValidator(positionTextBox, false, false, errorMessage: "Bitte eine Zahl als Position eingeben!");
            nameValidator = new NotEmptyValidator(nameTextBox, errorMessage: "Bitte einen Bahnhofsnamen eingeben!");
            validators = new ValidatorCollection(positionValidator, nameValidator);

            this.Shown += (s, e) =>
            {
                stationRendererHeight = stationRenderer.Height;
                stationRendererWidth = stationRenderer.Width;
            };
            stationRenderer.SizeChanged += (s, e) =>
            {
                if (WindowShown && stationRenderer.Height > stationRendererHeight)
                {
                    var diff = stationRenderer.Height - stationRendererHeight;
                    this.Height += diff;
                    stationRendererHeight = stationRenderer.Height;
                }
                if (WindowShown && stationRenderer.Width > stationRendererWidth)
                {
                    var diff = stationRenderer.Width - stationRendererWidth;
                    this.Width += diff;
                    stationRendererWidth = stationRenderer.Width;
                }
            };
        }

        /// <summary>
        /// Form to create a new station without a given route id.
        /// </summary>
        public EditStationForm(Timetable tt) : this(tt, -1)
        {
        }

        /// <summary>
        /// Form to create a new station with a given route id.
        /// </summary>
        public EditStationForm(Timetable tt, int route) : this()
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

        /// <summary>
        /// Form to edit a station (with given route id);
        /// </summary>
        public EditStationForm(Station station, int route) : this()
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

        private void CloseButton_Click(object sender, EventArgs e)
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
                    var path = tra.GetPath();
                    var idx = path.IndexOf(Station);
                    if (idx == -1) // Station not in path; not applicable to train.
                        continue;

                    // Filter out trains that don't use the current editing sessions route id.
                    var prev = path.ElementAtOrDefault(idx - 1);
                    var next = path.ElementAtOrDefault(idx + 1);

                    var empty = Array.Empty<int>();
                    var routes = empty.Concat(prev?.Routes ?? empty).Concat(next?.Routes ?? empty);
                    if (!routes.Contains(route))
                        continue;

                    var arrDep = tra.GetArrDep(Station);
                    ardeps[tra] = arrDep.Clone<ArrDep>();
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
                var trainsDataLoss = new List<string>();
                foreach (var ardp in ardeps)
                {
                    var a = ardp.Key.AddArrDep(Station, route);
                    a?.ApplyCopy(ardp.Value);
                    if (a == null)
                        trainsDataLoss.Add(ardp.Key.TName);
                }
                if (trainsDataLoss.Any()) //TODO: Is this safeguard message really needed (or should we throw?)
                    MessageBox.Show($"Unerwarteter Datenverlust beim Umschreiben der Züge: {string.Join(",", trainsDataLoss)}", "FPLedit", MessageBoxType.Error);
            }

            // Update track data.
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

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
