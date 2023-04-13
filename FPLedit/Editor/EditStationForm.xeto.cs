using Eto.Forms;
using FPLedit.Editor.Rendering;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Editor
{
    internal sealed class EditStationForm : FDialog<DialogResult>
    {
        private readonly IPluginInterface pluginInterface;
        private readonly int route;

#pragma warning disable CS0649,CA2213
        private readonly TextBox nameTextBox = default!, positionTextBox = default!, codeTextBox = default!;
        private readonly ComboBox typeComboBox = default!;
        private readonly CheckBox requestCheckBox = default!;
        private readonly StationRenderer stationRenderer = default!;
#pragma warning restore CS0649,CA2213
        private readonly ValidatorCollection validators;

        public Station Station { get; }

        public float Position { get; private set; }

        private readonly bool existingStation;

        private int stationRendererHeight, stationRendererWidth;

        private EditStationForm(Timetable tt, IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            Eto.Serialization.Xaml.XamlReader.Load(this);

            var positionValidator = new NumberValidator(positionTextBox, false, false, errorMessage: T._("Bitte eine Zahl als Position eingeben!"));
            var nameValidator = new NotEmptyValidator(nameTextBox, errorMessage: T._("Bitte einen Bahnhofsnamen eingeben!"));
            validators = new ValidatorCollection(positionValidator, nameValidator);

            this.Shown += (_, _) =>
            {
                stationRendererHeight = stationRenderer.Height;
                stationRendererWidth = stationRenderer.Width;
            };
            stationRenderer.SizeChanged += (_, _) =>
            {
                var size = ClientSize;
                var changed = false;
                
                if (WindowShown && stationRenderer.Height > stationRendererHeight)
                {
                    var diff = stationRenderer.Height - stationRendererHeight;
                    size.Height += diff;
                    stationRendererHeight = stationRenderer.Height;
                    changed = true;
                }

                if (WindowShown && stationRenderer.Width > stationRendererWidth)
                {
                    var diff = stationRenderer.Width - stationRendererWidth;
                    size.Width += diff;
                    stationRendererWidth = stationRenderer.Width;
                    changed = true;
                }

                if (changed)
                    ClientSize = size;
            };
            
            typeComboBox.DataStore = tt.Stations.Select(s => s.StationType).Distinct().Where(s => s != "").OrderBy(s => s).Select(s => new ListItem { Text = s });
        }

        /// <summary>
        /// Form to create a new station with a given route index.
        /// </summary>
        /// <remarks>Use <see cref="Timetable.UNASSIGNED_ROUTE_ID"/> as route to not specify a specific route id.</remarks>
        public EditStationForm(IPluginInterface pluginInterface, Timetable tt, int route) : this(tt, pluginInterface)
        {
            Title = T._("Neue Station erstellen");
            this.route = route;
            existingStation = false;
            Station = new Station(tt);

            stationRenderer.InitializeWithStation(route, Station);
        }

        /// <summary>
        /// Form to edit a station (with given route index);
        /// </summary>
        public EditStationForm(IPluginInterface pluginInterface, Station station, int route) : this(station.ParentTimetable, pluginInterface)
        {
            Title = T._("Station bearbeiten");
            nameTextBox.Text = station.SName;
            positionTextBox.Text = station.Positions.GetPosition(route)!.Value.ToString("0.0");
            
            Station = station;
            this.route = route;

            existingStation = true;

            stationRenderer.InitializeWithStation(route, Station);
            
            codeTextBox.Text = station.StationCode;
            typeComboBox.Text = station.StationType;
            requestCheckBox.Checked = Station.RequestStop;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (!validators.IsValid)
            {
                MessageBox.Show(T._("Bitte erst alle Fehler beheben:\n{0}", validators.Message));
                return;
            }
            
            if (existingStation && route == Timetable.UNASSIGNED_ROUTE_ID)
                throw new InvalidOperationException("Invalid state: No assigned route but Station marked as existing.");

            Station.SName = nameTextBox.Text;
            
            Station.StationCode = codeTextBox.Text;
            Station.StationType = typeComboBox.Text;
            Station.RequestStop = requestCheckBox.Checked ?? false; 

            // Set position.
            var newPos = float.Parse(positionTextBox.Text);
            if (route == Timetable.UNASSIGNED_ROUTE_ID) // We have a new station on a new route
                Position = newPos;
            else if (!StationMoveHelper.TrySafeMove(Station, existingStation, newPos, route))
            {
                // We tried a safe move, but it is not possible.
                var res = MessageBox.Show(T._("ACHTUNG: Sie versuchen Stationen einer Strecke durch eine Positionsänderung zwischen zwei andere Bahnhöfe zu verschieben und damit die Stationen umzusortieren.\n\nDies wird IN JEDEM FALL bei Zügen, die über diese Strecke verkehren, zu unerwarteten und Effekten und Fehlern führen und wird mit großer Wahrscheinlickeit zu Nebeneffekten an anderen Stellen führen und benötigt manuelle Nacharbeit. Diese Aktion wird nicht empfohlen.\n\n(Sie können stattdessen die alte Station löschen und eine neue anlegen). Trotzdem fortfahren und Stationen umzusortieren (auf eigenes Risiko)?"),
                    "FPLedit", MessageBoxButtons.YesNo, MessageBoxType.Warning);
                if (res == DialogResult.No)
                    return; // Do not proceed as user does not want to destroy his timetable.
                // User definitely wants to do evil things. Proceed.
                var backup = pluginInterface.BackupTimetable();
                try
                {
                    StationMoveHelper.PerformUnsafeMove(Station, existingStation, newPos, route);
                }
                catch
                {
                    pluginInterface.RestoreTimetable(backup);
                    MessageBox.Show(T._("Beim Anwenden ist (wie zu erwarten) ein Problem aufgetreten. Es wurden keine Änderungen vorgenommen."), "FPLedit", MessageBoxType.Error);
                }
                finally
                {
                    pluginInterface.ClearBackup(backup);
                }
            }
            
            // Update track data.
            Station.DefaultTrackLeft.FromStandalone(stationRenderer.DefaultTrackLeft);
            Station.DefaultTrackRight.FromStandalone(stationRenderer.DefaultTrackRight);
            Station.Tracks.Clear();
            foreach (var track in stationRenderer.Tracks)
                Station.Tracks.Add(track);
            foreach (var rename in stationRenderer.TrackRenames)
                Station.ParentTimetable._InternalRenameAllTrainTracksAtStation(Station, rename.Key, rename.Value);
            foreach (var remove in stationRenderer.TrackRemoves)
                Station.ParentTimetable._InternalRemoveAllTrainTracksAtStation(Station, remove);

            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
        
        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Schließen");
            public static readonly string Position = T._("Position (km)");
            public static readonly string Title = T._("Station bearbeiten");
            public static readonly string Name = T._("Name");
            public static readonly string Abbreviation = T._("Betriebsst.-Abk.");
            public static readonly string Type = T._("Betriebsst.-Typ");
            public static readonly string RequestStop = T._("Bedarfshalt");
        }
    }
}