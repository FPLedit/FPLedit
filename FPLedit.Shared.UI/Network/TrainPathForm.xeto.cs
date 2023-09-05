using Eto.Forms;
using FPLedit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace FPLedit.Shared.UI.Network;

//TODO: Refactor out logic code
public sealed class TrainPathForm : FDialog<DialogResult>
{
#pragma warning disable CS0649,CA2213
    private readonly NetworkRenderer networkRenderer = default!;
    private readonly Button closeButton = default!;
    private readonly CheckBox waypointsCheckBox = default!;
    private readonly UrlButton waypointsDocuLink = default!;
#pragma warning restore CS0649,CA2213

    private readonly Pathfinder pathfinder;
    private readonly bool globalWaypointsAllowed = false;

    // Internal state: path & waypoints
    private readonly List<Station> wayPoints = new ();
    private List<Station>? path;

    public List<Station>? Path
    {
        get => path; private set
        {
            path = value;
            networkRenderer.SetHighlightedPath(value);
        }
    }

    public IReadOnlyList<Station> Waypoints => wayPoints.AsReadOnly();
        
    private readonly Train? train;

    private readonly RouteEditState stateSetRoute, stateChangeRoute, stateAddWaypoints;

    private TrainPathForm(IPluginInterface pluginInterface, bool waypointsAllowed, Train? train = null)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        pathfinder = new Pathfinder(pluginInterface.Timetable);

        networkRenderer.StationMovingEnabled = false;
        networkRenderer.HighlightBetweenStations = true;
        networkRenderer.SelectedRoute = -1;
        networkRenderer.SetTimetable(pluginInterface.Timetable);
        networkRenderer.DisableTopBorder = true;
        networkRenderer.SetPanCenterEnabled = false;
        networkRenderer.StationClicked += HandleStationClick;

        globalWaypointsAllowed =
            waypointsCheckBox.Visible =
                waypointsDocuLink.Visible = waypointsAllowed;

        // Maybe initialize train data
        this.train = train;
        if (train != null)
            Path = train.GetPath();

        // Initialize edit states
        stateSetRoute = new RouteEditState()
        {
            InitialStatusString = T._("Startstation auswählen"),
            StationClicked = SetRoute,
            WaypointsTransitionAllowed = false, // Waypoints can be edited only after creating initial route
            IsTerminating = false,
            StateInitialize = InitSetRoute,
        };

        stateChangeRoute = new RouteEditState()
        {
            InitialStatusString = T._("Durch Klick Stationen am Anfang/Ende des Laufwegs entfernen"),
            StationClicked = ChangeRoute,
            WaypointsTransitionAllowed = true, // Set with global values
            IsTerminating = true,
        };

        stateAddWaypoints = new RouteEditState()
        {
            InitialStatusString = T._("Durch Klick Wegpunkte in der Reihenfolge des Durchfahrens hinzufügen"),
            StationClicked = AddWaypoint,
            WaypointsTransitionAllowed = true,
            IsTerminating = true,
        };

        this.AddSizeStateHandler();
    }

    public static TrainPathForm EditPath(IPluginInterface pluginInterface, Train tra)
    {
        var tpf = new TrainPathForm(pluginInterface, false, tra); // Disable waypoints, because we don't know old waypoints
        tpf.Title = tpf.Title.Replace("{train}", tra.TName);

        tpf.Transition(tpf.stateChangeRoute);
        tpf.networkRenderer.SetHighlightedPath(tpf.Path);
        return tpf;
    }

    public static TrainPathForm NewTrain(IPluginInterface pluginInterface)
    {
        var tpf = new TrainPathForm(pluginInterface, pluginInterface.Timetable.HasRouteCycles) // Set waypoints state according to timetable
        {
            Title = T._("Fahrtstrecke für neuen Zug auswählen")
        };

        tpf.Transition(tpf.stateSetRoute);

        tpf.closeButton.Text = T._("Weiter >>");
        return tpf;
    }

    #region State handlers
    private void ChangeRoute(Station sta)
    {
        if (PathSafeguard(nameof(ChangeRoute)))
            return;

        if (!Path.Contains(sta)) // Not in path. Maybe add it.
        {
            var pf = pathfinder.GetPath(sta, Path.First());
            var pl = pathfinder.GetPath(Path.Last(), sta);

            var intersectf = Path.Intersect(pf);
            var intersectl = Path.Intersect(pl);

            if (pf.Count > pl.Count && intersectl.Count() == 1)
                Path.AddRange(pl.Skip(1)); // Insert at start of path
            else if (intersectf.Count() == 1)
                Path.InsertRange(0, pf.Take(pf.Count - 1)); // Insert at the end of path
            else
                MessageBox.Show(T._("Diese Station kann nicht zum Laufweg hinzugefügt werden, da dabei ein verzweigter Laufweg entstehen würde!"), "FPLedit");

        }
        // From here on, we search for reasons, NOT removing the station.
        else if (sta != Path.Last() && sta != Path.First()) // Currently already in path, but not at both ends
        {
            MessageBox.Show(T._("Diese Station kann nicht aus dem Laufweg entfernt werden, da sie zwischen Start- und Zielstatation liegt. Nur Start- und Zielstationen können entfernt werden!"),
                "FPLedit");
        }
        else if (Path.Count < 3)
        {
            MessageBox.Show(T._("Diese Station kann nicht aus dem Laufweg entfernt werden, da mindestens immer 2 Stationen enthalten sein müssen!"),
                "FPLedit");
        }
        else
            Path.Remove(sta); // No reasons found, finally remove.

        networkRenderer.SetHighlightedPath(Path);
    }

    private void AddWaypoint(Station sta)
    {
        if (PathSafeguard(nameof(AddWaypoint)))
            return;

        if (!wayPoints.Contains(sta))
            wayPoints.Add(sta);
        Path = pathfinder.GetPath(Path.First(), Path.Last(), wayPoints.ToArray());
    }

    private void SetRoute(Station sta)
    {
        if (Path == null || !Path.Any()) // There is no current path, so we set the first station
        {
            Path = new[] { sta }.ToList();
            networkRenderer.FixedStatusString = T._("Zielstation auswählen");
        }
        else if (Path.Count == 1) // We already have the first station set, so set the end of path
        {
            if (sta == Path.First())
            {
                MessageBox.Show(T._("Züge mit gleicher Start- und Zielstation sind nicht möglich!"), "FPLedit");
                return;
            }

            Path = pathfinder.GetPath(Path.First(), sta);

            if (!Path.Any())
            {
                MessageBox.Show(T._("Es konnte kein gültiger Laufweg für diesen Zug ermittelt werden!"), "FPLedit", MessageBoxType.Error);
                return;
            }

            Transition(stateChangeRoute);
        }
    }

    private void InitSetRoute() // Initialization for state "SetRoute"
    {
        Path = new List<Station>();
        wayPoints.Clear();
    }

    [MemberNotNullWhen(false, "Path")]
    private bool PathSafeguard(string mode) // Helper, used in states
    {
        var hasNoPath = Path == null || !Path.Any();
        if (hasNoPath) // We have no path. How did we come so far?
        {
            MessageBox.Show(T._("Interner Fehler: Unerwartet kein Pfad vorhanden! Dieses Fenster muss leider geschlossen werden! ({0})", mode), "FPLedit", MessageBoxType.Error);
            Close(DialogResult.Cancel);
        }
        return hasNoPath;
    }
    #endregion

    private void ResetRoute(object sender, EventArgs e)
        => Transition(stateSetRoute); // Just switch back to state SetRoute

    private void WaypointsCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        if (globalWaypointsAllowed && waypointsCheckBox.Checked!.Value)
            Transition(stateAddWaypoints);
        else
            Transition(stateChangeRoute);
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        if (PathSafeguard("Close"))
            return;

        if (Path.Distinct().Count() < Path.Count)
        {
            MessageBox.Show(T._("Der Laufweg enthält eine Station mehr als einmal. Dies ist aktuell nicht möglich. Ggf. fehlt ein weiterer Wegpunkt?"),
                "FPLedit");
            return;
        }

        if (train != null)
        {
            var ardps = train.GetArrDepsUnsorted();

            foreach (var ardp in ardps) // Remove all old stations
                train.RemoveArrDep(ardp.Key);

            train.AddAllArrDeps(Path);
            foreach (var (sta, ardp) in ardps)
            {
                if (!Path.Contains(sta))
                    continue;
                train.GetArrDep(sta).ApplyCopy(ardp);
            }

            train.RemoveOrphanedTimes();
        }
        Close(DialogResult.Ok);
    }

    private void CancelButton_Click(object sender, EventArgs e)
        => Close(DialogResult.Cancel);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Keys.R && e.Modifiers == Keys.None)
            networkRenderer.DispatchKeystroke(e);
        base.OnKeyDown(e);
    }

    #region State machine
    private RouteEditState? currentState;

    private void Transition(RouteEditState targetState)
    {
        networkRenderer.FixedStatusString = targetState.InitialStatusString;
        closeButton.Enabled = targetState.IsTerminating;
        waypointsCheckBox.Enabled = targetState.WaypointsTransitionAllowed && globalWaypointsAllowed;
        targetState.StateInitialize?.Invoke();
        currentState = targetState;
    }

    private void HandleStationClick(object? sender, EventArgs e)
    {
        currentState?.StationClicked?.Invoke((Station)sender!);
    }

    private class RouteEditState
    {
        public string InitialStatusString { get; init; } = "";
        public Action<Station>? StationClicked { get; init; }
        public bool WaypointsTransitionAllowed { get; init; }
        public bool IsTerminating { get; init; }
        public Action? StateInitialize { get; init; }
    }
    #endregion
        
    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Title = T._("Fahrtstrecke bearbeiten: {train}");
        public static readonly string Help = T._("Hilfe zu Wegpunkten");
        public static readonly string Waypoints = T._("Wegpunkte setzen (für zirkuläre Netze)");
        public static readonly string HelpUrl = T._("https://fahrplan.manuelhu.de/fahrplaene-bearbeiten/cycles/");
        public static readonly string Reset = T._("Laufweg zurücksetzen/leeren");
    }
}