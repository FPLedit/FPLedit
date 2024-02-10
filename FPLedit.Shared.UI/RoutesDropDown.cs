using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.UI;

public sealed class RoutesDropDown : DropDown
{
    private IPluginInterface? pluginInterface;
    private string? lastFn;
    private int selectedRoute;
    private bool enableVirtualRoutes;

    public void Initialize(IPluginInterface pi)
    {
        pluginInterface = pi;

        pluginInterface.FileStateChanged += (_, e) =>
        {
            if (IsDisposed)
                return;

            ReloadRouteNames(lastFn != e.FileState.FileName);
            lastFn = e.FileState.FileName;
        };

        SelectedIndexChanged += (_, _) =>
        {
            if (SelectedIndex == -1)
                return;
            selectedRoute = (int)((ListItem)Items[SelectedIndex]).Tag;

            SelectedRouteChanged?.Invoke(this, EventArgs.Empty);
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
            var routes = GetRouteNames(pluginInterface!.Timetable).ToList();

            if (routes.All(r => (int) r.Tag != value))
                throw new ArgumentOutOfRangeException($"Route {value} does not exist");

            SelectedIndex = routes.IndexOf(routes.FirstOrDefault(li => (int)li.Tag == value)!);
            selectedRoute = value;
        }
    }

    public bool EnableVirtualRoutes
    {
        get => enableVirtualRoutes;
        set
        {
            enableVirtualRoutes = value;
            ReloadRouteNames(true);
        }
    }

    public event EventHandler? SelectedRouteChanged;

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
            if (EnableVirtualRoutes)
            {
                var vrt = VirtualRoute.GetVRoutes(tt);
                foreach (var v in vrt)
                    yield return BuildItem(v.GetRouteName(), v.Index);
            }
        }
        else
            yield return BuildItem(T._("<Standard>"), Timetable.LINEAR_ROUTE_ID);
    }

    private void ReloadRouteNames(bool forceReload)
    {
        if (pluginInterface == null) // This might be the case if we are setting config before calling Initialize().
            return;
        if (pluginInterface.TimetableMaybeNull == null)
        {
            Items.Clear();
            return;
        }

        int oldSelected = -1;
        if (SelectedValue != null)
            oldSelected = (int)((ListItem)SelectedValue).Tag;
        var routes = GetRouteNames(pluginInterface.Timetable).ToList();
        Items.Clear();
        Items.AddRange(routes);

        if (oldSelected != -1 && !forceReload && routes.Any(r => (int)r.Tag == oldSelected))
        {
            var rl = routes.ToList();
            SelectedIndex = rl.IndexOf(rl.FirstOrDefault(li => (int)li.Tag == oldSelected)!);
            selectedRoute = oldSelected;
        }
        else
        {
            selectedRoute = 0;
            SelectedIndex = 0;
        }
    }
}