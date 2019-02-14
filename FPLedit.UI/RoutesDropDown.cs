using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.UI
{
    public class RoutesDropDown : DropDown
    {
        private IInfo info;
        private string lastFn;
        private int selectedRoute;

        public void Initialize(IInfo info)
        {
            this.info = info;

            info.FileStateChanged += (s, e) =>
            {
                ReloadRouteNames(lastFn != e.FileState.FileName);
                lastFn = e.FileState.FileName;
            };

            SelectedIndexChanged += (s, e) =>
            {
                if (SelectedIndex == -1)
                    return;
                selectedRoute = (int)((ListItem)Items[SelectedIndex]).Tag;

                SelectedRouteChanged?.Invoke(this, new EventArgs());
            };

            // Initialisieren der Daten
            ReloadRouteNames(true);
            SelectedIndex = 0;
        }

        public int SelectedRoute
        {
            get => selectedRoute;
            set
            {
                var routes = GetRouteNames(info.Timetable);

                if (!routes.Any(r => (int)r.Tag == value))
                    throw new ArgumentOutOfRangeException($"Route {value} does not exist");

                var rl = routes.ToList();
                SelectedIndex = rl.IndexOf(rl.FirstOrDefault(li => (int)li.Tag == value));
                selectedRoute = value;
            }
        }

        public event EventHandler SelectedRouteChanged;

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
                Items.Clear();
                return;
            }

            int oldSelected = -1;
            if (SelectedValue != null)
                oldSelected = (int)((ListItem)SelectedValue).Tag;
            var routes = GetRouteNames(info.Timetable);
            Items.Clear();
            Items.AddRange(routes);

            if (oldSelected != -1 && !forceReload && routes.Any(r => (int)r.Tag == oldSelected))
            {
                var rl = routes.ToList();
                SelectedIndex = rl.IndexOf(rl.FirstOrDefault(li => (int)li.Tag == oldSelected));
                selectedRoute = oldSelected;
            }
            else
            {
                selectedRoute = 0;
                SelectedIndex = 0;
            }
        }
    }
}
