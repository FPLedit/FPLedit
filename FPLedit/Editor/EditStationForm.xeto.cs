using Eto.Forms;
using FPLedit.Editor.Rendering;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Editor
{
    internal class EditStationForm : FDialog<DialogResult>
    {
        private readonly int route;

#pragma warning disable CS0649
        private readonly TextBox nameTextBox, positionTextBox;
        private readonly StationRenderer stationRenderer;
#pragma warning restore CS0649
        private readonly ValidatorCollection validators;

        public Station Station { get; }

        public float Position { get; private set; }

        private readonly bool existingStation;
        private readonly Station trackStation;

        private int stationRendererHeight, stationRendererWidth;

        private EditStationForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            var positionValidator = new NumberValidator(positionTextBox, false, false, errorMessage: "Bitte eine Zahl als Position eingeben!");
            var nameValidator = new NotEmptyValidator(nameTextBox, errorMessage: "Bitte einen Bahnhofsnamen eingeben!");
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
        public EditStationForm(Timetable tt) : this(tt, Timetable.UNASSIGNED_ROUTE_ID)
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
            if (!validators.IsValid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben:" + Environment.NewLine + validators.Message);
                return;
            }
            
            if (existingStation && route == Timetable.UNASSIGNED_ROUTE_ID)
                throw new InvalidOperationException("Invalid state: No assigned route but Station marked as existing.");

            Station.SName = nameTextBox.Text;
            
            // Set position.
            var newPos = float.Parse(positionTextBox.Text);
            if (route == Timetable.UNASSIGNED_ROUTE_ID)
                Position = newPos;
            else if (!StationMoveHelper.TrySafeMove(Station, existingStation, newPos, route))
            {
                // We tried a safe move, but it is not possible.
                var res = MessageBox.Show("ACHTUNG: Sie versuchen Stationen einer Strecke durch eine Positionsänderung zwischen zwei andere Bahnhöfe zu verschieben und damit die Stationen umzusortieren.\n\nDies wird IN JEDEM FALL bei Zügen, die über diese Strecke verkehren, zu unerwarteten und Effekten und Fehlern führen und wird mit großer Wahrscheinlickeit zu Nebeneffekten an anderen Stellen führen und benötigt manuelle Nacharbeit. Diese Aktion wird nicht empfohlen.\n\n(Sie können stattdessen die alte Station löschen und eine neue anlegen). Trotzdem fortfahren und Stationen umzusortieren (auf eigenes Risiko)?",
                    "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);
                if (res == DialogResult.No)
                    return; // Do not proceed as user does not want to destroy his timetable.
                // User definitely wants to do evil things. Proceed.
                StationMoveHelper.PerformUnsafeMove(Station, existingStation, newPos, route); //TODO: Maybe catch and restore some sort of backup
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