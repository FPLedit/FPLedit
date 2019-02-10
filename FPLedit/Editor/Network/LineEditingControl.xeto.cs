﻿using Eto.Drawing;
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

#pragma warning disable CS0649
        private RoutesDropDown routesDropDown;
        private LineRenderer lineRenderer;
        private Button newLineButton, newButton, joinLineButton;
        private Divider divider1;
        private StackLayout toolbar;
#pragma warning restore CS0649

        public LineEditingControl()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
        }



        public void Initialize(IInfo info)
        {
            this.info = info;
            routesDropDown.Initialize(info);
            info.FileStateChanged += (s, e) =>
            {
                ReloadTimetable();
                newButton.Enabled = routesDropDown.Enabled = newLineButton.Enabled = e.FileState.Opened;
                routesDropDown.Visible = info.FileState.Opened;

                newLineButton.Visible = joinLineButton.Visible = divider1.Visible = routesDropDown.Visible = info.FileState.Opened && info.Timetable.Type == TimetableType.Network;
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
                    btn.Click += (se, ev) => action.Show(info, info.Timetable?.GetRoute(routesDropDown.SelectedRoute));
                    toolbar.Items.Add(btn);
                }
            };

            routesDropDown.SelectedRouteChanged += (s, e) =>
            {
                lineRenderer.SelectedRoute = routesDropDown.SelectedRoute;
                info.FileState.SelectedRoute = routesDropDown.SelectedRoute;
            };

            lineRenderer.StationDoubleClicked += (s, e) =>
            {
                info.StageUndoStep();
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
            //TODO: Neue Strecke auch auswählen
            lineRenderer.NewRouteAdded += (s, args) => (info.FileState as FileState).Saved = false;
            lineRenderer.StationMoveEnd += (s, args) => (info.FileState as FileState).Saved = false;
            newButton.Click += (s, e) =>
            {
                info.StageUndoStep();
                var nsf = new EditStationForm(info.Timetable, routesDropDown.SelectedRoute);
                if (nsf.ShowModal(this) == DialogResult.Ok)
                {
                    Station sta = nsf.Station;
                    if (info.Timetable.Type == TimetableType.Network)
                    {
                        var handler = new StaPosHandler();
                        handler.SetMiddlePos(routesDropDown.SelectedRoute, sta, info.Timetable);
                        var r = sta.Routes.ToList();
                        r.Add(lineRenderer.SelectedRoute);
                        sta.Routes = r.ToArray();
                    }
                    info.Timetable.AddStation(sta, routesDropDown.SelectedRoute);
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
