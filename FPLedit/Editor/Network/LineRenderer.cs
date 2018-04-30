using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Network
{
    public class LineRenderer : Drawable
    {
        private PixelLayout layout;
        private PointF mousePosition = new PointF();

        private Timetable tt;
        private List<DrawArgs> panels = new List<DrawArgs>();
        private Font font;
        private Pen linePen;

        private bool IsNetwork => tt?.Type == TimetableType.Network;

        private bool _stationMovingEnabled = true;
        public bool StationMovingEnabled
        {
            get => _stationMovingEnabled;
            set { _stationMovingEnabled = value; Invalidate(); }
        }
        private string _fixedStatusString = null;
        public string FixedStatusString
        {
            get => _fixedStatusString;
            set { _fixedStatusString = value; Invalidate(); }
        }
        private bool _disableTopBorder = false;
        public bool DisableTopBorder
        {
            get => _disableTopBorder;
            set { _disableTopBorder = value; Invalidate(); }
        }
        private int _selectedRoute = Timetable.LINEAR_ROUTE_ID;
        public int SelectedRoute
        {
            get => _selectedRoute;
            set { _selectedRoute = value; this.Invalidate(); }
        }
        private List<Station> _highlightedStations = new List<Station>();


        private StaPosHandler handler;
        private Dictionary<Station, Point> stapos;
        private Route[] routes;

        public event EventHandler StationClicked;
        public event EventHandler StationRightClicked;
        public event EventHandler StationDoubleClicked;
        public event EventHandler NewRouteAdded;
        public event EventHandler StationMoveEnd;

        private const int OFFSET_X = 20;
        private const int OFFSET_Y = 50;

        private Station tmp_sta;
        private float tmp_km;
        private bool addMode => tmp_sta != null;

        public LineRenderer()
        {
            layout = new PixelLayout();
            font = new Font(FontFamilies.SansFamilyName, 8);
            linePen = new Pen(Colors.Black, 2.0f);
            handler = new StaPosHandler();

            MouseDown += (s, e) => PlaceStation();
        }

        public void SetTimetable(Timetable tt)
        {
            this.tt = tt;
            routes = tt?.GetRoutes();
            if (tt != null)
            {
                if (tt.Type == TimetableType.Linear)
                    stapos = handler.GenerateLinearPoints(tt, ClientSize.Width);
                else
                    stapos = handler.LoadNetworkPoints(tt);
            }

            this.Invalidate();
        }

        public void ReloadTimetable()
        {
            this.SetTimetable(tt);
            this.Invalidate();
        }

        #region Drawing
        protected override void OnPaint(PaintEventArgs e)
        {
            this.SuspendLayout();
            // Reset
            e.Graphics.Clear(Colors.White);
            panels.Clear();

            DrawStatus(e.Graphics);
            DrawBorder(e.Graphics);

            if (routes == null || routes.Length == 0)
                return;

            foreach (var r in routes)
            {
                var pen = linePen;
                if (r.Index == SelectedRoute)
                    pen = new Pen(Colors.Red, 2);
                Point? lastP = null;
                foreach (var sta in r.GetOrderedStations())
                {
                    var pos = stapos[sta];
                    var x = OFFSET_X + pos.X;
                    var y = OFFSET_Y + pos.Y;

                    e.Graphics.SaveTransform();
                    e.Graphics.TranslateTransform(x + 6, y + 7);
                    e.Graphics.RotateTransform(60);

                    var text = sta.SName + " (";
                    foreach (var ri in sta.Routes)
                    {
                        var km = sta.Positions.GetPosition(ri).Value.ToString("0.0");
                        if (ri == SelectedRoute && sta.Routes.Length > 1)
                            km = "▶" + km;
                        text += km + "|";
                    }
                    text = text.Substring(0, text.Length - 1) + ")";
                    e.Graphics.DrawText(font, Brushes.Black, new Point(0, 0), text);

                    e.Graphics.RestoreTransform();

                    if (lastP.HasValue)
                        e.Graphics.DrawLine(pen, x, y, OFFSET_X + lastP.Value.X, OFFSET_Y + lastP.Value.Y);
                    lastP = pos;

                    var panelColor = _highlightedStations.Contains(sta) ? Colors.Red : Colors.Gray;
                    DrawArgs args = panels.FirstOrDefault(pa => pa.Station == sta);
                    if (args == null)
                    {
                        args = new DrawArgs(sta, new Point(x - 5, y - 5), new Size(10, 10), panelColor);
                        panels.Add(args);
                        // Wire events
                        if (!addMode) ApplyNormalMode(args, sta);
                        else ApplyAddMode(args, sta);
                    }
                    args.Color = panelColor;
                }
            }

            if (tmp_sta != null && stapos.TryGetValue(tmp_sta, out Point point))
            {
                var x = OFFSET_X + point.X;
                var y = OFFSET_Y + point.Y;

                e.Graphics.DrawLine(linePen, new Point(x, y), mousePosition);

                DrawArgs args = new DrawArgs(tmp_sta, new Point(x - 5, y - 5), new Size(10, 10), Colors.DarkCyan);
                panels.Add(args);
            }

            foreach (var args in panels)
                args.Draw(e.Graphics);

            this.ResumeLayout();
            base.OnPaint(e);
        }

        private void DrawStatus(Graphics g)
        {
            string status = addMode ? "Klicken, um Station hinzuzufügen und diese mit einer bestehenden Station zu verbinden; ESC zum Abbrechen" : "Streckennetz bearbeiten";
            status = FixedStatusString ?? status;
            var size = g.MeasureString(font, status);
            var point = new PointF(ClientSize.Width - size.Width, ClientSize.Height - size.Height);
            g.FillRectangle(Brushes.Turquoise, new RectangleF(point, size));
            g.DrawText(font, Brushes.Black, point, status);
        }

        private void DrawBorder(Graphics g)
        {
            if (DisableTopBorder)
                return;
            var pen = new Pen(Brushes.Black) { DashStyle = DashStyle.Parse("2,2,2,2") };
            g.DrawLine(pen, Point.Empty, new Point(ClientSize.Width, 0));
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            this.Invalidate();
            base.OnSizeChanged(e);
        }
        #endregion

        #region Normal Mode
        private void ApplyNormalMode(DrawArgs p, Station sta)
        {
            p.DoubleClick += (s, e) => StationDoubleClicked?.Invoke(sta, new EventArgs());
            p.Click += (s, e) => StationClicked?.Invoke(sta, new EventArgs());
            p.RightClick += (s, e) => StationRightClicked?.Invoke(sta, new EventArgs());
        }
        #endregion

        #region AddMode
        private void ApplyAddMode(DrawArgs p, Station sta)
        {
            p.Click += (s, e) => ConnectAddStation(sta);
        }

        private void ConnectAddStation(Station sta)
        {
            var rtIdx = tt.AddRoute(sta, tmp_sta, 0, tmp_km);
            tt.AddStation(tmp_sta, rtIdx);
            handler.WriteStapos(tt, stapos);
            tmp_sta = null;

            NewRouteAdded?.Invoke(this, new EventArgs());
            ReloadTimetable();
        }

        public void StartAddStation(Station rawSta, float km)
        {
            tmp_sta = rawSta;
            tmp_km = km;

            Cursor = Cursors.Crosshair;
            this.Focus();
        }

        public void AbortAddStation()
        {
            if (tmp_sta == null) // Nicht benötigt
                return;

            tmp_sta = null;
            Cursor = Cursors.Default;

            Invalidate();
        }

        private void PlaceStation()
        {
            if (tmp_sta == null || stapos.TryGetValue(tmp_sta, out Point p))
                return;

            Cursor = Cursors.Default;
            var point = mousePosition;
            point.X -= OFFSET_X;
            point.Y -= OFFSET_Y;
            stapos[tmp_sta] = new Point(point);

            Invalidate();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                AbortAddStation();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
        #endregion

        #region Drag'n'Drop
        private DrawArgs draggedControl;
        private bool hasDragged = false;
        private const int CLICK_TIME = 1000000; //0.1s
        private long lastClick = 0;

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            foreach (var args in panels.ToArray())
                args.HandleDoubleClick(new Point(e.Location));
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            lastClick = DateTime.Now.Ticks;

            if (e.Buttons == MouseButtons.Alternate)
                foreach (var args in panels.ToArray())
                    args.HandleRightClick(new Point(e.Location));
            else if (e.Buttons == MouseButtons.Primary && StationMovingEnabled && IsNetwork)
            {
                foreach (var args in panels.ToArray())
                {
                    if (args.Rect.Contains(new Point(e.Location)))
                    {
                        draggedControl = args;
                        Cursor = Cursors.Move;
                    }
                }

            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            mousePosition = e.Location;
            if (StationMovingEnabled && IsNetwork && draggedControl != null)
            {
                var p = new Point(e.Location);

                //Not getting out of bounds
                if (p.X < 0)
                    p.X = 0;
                if (p.Y < 0)
                    p.Y = 0;
                if (p.X > ClientSize.Width)
                    p.X = ClientSize.Width;
                if (p.Y > ClientSize.Height)
                    p.Y = ClientSize.Height;

                draggedControl.Location = p;
                stapos[draggedControl.Station] = new Point(p.X - OFFSET_X, p.Y - OFFSET_Y);
                hasDragged = true;
                Invalidate();
            }
            if (tmp_sta != null)
                Invalidate();
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (DateTime.Now.Ticks <= lastClick + CLICK_TIME)
            {
                foreach (var args in panels.ToArray())
                    args.HandleClick(new Point(e.Location));
            }

            if (StationMovingEnabled && IsNetwork && draggedControl != null)
            {
                draggedControl = null;
                Cursor = Cursors.Default;
                handler.WriteStapos(tt, stapos);
                if (hasDragged)
                {
                    Invalidate();
                    StationMoveEnd?.Invoke(this, new EventArgs());
                }
                hasDragged = false;
            }

            lastClick = 0;
            base.OnMouseUp(e);
        }
        #endregion

        #region Highlight
        public void AddHighlight(IEnumerable<Station> stations)
        {
            _highlightedStations.AddRange(stations);
            Invalidate();
        }

        public void AddHighlight(Station station)
        {
            _highlightedStations.Add(station);
            Invalidate();
        }

        public void RemoveHighlight(Station station)
        {
            _highlightedStations.Remove(station);
            Invalidate();
        }
        #endregion

        private class DrawArgs
        {
            public Station Station { get; set; }

            public Point Location { get; set; }

            public Size Size { get; set; }

            public Rectangle Rect => new Rectangle(Location, Size);

            public Color Color { get; set; }

            public event EventHandler Click;

            public event EventHandler RightClick;

            public event EventHandler DoubleClick;

            public DrawArgs(Station sta, Point loc, Size size, Color c)
            {
                Station = sta;
                Location = loc;
                Size = size;
                Color = c;
            }

            public void HandleClick(Point clickPosition)
            {
                if (Rect.Contains(clickPosition))
                    Click?.Invoke(this, new EventArgs());
            }

            public void HandleRightClick(Point clickPosition)
            {
                if (Rect.Contains(clickPosition))
                    RightClick?.Invoke(this, new EventArgs());
            }

            public void HandleDoubleClick(Point clickPosition)
            {
                if (Rect.Contains(clickPosition))
                    DoubleClick?.Invoke(this, new EventArgs());
            }

            public void Draw(Graphics g)
                => g.FillRectangle(Color, Rect);
        }
    }
}
