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
            set { _selectedRoute = value; Invalidate(); }
        }
        private bool _highlightBetween;
        public bool HighlightBetweenStations
        {
            get => _highlightBetween;
            set { _highlightBetween = value; Invalidate(); }
        }

        private List<Station> _highlightedStations = new List<Station>();

        private PointF _pan = new PointF();
        public PointF Pan
        {
            get => _pan;
            set { _pan = value; Invalidate(); }
        }


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
        private readonly Point OFFSET = new Point(OFFSET_X, OFFSET_Y);

        private Station tmp_sta;
        private float tmp_km;
        private Modes mode;

        public LineRenderer()
        {
            layout = new PixelLayout();
            font = new Font(FontFamilies.SansFamilyName, 8);
            linePen = new Pen(Colors.Black, 2.0f);
            handler = new StaPosHandler();

            MouseDown += (s, e) => PlaceStation();
            KeyDown += (s, e) => DispatchKeystroke(e);
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

            e.Graphics.TranslateTransform(_pan);

            if (routes == null || routes.Length == 0)
                return;

            Station lastSta = null;
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

                    var tPen = pen;
                    if (_highlightBetween && _highlightedStations.Contains(sta) && _highlightedStations.Contains(lastSta))
                        tPen = new Pen(Colors.Red, 2); //TODO: Check, ob Stationen wirklich ohne Zwischenstationen sind
                    if (lastP.HasValue)
                        e.Graphics.DrawLine(tPen, x, y, OFFSET_X + lastP.Value.X, OFFSET_Y + lastP.Value.Y);
                    lastP = pos;
                    lastSta = sta;

                    var panelColor = _highlightedStations.Contains(sta) ? Colors.Red : Colors.Gray;
                    DrawArgs args = panels.FirstOrDefault(pa => pa.Station == sta);
                    if (args == null)
                    {
                        args = new DrawArgs(sta, new Point(x - 5, y - 5), new Size(10, 10), panelColor);
                        panels.Add(args);
                        // Wire events
                        switch (mode)
                        {
                            case Modes.Normal: ApplyNormalMode(args, sta); break;
                            case Modes.AddRoute: ApplyAddMode(args, sta); break;
                            case Modes.JoinRoutes: ApplyJoinMode(args, sta); break;
                        }
                    }
                    args.Color = panelColor;
                }
            }

            if (tmp_sta != null && stapos.TryGetValue(tmp_sta, out Point point))
            {
                var x = OFFSET_X + point.X;
                var y = OFFSET_Y + point.Y;

                e.Graphics.DrawLine(linePen, new Point(x, y), mousePosition - _pan);

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
            if (!_pan.IsZero)
            {
                string statusL = "Ansicht verschoben, [R] für Reset";
                var sizeL = g.MeasureString(font, statusL);
                var pointL = new PointF(0, ClientSize.Height - sizeL.Height);
                g.FillRectangle(Brushes.Orange, new RectangleF(pointL, sizeL));
                g.DrawText(font, Brushes.Black, pointL, statusL);
            }

            string statusR = GetStatusString(mode);
            statusR = FixedStatusString ?? statusR;
            var sizeR = g.MeasureString(font, statusR);
            var pointR = new PointF(ClientSize.Width - sizeR.Width, ClientSize.Height - sizeR.Height);
            g.FillRectangle(Brushes.Turquoise, new RectangleF(pointR, sizeR));
            g.DrawText(font, Brushes.Black, pointR, statusR);
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
            mode = Modes.Normal;
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
            mode = Modes.AddRoute;

            Cursor = Cursors.Crosshair;
            this.Focus();
        }

        private void ApplyJoinMode(DrawArgs p, Station sta)
        {
            p.Click += (s, e) => ConnectJoinLines(sta);
        }

        public void StartJoinLines(float km)
        {
            tmp_km = km;
            mode = Modes.JoinRoutes;

            Cursor = Cursors.Crosshair;
            this.Focus();
        }

        private void ConnectJoinLines(Station sta)
        {
            tt.JoinRoutes(SelectedRoute, sta, tmp_km);
            tmp_sta = null;
            mode = Modes.Normal;

            ReloadTimetable();
        }

        public void AbortAddStation()
        {
            if (tmp_sta == null) // Nicht benötigt
                return;

            tmp_sta = null;
            Cursor = Cursors.Default;
            mode = Modes.Normal;

            Invalidate();
        }

        private void PlaceStation()
        {
            if (tmp_sta == null || stapos.TryGetValue(tmp_sta, out var _))
                return;

            Cursor = Cursors.Default;
            var point = mousePosition - OFFSET - _pan;
            stapos[tmp_sta] = new Point(point);

            Invalidate();
        }
        #endregion

        private string GetStatusString(Modes mode)
        {
            switch (mode)
            {
                case Modes.Normal: return "Streckennetz bearbeiten";
                case Modes.AddRoute: return "Klicken, um Station hinzuzufügen und diese mit einer bestehenden Station zu verbinden; ESC zum Abbrechen";
                case Modes.JoinRoutes: return "Klicken, um die Zielstation der Verbindung auzuwählen; ESC zum Abbrechen";
                default: return "";
            }
        }

        public void DispatchKeystroke(KeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                AbortAddStation();
                e.Handled = true;
            }
            if (e.Key == Keys.R)
            {
                Pan = new PointF();
                e.Handled = true;
            }
        }

        #region Drag'n'Drop
        private DrawArgs draggedControl;
        private bool hasDragged = false;
        private const int CLICK_TIME = 1000000; //0.1s
        private long lastClick = 0;
        private bool lastDoubleClick;
        private bool hasPanned = false;
        private PointF originalLocation;
        private PointF originalPan;

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            draggedControl = null;
            Cursor = Cursors.Default;
            hasDragged = false;

            foreach (var args in panels.ToArray())
                args.HandleDoubleClick(new Point(e.Location), new Point(_pan));

            lastDoubleClick = true;
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            lastClick = DateTime.Now.Ticks;

            if (!lastDoubleClick)
            {
                if (e.Buttons == MouseButtons.Alternate)
                    foreach (var args in panels.ToArray())
                        args.HandleRightClick(new Point(e.Location), new Point(_pan));
                else if (e.Buttons == MouseButtons.Primary && StationMovingEnabled && IsNetwork)
                {
                    foreach (var args in panels.ToArray())
                    {
                        if (args.Rect.Contains(new Point(e.Location) - new Point(_pan)))
                        {
                            draggedControl = args;
                            Cursor = Cursors.Move;
                        }
                    }
                }

                if (e.Buttons == MouseButtons.Primary && draggedControl == null)
                {
                    hasPanned = true;
                    originalPan = _pan;
                    originalLocation = e.Location;
                    Cursor = Cursors.Move;
                }
            }

            lastDoubleClick = false;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            mousePosition = e.Location;
            if (StationMovingEnabled && IsNetwork && draggedControl != null)
            {
                var p = new Point(e.Location);

                // Not getting out of bounds
                if (p.X < 0)
                    p.X = 0;
                if (p.Y < 0)
                    p.Y = 0;
                if (p.X > ClientSize.Width)
                    p.X = ClientSize.Width;
                if (p.Y > ClientSize.Height)
                    p.Y = ClientSize.Height;

                draggedControl.Location = p;
                stapos[draggedControl.Station] = p - OFFSET - new Point(_pan);
                hasDragged = true;
                Invalidate();
            }
            if (hasPanned)
            {
                _pan = originalPan + (e.Location - originalLocation);
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
                    args.HandleClick(new Point(e.Location), new Point(_pan));
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

            if (hasPanned)
            {
                hasPanned = false;
                Cursor = Cursors.Default;
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

        public void ClearHighlight()
        {
            _highlightedStations.Clear();
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

            public void HandleClick(Point clickPosition, Point pan)
            {
                if (Rect.Contains(clickPosition - pan))
                    Click?.Invoke(this, new EventArgs());
            }

            public void HandleRightClick(Point clickPosition, Point pan)
            {
                if (Rect.Contains(clickPosition - pan))
                    RightClick?.Invoke(this, new EventArgs());
            }

            public void HandleDoubleClick(Point clickPosition, Point pan)
            {
                if (Rect.Contains(clickPosition - pan))
                    DoubleClick?.Invoke(this, new EventArgs());
            }

            public void Draw(Graphics g)
                => g.FillRectangle(Color, Rect);
        }

        private enum Modes
        {
            Normal,
            AddRoute,
            JoinRoutes,
        }
    }
}
