﻿using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System.Linq;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI.Network;

namespace FPLedit.Editor.Rendering;

internal sealed class NetworkEditingControl : Panel
{
    private IPluginInterface pluginInterface = null!;

    private const int ICON_SIZE = 32;
    private const int ICON_SIZE_CONTEXT = 16;
    public const string TOOLBAR_ICON_SETTINGS_KEY = "ui.toolbar-icons";
    private bool toolbarUseIcons = true;

#pragma warning disable CS0649,CA2213
    private readonly RoutesDropDown routesDropDown = null!;
    private readonly NetworkRenderer networkRenderer = null!;
    private readonly Button newLineButton = null!, newButton = null!, joinLineButton = null!;
    private readonly Divider divider1 = null!;
    private readonly StackLayout toolbar = null!;
#pragma warning restore CS0649,CA2213

    public static readonly Keys[] DispatchableKeys = { Keys.Home };

    public NetworkEditingControl()
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);
    }

    public void Initialize(IPluginInterface pluginInterface)
    {
        void ToolbarBtn(Button btn, Bitmap img)
        {
            btn.ToolTip = btn.Text.Replace("&", "");
            btn.Text = "";
            btn.Image = img.WithSize(ICON_SIZE, ICON_SIZE);
            btn.ImagePosition = ButtonImagePosition.Overlay;
        }

        toolbarUseIcons = pluginInterface.Settings.Get(TOOLBAR_ICON_SETTINGS_KEY, true);
        // Convert the toolbar buttons to icons.
        if (toolbarUseIcons)
        {
            ToolbarBtn(newButton, new Bitmap(this.GetResource("Resources.toolbar-add-station.png")));
            ToolbarBtn(newLineButton, new Bitmap(this.GetResource("Resources.toolbar-add-line.png")));
            ToolbarBtn(joinLineButton, new Bitmap(this.GetResource("Resources.toolbar-join-line.png")));

            toolbar.VerticalContentAlignment = VerticalAlignment.Center;
        }

        this.pluginInterface = pluginInterface;
        routesDropDown.Initialize(pluginInterface);
        pluginInterface.FileStateChanged += (_, e) =>
        {
            ReloadTimetable();
            newButton.Enabled = routesDropDown.Enabled = newLineButton.Enabled = e.FileState.Opened;

            newLineButton.Visible = joinLineButton.Visible = divider1.Visible = routesDropDown.Visible = pluginInterface.FileState.Opened && pluginInterface.Timetable.Type == TimetableType.Network;
            newLineButton.Enabled = joinLineButton.Enabled = pluginInterface.FileState.Opened && pluginInterface.Timetable.Type == TimetableType.Network && pluginInterface.Timetable.GetRoutes().Any();

            foreach (Control c in toolbar.Controls)
            {
                if (c.Tag is IRouteAction act)
                    c.Enabled = act.IsEnabled(pluginInterface);
            }
        };
        pluginInterface.ExtensionsLoaded += (_, _) =>
        {
            var actions = pluginInterface.GetRegistered<IRouteAction>();
            if (actions.Length > 0)
                toolbar.Items.Add(new Divider());

            foreach (var action in actions)
            {
                var btn = new Button { Tag = action, Text = action.DisplayName, Enabled = action.IsEnabled(pluginInterface) };
                if (action.EtoIconBitmap is Bitmap bitmap && toolbarUseIcons)
                    ToolbarBtn(btn, bitmap);

                btn.Click += (_, _) => action.Invoke(pluginInterface, pluginInterface.TimetableMaybeNull?.GetRoute(routesDropDown.SelectedRoute));
                toolbar.Items.Add(btn);
            }
        };

        routesDropDown.SelectedRouteChanged += (_, _) =>
        {
            networkRenderer.SelectedRoute = routesDropDown.SelectedRoute;
            pluginInterface.FileState.SelectedRoute = routesDropDown.SelectedRoute;
        };

        networkRenderer.StationDoubleClicked += (s, _) =>
        {
            var sta = (Station?) s;
            if (sta == null) return; // Something weird happened.

            pluginInterface.StageUndoStep();
            var r = routesDropDown.SelectedRoute;
            if (sta.Routes.Length == 1)
                r = sta.Routes[0];
            if (!sta.Routes.Contains(r))
            {
                MessageBox.Show(T._("Die Station liegt auf mehreren Strecken. Bitte zuerst die Strecke auswählen, für die die Station bearbeitet werden soll!"),
                    "FPLedit", MessageBoxButtons.OK, MessageBoxType.Warning);
                return;
            }
            var nsf = new EditStationForm(pluginInterface, sta, r);
            if (nsf.ShowModal(this) == null) return;

            ReloadTimetable();
            pluginInterface.SetUnsaved();
        };
        networkRenderer.StationRightClicked += (s, _) =>
        {
            var sta = (Station?) s;
            if (sta == null) return; // Something weird happened.

            var menu = new ContextMenu();
            var deleteItem = menu.CreateItem(T._("Löschen"));
            deleteItem.Click += (_, _) =>
            {
                if (pluginInterface.Timetable.WouldProduceAmbiguousRoute(sta))
                {
                    MessageBox.Show(T._("Sie versuchen eine Station zu löschen, ohne die danach zwei Routen zusammenfallen, das heißt zwei Stationen auf mehr als einer Route ohne Zwischenstation verbunden sind.\n\n" +
                                        "Der Konflikt kann nicht automatisch aufgehoben werden."), "FPLedit", MessageBoxType.Error);
                    return;
                }

                if (sta.IsJunction)
                {
                    MessageBox.Show(T._("Sie versuchen eine Station zu löschen, die an einem Kreuzungspunkt zweier Strecken liegt. Dies ist leider nicht möglich."), "FPLedit", MessageBoxType.Error);
                    return;
                }

                pluginInterface.StageUndoStep();
                pluginInterface.Timetable.RemoveStation(sta);
                ReloadTimetable();
                pluginInterface.SetUnsaved();
            };
            if (pluginInterface.Timetable.Type == TimetableType.Network)
            {
                var breakItem = menu.CreateItem(T._("Eine Strecke trennen"));
                breakItem.Image = new Bitmap(this.GetResource("Resources.toolbar-break-line.png")).WithSize(ICON_SIZE_CONTEXT, ICON_SIZE_CONTEXT);
                breakItem.Click += (_, _) =>
                {
                    pluginInterface.StageUndoStep();
                    networkRenderer.StartBreakLine(sta);
                    pluginInterface.SetUnsaved();
                };
            }
            menu.Show(this);
        };
        networkRenderer.RoutesChanged += (_, args) =>
        {
            (pluginInterface.FileState as FileState)!.Saved = false;
            routesDropDown.SelectedRoute = args.Value;
        };
        networkRenderer.StationMoveEnd += (_, _) => (pluginInterface.FileState as FileState)!.Saved = false;
        newButton.Click += (_, _) =>
        {
            pluginInterface.StageUndoStep();
            var nsf = new EditStationForm(pluginInterface, pluginInterface.Timetable, routesDropDown.SelectedRoute);
            var result = nsf.ShowModal(this);
            if (result == null) return;

            if (pluginInterface.Timetable.Type == TimetableType.Network)
            {
                var handler = new StationCanvasPositionHandler();
                handler.SetMiddlePos(routesDropDown.SelectedRoute, result.Station, pluginInterface.Timetable);
            }
            pluginInterface.Timetable.AddStation(result.Station, networkRenderer.SelectedRoute);
            pluginInterface.SetUnsaved();
            ReloadTimetable();
        };
        newLineButton.Click += (_, _) =>
        {
            pluginInterface.StageUndoStep();
            var nlf = new EditStationForm(pluginInterface, pluginInterface.Timetable, Timetable.UNASSIGNED_ROUTE_ID);
            var result = nlf.ShowModal(this);
            if (result == null) return;

            networkRenderer.StartAddStation(result.Station, result.NewPosition!.Value);
            pluginInterface.SetUnsaved();
        };
        joinLineButton.Click += (_, _) =>
        {
            pluginInterface.StageUndoStep();
            var epf = new EditPositionForm();
            var newPosition = epf.ShowModal(this);
            if (newPosition.HasValue)
            {
                networkRenderer.StartJoinLines(newPosition.Value);
                ReloadTimetable();
                pluginInterface.SetUnsaved();
            }
        };

        KeyDown += (_, e) => DispatchKeystroke(e);
    }

    public void ReloadTimetable()
        => networkRenderer.SetTimetable(pluginInterface.TimetableMaybeNull);

    public void DispatchKeystroke(KeyEventArgs e)
    {
        if (e.Modifiers != Keys.None)
            return;
        switch (e.Key)
        {
            // See DispatchableKeys
            case Keys.Home:
                routesDropDown.Focus();
                e.Handled = true;
                break;
        }

        if (!e.Handled)
            networkRenderer.DispatchKeystroke(e);
    }

    public void ResetPan()
        => networkRenderer.Pan = PointF.Empty;

    private static class L
    {
        public static readonly string NewStation = T._("&Neue Station");
        public static readonly string NewLine = T._("Neue &Strecke");
        public static readonly string JoinLines = T._("Strecken &zusammenführen");
    }
}