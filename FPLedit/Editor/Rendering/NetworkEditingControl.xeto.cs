using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public NetworkEditingControl()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
        }

        public void Initialize(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            routesDropDown.Initialize(pluginInterface);
            pluginInterface.FileStateChanged += (s, e) =>
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
            pluginInterface.ExtensionsLoaded += (s, e) =>
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
                    btn.Click += (se, ev) => action.Show(pluginInterface, pluginInterface.Timetable?.GetRoute(routesDropDown.SelectedRoute));
                    toolbar.Items.Add(btn);
                }
            };

            routesDropDown.SelectedRouteChanged += (s, e) =>
            {
                networkRenderer.SelectedRoute = routesDropDown.SelectedRoute;
                pluginInterface.FileState.SelectedRoute = routesDropDown.SelectedRoute;
            };

            networkRenderer.StationDoubleClicked += (s, e) =>
            {
                pluginInterface.StageUndoStep();
                var sta = (Station)s;
                var r = routesDropDown.SelectedRoute;
                if (sta.Routes.Length == 1)
                    r = sta.Routes[0];
                if (!sta.Routes.Contains(r))
                {
                    MessageBox.Show("Die Station liegt auf mehreren Strecken. Bitte zuerst die Strecke auswählen, für die die Station bearbeitet werden soll!",
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
            networkRenderer.StationRightClicked += (s, e) =>
            {
                var menu = new ContextMenu();
                var itm = menu.CreateItem("Löschen");
                itm.Click += (se, ar) =>
                {
                    pluginInterface.StageUndoStep();
                    pluginInterface.Timetable.RemoveStation((Station)s);
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
            networkRenderer.StationMoveEnd += (s, args) => (pluginInterface.FileState as FileState).Saved = false;
            newButton.Click += (s, e) =>
            {
                pluginInterface.StageUndoStep();
                var nsf = new EditStationForm(pluginInterface, pluginInterface.Timetable, routesDropDown.SelectedRoute);
                if (nsf.ShowModal(this) == DialogResult.Ok)
                {
                    Station sta = nsf.Station;
                    if (pluginInterface.Timetable.Type == TimetableType.Network)
                    {
                        var handler = new StaPosHandler();
                        handler.SetMiddlePos(routesDropDown.SelectedRoute, sta, pluginInterface.Timetable);
                        pluginInterface.Timetable.StationAddRoute(sta, networkRenderer.SelectedRoute);
                    }
                    pluginInterface.Timetable.AddStation(sta, routesDropDown.SelectedRoute);
                    pluginInterface.SetUnsaved();
                    ReloadTimetable();
                }
            };
            newLineButton.Click += (s, e) =>
            {
                pluginInterface.StageUndoStep();
                var nlf = new EditStationForm(pluginInterface, pluginInterface.Timetable);
                if (nlf.ShowModal(this) == DialogResult.Ok)
                {
                    networkRenderer.StartAddStation(nlf.Station, nlf.Position);
                    pluginInterface.SetUnsaved();
                }
            };
            joinLineButton.Click += (s, e) =>
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

            KeyDown += (s, e) => DispatchKeystroke(e);
        }

        public void ReloadTimetable()
            => networkRenderer.SetTimetable(pluginInterface.Timetable);

        public void DispatchKeystroke(KeyEventArgs e)
            => networkRenderer.DispatchKeystroke(e);

        public void ResetPan()
            => networkRenderer.Pan = new PointF();
    }
}
