using Eto.Forms;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtoPoint = Eto.Drawing.Point;

namespace FPLedit.Bildfahrplan.Forms
{
    internal class DynamicPreviewForm : FForm
    {
        private readonly IInfo info;
        private Drawable panel;
        private Scrollable scrollable;
        private Renderer renderer;
        private DropDown routesComboBox;
        private string lastFn = null;
        private int selectedRoute = Timetable.LINEAR_ROUTE_ID;
        private EtoPoint scrollPosition = new EtoPoint(0, 0);

        public DynamicPreviewForm(IInfo info)
        {
            this.info = info;
            info.FileStateChanged += Info_FileStateChanged;

            ShowInTaskbar = true;
            Title = "Dynamische Bildfahrplan-Vorschau";
            Maximizable = false;
            Resizable = false;

            var stackLayout = new StackLayout();
            Content = stackLayout;

            var nStack = new StackLayout
            {
                Orientation = Orientation.Horizontal,
                VerticalContentAlignment = VerticalAlignment.Center,
                Spacing = 5,
                Padding = new Eto.Drawing.Padding(5),
            };
            stackLayout.Items.Add(nStack);

            routesComboBox = new DropDown();
            nStack.Items.Add(routesComboBox);

            routesComboBox.SelectedIndexChanged += (s, e) =>
            {
                if (routesComboBox.SelectedIndex == -1)
                    return;
                selectedRoute = (int)((ListItem)routesComboBox.Items[routesComboBox.SelectedIndex]).Tag;
                renderer = new Renderer(info.Timetable, selectedRoute);
                scrollPosition.Y = 0;
                panel.Height = renderer.GetHeight();
                panel.Invalidate();
            };

            panel = new Drawable
            {
                Height = 0,
                Width = 800
            };
            panel.Paint += Panel_Paint;

            scrollable = new Scrollable
            {
                ExpandContentWidth = false,
                ExpandContentHeight = false,
                Height = 800,
                Content = panel,
            };
            stackLayout.Items.Add(scrollable);

            // Initialisierung der Daten
            ReloadRouteNames(true);
            routesComboBox.SelectedIndex = 0;
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

        private void Info_FileStateChanged(object sender, FileStateChangedEventArgs e)
        {
            ReloadRouteNames(lastFn != e.FileState.FileName);
            lastFn = e.FileState.FileName;

            scrollPosition = scrollable.ScrollPosition;
            panel.Invalidate();
        }

        private void Panel_Paint(object sender, PaintEventArgs e)
        {
            if (renderer == null)
                return;

            renderer.width = panel.Width;

            using (var bmp = new Bitmap(panel.Width, renderer.GetHeight()))
            using (var g = Graphics.FromImage(bmp))
            using (var ms = new MemoryStream())
            {
                renderer.Draw(g);
                bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                using (var eto = new Eto.Drawing.Bitmap(ms.ToArray()))
                    e.Graphics.DrawImage(eto, new Eto.Drawing.PointF(0, 0));
            }

            scrollable.ScrollPosition = scrollPosition;
        }
    }
}
