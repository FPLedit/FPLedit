using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Network
{
    public class LineEditingControl : Panel
    {
        private IInfo info;
        private int selectedRoute = Timetable.LINEAR_ROUTE_ID;

#pragma warning disable CS0649
        private DropDown routesComboBox;
        private LineRenderer lineRenderer;
        private Button newLineButton, newButton, joinLineButton;
        private Divider divider1;
        private StackLayout toolbar;
#pragma warning restore CS0649

        public LineEditingControl()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
        }

        private IEnumerable<ListItem> GetRouteNames(Timetable tt)
        {
            ListItem BuildItem(string name, int route)
            {
                var itm = new ListItem
                {
                    Text = name,
                    Tag = route
                };
                return itm;
            }

            if (tt.Type == TimetableType.Network)
            {
                var rt = tt.GetRoutes();
                foreach (var route in rt)
                    yield return BuildItem(route.GetRouteName(), route.Index);
            }
            else
                yield return BuildItem("<Standard>", Timetable.LINEAR_ROUTE_ID);
        }

        private void ReloadRouteNames(bool forceReload)
        {
            if (info.Timetable == null)
            {
                routesComboBox.Items.Clear();
                return;
            }

            int oldSelected = -1;
            if (routesComboBox.SelectedValue != null)
                oldSelected = (int)((ListItem)routesComboBox.SelectedValue).Tag;
            var routes = GetRouteNames(info.Timetable);
            routesComboBox.Items.Clear();
            routesComboBox.Items.AddRange(routes);

            if (oldSelected != -1 && !forceReload && routes.Any(r => (int)r.Tag == oldSelected))
            {
                var rl = routes.ToList();
                routesComboBox.SelectedIndex = rl.IndexOf(rl.FirstOrDefault(li => (int)li.Tag == oldSelected));
                selectedRoute = oldSelected;
            }
            else
            {
                selectedRoute = 0;
                routesComboBox.SelectedIndex = 0;
            }
        }

        public void Initialize(IInfo info)
        {
            this.info = info;
            string lastFn = null;
            info.FileStateChanged += (s, e) =>
            {
                ReloadTimetable();
                newButton.Enabled = routesComboBox.Enabled = newLineButton.Enabled = e.FileState.Opened;
                routesComboBox.Visible = info.FileState.Opened;

                ReloadRouteNames(lastFn != e.FileState.FileName);
                lastFn = e.FileState.FileName;

                newLineButton.Visible = joinLineButton.Visible = divider1.Visible = routesComboBox.Visible = info.FileState.Opened && info.Timetable.Type == TimetableType.Network;
                newLineButton.Enabled = joinLineButton.Enabled = info.FileState.Opened && info.Timetable.Type == TimetableType.Network && info.Timetable.GetRoutes().Any();

                foreach (Control c in toolbar.Controls)
                {
                    if (c.Tag is IRouteAction act)
                        c.Enabled = act.IsEnabled(info);
                }
            };
            info.ExtensionsLoaded += (s, e) =>
            {
                var actions = info.GetRegistered<IRouteAction>();
                if (actions.Length > 0)
                    toolbar.Items.Add(new Divider());

                foreach (var action in actions)
                {
                    var btn = new Button()
                    {
                        Text = action.DisplayName,
                        Tag = action,
                    };
                    btn.Enabled = action.IsEnabled(info);
                    btn.Click += (se, ev) => action.Show(info, info.Timetable?.GetRoute(selectedRoute));
                    toolbar.Items.Add(btn);
                }
            };

            routesComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (routesComboBox.SelectedIndex == -1)
                    return;
                selectedRoute = (int)((ListItem)routesComboBox.Items[routesComboBox.SelectedIndex]).Tag;
                lineRenderer.SelectedRoute = selectedRoute;

                info.FileState.SelectedRoute = selectedRoute;
            };

            lineRenderer.StationDoubleClicked += (s, e) =>
            {
                info.StageUndoStep();
                var sta = (Station)s;
                var r = selectedRoute;
                if (sta.Routes.Length == 1)
                    r = sta.Routes[0];
                if (!sta.Routes.Contains(r))
                {
                    MessageBox.Show("Die Station liegt auf mehreren Strecken. Bitte zuerst die Strecke auswählen, für die die Station bearbeitet werden soll!",
                        "FPLedit", MessageBoxButtons.OK, MessageBoxType.Warning);
                    return;
                }
                var nsf = new EditStationForm(sta, r);
                if (nsf.ShowModal(this) == DialogResult.Ok)
                {
                    ReloadTimetable();
                    info.SetUnsaved();
                }
            };
            lineRenderer.StationRightClicked += (s, e) =>
            {
                var menu = new ContextMenu();
                var itm = menu.CreateItem("Löschen");
                itm.Click += (se, ar) =>
                {
                    info.StageUndoStep();
                    info.Timetable.RemoveStation((Station)s);
                    ReloadTimetable();
                    info.SetUnsaved();
                };
                menu.Show(this);
            };
            lineRenderer.NewRouteAdded += (s, args) => ReloadRouteNames(false);
            lineRenderer.StationMoveEnd += (s, args) => (info.FileState as FileState).Saved = false;
            newButton.Click += (s, e) =>
            {
                info.StageUndoStep();
                var nsf = new EditStationForm(info.Timetable, selectedRoute);
                if (nsf.ShowModal(this) == DialogResult.Ok)
                {
                    Station sta = nsf.Station;
                    if (info.Timetable.Type == TimetableType.Network)
                    {
                        var handler = new StaPosHandler();
                        handler.SetMiddlePos(selectedRoute, sta, info.Timetable);
                        var r = sta.Routes.ToList();
                        r.Add(lineRenderer.SelectedRoute);
                        sta.Routes = r.ToArray();
                    }
                    info.Timetable.AddStation(sta, selectedRoute);
                    info.SetUnsaved();
                    ReloadTimetable();
                }
            };
            newLineButton.Click += (s, e) =>
            {
                info.StageUndoStep();
                var nlf = new EditStationForm(info.Timetable);
                if (nlf.ShowModal(this) == DialogResult.Ok)
                {
                    lineRenderer.StartAddStation(nlf.Station, nlf.Position);
                    info.SetUnsaved();
                }
            };
            joinLineButton.Click += (s, e) =>
            {
                info.StageUndoStep();
                var epf = new EditPositionForm();
                if (epf.ShowModal(this) == DialogResult.Ok)
                {
                    lineRenderer.StartJoinLines(epf.Position);
                    ReloadTimetable();
                    info.SetUnsaved();
                }
            };

            KeyDown += (s, e) => DispatchKeystroke(e);
        }

        public void ReloadTimetable()
            => lineRenderer.SetTimetable(info.Timetable);

        public void DispatchKeystroke(KeyEventArgs e)
            => lineRenderer.DispatchKeystroke(e);

        public void ResetPan()
            => lineRenderer.Pan = new PointF();
    }

    public class Divider : Panel
    {
        public Divider()
        {
            BackgroundColor = Colors.Gray;
            Size = new Size(2, 23);
        }
    }
}
