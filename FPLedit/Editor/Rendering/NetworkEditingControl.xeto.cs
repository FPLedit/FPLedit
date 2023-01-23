using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System.Linq;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI.Network;

namespace FPLedit.Editor.Rendering
{
    internal sealed class NetworkEditingControl : Panel
    {
        private IPluginInterface pluginInterface;

#pragma warning disable CS0649
        private readonly RoutesDropDown routesDropDown;
        private readonly NetworkRenderer networkRenderer;
        private readonly Button newLineButton, newButton, joinLineButton;
        private readonly Divider divider1;
        private readonly StackLayout toolbar;
#pragma warning restore CS0649
        
        public static readonly Keys[] DispatchableKeys = { Keys.Home };

        public NetworkEditingControl()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
        }

        public void Initialize(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            routesDropDown.Initialize(pluginInterface);
            pluginInterface.FileStateChanged += (_, e) =>
            {
                ReloadTimetable();
                newButton.Enabled = routesDropDown.Enabled = newLineButton.Enabled = e.FileState.Opened;
                routesDropDown.Visible = pluginInterface.FileState.Opened;

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
                    var btn = new Button()
                    {
                        Text = action.DisplayName,
                        Tag = action,
                    };
                    btn.Enabled = action.IsEnabled(pluginInterface);
                    btn.Click += (_, _) => action.Invoke(pluginInterface, pluginInterface.Timetable?.GetRoute(routesDropDown.SelectedRoute));
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
                pluginInterface.StageUndoStep();
                var sta = (Station)s;
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
                if (nsf.ShowModal(this) == DialogResult.Ok)
                {
                    ReloadTimetable();
                    pluginInterface.SetUnsaved();
                }
            };
            networkRenderer.StationRightClicked += (s, _) =>
            {
                var menu = new ContextMenu();
                var itm = menu.CreateItem("Löschen");
                itm.Click += (se, ar) =>
                {
                    var sta = (Station) s;
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
                menu.Show(this);
            };
            networkRenderer.NewRouteAdded += (s, args) =>
            {
                (pluginInterface.FileState as FileState).Saved = false;
                routesDropDown.SelectedRoute = args.Value;
            };
            networkRenderer.StationMoveEnd += (_, _) => (pluginInterface.FileState as FileState).Saved = false;
            newButton.Click += (_, _) =>
            {
                pluginInterface.StageUndoStep();
                var nsf = new EditStationForm(pluginInterface, pluginInterface.Timetable, routesDropDown.SelectedRoute);
                if (nsf.ShowModal(this) == DialogResult.Ok)
                {
                    Station sta = nsf.Station;
                    if (pluginInterface.Timetable.Type == TimetableType.Network)
                    {
                        var handler = new StationCanvasPositionHandler();
                        handler.SetMiddlePos(routesDropDown.SelectedRoute, sta, pluginInterface.Timetable);
                    }
                    pluginInterface.Timetable.AddStation(sta, networkRenderer.SelectedRoute);
                    pluginInterface.SetUnsaved();
                    ReloadTimetable();
                }
            };
            newLineButton.Click += (_, _) =>
            {
                pluginInterface.StageUndoStep();
                var nlf = new EditStationForm(pluginInterface, pluginInterface.Timetable, Timetable.UNASSIGNED_ROUTE_ID);
                if (nlf.ShowModal(this) == DialogResult.Ok)
                {
                    networkRenderer.StartAddStation(nlf.Station, nlf.Position);
                    pluginInterface.SetUnsaved();
                }
            };
            joinLineButton.Click += (_, _) =>
            {
                pluginInterface.StageUndoStep();
                var epf = new EditPositionForm();
                if (epf.ShowModal(this) == DialogResult.Ok)
                {
                    networkRenderer.StartJoinLines(epf.Position);
                    ReloadTimetable();
                    pluginInterface.SetUnsaved();
                }
            };

            KeyDown += (_, e) => DispatchKeystroke(e);
        }

        public void ReloadTimetable()
            => networkRenderer.SetTimetable(pluginInterface.Timetable);

        public void DispatchKeystroke(KeyEventArgs e)
        {
            switch (e.Key)
            {
                // See DISPATCHABLE_KEYS
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
}
