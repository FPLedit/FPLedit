using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor.Network
{
    //TODO: Refactor, better state handling (simple state machine, enum, attributes for status string?)
    internal class TrainRouteForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly LineRenderer lineRenderer;
        private readonly Button closeButton;
        private readonly CheckBox waypointsCheckBox;
        private readonly UrlButton waypointsDocuLink;
#pragma warning restore CS0649

        private readonly IInfo info;

        public List<Station> Path { get; private set; }
        private Train train;

        private Station staStart, staEnd; // Set-Mode

        private bool waypointsAllowed = false;
        private readonly List<Station> wayPoints = new List<Station>();

        private TrainRouteForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;

            lineRenderer.StationMovingEnabled = false;
            lineRenderer.HighlightBetweenStations = true;
            lineRenderer.SelectedRoute = -1;
            lineRenderer.SetTimetable(info.Timetable);
            lineRenderer.DisableTopBorder = true;

            this.AddSizeStateHandler();
        }

        public static TrainRouteForm EditPath(IInfo info, Train tra)
        {
            var trf = new TrainRouteForm(info)
            {
                train = tra,
                Path = tra.GetPath()
            };
            trf.Title = trf.Title.Replace("{train}", tra.TName);

            trf.lineRenderer.FixedStatusString = "Durch Klick Stationen am Anfang/Ende des Laufwegs entfernen";
            trf.lineRenderer.StationClicked += trf.ChangeRoute;
            trf.lineRenderer.SetHighlightedPath(trf.Path);
            // Disable waypoints
            trf.waypointsAllowed =
                trf.waypointsCheckBox.Visible =
                trf.waypointsDocuLink.Visible = false;
            return trf;
        }

        public static TrainRouteForm NewTrain(IInfo info)
        {
            var trf = new TrainRouteForm(info)
            {
                Title = "Fahrtstrecke für neuen Zug auswählen"
            };

            trf.lineRenderer.FixedStatusString = "Startstation auswählen";
            trf.lineRenderer.StationClicked += trf.SetRoute;

            trf.closeButton.Text = "Weiter >>";
            trf.closeButton.Enabled = false;
            // Set waypoints state according to timetable
            trf.waypointsAllowed =
                trf.waypointsCheckBox.Visible =
                trf.waypointsDocuLink.Visible = info.Timetable.HasRouteCycles;
            // Waypoints can be edited only after creating initial route
            trf.waypointsCheckBox.Enabled = false;
            return trf;
        }

        private void ChangeRoute(object sender, EventArgs e)
        {
            var sta = (Station)sender;

            if (Path == null || !Path.Any()) // We have no path. How did we come so far?
            {
                MessageBox.Show("Interner Fehler: Kein Pfad vorhanden, obwohl im Change-Route Modus. Dieses Fenster muss leider geschlossen werden!", "FPLedit", MessageBoxType.Error);
                Close(DialogResult.Cancel);
                return;
            }

            if (!Path.Contains(sta)) // Not in path. Maybe add it.
            {
                var pathfinder = new Pathfinder(info.Timetable);
                if (waypointsAllowed && waypointsCheckBox.Checked.Value)
                {
                    if (!wayPoints.Contains(sta))
                        wayPoints.Add(sta);
                    Path = pathfinder.GetPath(Path.First(), Path.Last(), wayPoints.ToArray());
                }
                else
                {
                    var pf = pathfinder.GetPath(sta, Path.First(), wayPoints.ToArray());
                    var pl = pathfinder.GetPath(Path.Last(), sta);

                    var intersectf = Path.Intersect(pf);
                    var intersectl = Path.Intersect(pl);

                    if (pf.Count > pl.Count && intersectl.Count() == 1)
                        Path.AddRange(pl.Skip(1)); // Insert at start of path
                    else if (intersectf.Count() == 1)
                        Path.InsertRange(0, pf.Take(pf.Count - 1)); // Insert at the end of path
                    else
                        MessageBox.Show("Diese Station kann nicht zum Laufweg hinzugefügt werden, da dabei ein verzweigter Laufweg entstehen würde!", "FPLedit");
                }
            }
            // From here on, we search for reasons, NOT removing the station.
            else if (sta != Path.Last() && sta != Path.First()) // Currently already in path, but not at both ends
            {
                MessageBox.Show("Diese Station kann nicht aus dem Laufweg entfernt werden, da sie zwischen Start- und Zielstatation liegt. Nur Start- und Zielstationen können entfernt werden!",
                    "FPLedit");
            }
            else if (Path.Count < 3)
            {
                MessageBox.Show("Diese Station kann nicht aus dem Laufweg entfernt werden, da mindestens immer 2 Stationen enthalten sein müssen!",
                    "FPLedit");
            }
            else
                Path.Remove(sta); // No reasons found, finally remove.

            lineRenderer.SetHighlightedPath(Path);
        }

        private void SetRoute(object sender, EventArgs e)
        {
            if (staStart == null)
            {
                staStart = (Station)sender;
                lineRenderer.SetHighlightedPath(new[] { staStart });
                lineRenderer.FixedStatusString = "Zielstation auswählen";
            }
            else if (staEnd == null)
            {
                staEnd = (Station)sender;

                if (staEnd == staStart)
                {
                    MessageBox.Show("Züge mit gleicher Start- und Zielstation sind nicht möglich!");
                    staEnd = null;
                    return;
                }

                var pathfinder = new Pathfinder(info.Timetable);
                Path = pathfinder.GetPath(staStart, staEnd);
                lineRenderer.SetHighlightedPath(Path);
                closeButton.Enabled = true;

                lineRenderer.FixedStatusString = "";

                // Wechseln in den Änderungsmodus
                lineRenderer.StationClicked -= SetRoute;
                lineRenderer.StationClicked += ChangeRoute;
                // Waypoints can be edited only after creating initial route, and only if available
                waypointsCheckBox.Enabled = waypointsAllowed;
            }
        }

        //TODO: Removed because of bugs. I think this is not urgent, as this feature is probably not used by anyone. Also re-enable in XAML.
        //private void ResetRoute(object sender, EventArgs e)
        //{
        //    staStart = null;
        //    staEnd = null;

        //    Path = new List<Station>();

        //    lineRenderer.ClearHighlightedPath();
        //    closeButton.Enabled = false;
        //    lineRenderer.FixedStatusString = "Startstation auswählen";

        //    if (train != null)
        //    {
        //        // Wechsel in den Neu Setzen-Modus
        //        lineRenderer.StationClicked -= ChangeRoute;
        //        lineRenderer.StationClicked += SetRoute;
        //    }
        //}

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (Path.Distinct().Count() < Path.Count)
            {
                MessageBox.Show("Der Laufweg enthält eine Station mehr als einmal. Dies ist aktuell nicht möglich. Ggf. fehlt ein weiterer Wegpunkt!",
                    "FPLedit");
                return;
            }

            if (train != null)
            {
                var ardps = train.GetArrDeps();

                foreach (var ardp in ardps) // Alle alten Stationen entfernen
                    train.RemoveArrDep(ardp.Key);

                train.AddAllArrDeps(Path);
                foreach (var ardp in ardps)
                {
                    if (!Path.Contains(ardp.Key))
                        continue;
                    train.GetArrDep(ardp.Key).ApplyCopy(ardp.Value);
                }

                train.RemoveOrphanedTimes();
            }
            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Keys.R)
                lineRenderer.DispatchKeystroke(e);
            base.OnKeyDown(e);
        }
    }
}
