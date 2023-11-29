using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.UI.Network;

public sealed class NetworkRenderer : Drawable
{
    private PointF mousePosition = PointF.Empty;

    private Timetable? tt;
    private readonly List<RenderBtn<Station>> panels = new ();
    private readonly Font? font;
    private readonly Pen linePen, highlightPen, borderPen;
    private readonly Color systemBgColor, systemTextColor;

    public static readonly Keys[] DispatchableKeys = { Keys.R, Keys.S, Keys.Escape, Keys.Equal /* Plus */, Keys.Minus, Keys.Add, Keys.Subtract };

    protected override void Dispose(bool disposing)
    {
        if (font is { IsDisposed: false })
            font.Dispose();
        linePen.Dispose();
        highlightPen.Dispose();
        borderPen.Dispose();
    }

    private bool IsNetwork => tt?.Type == TimetableType.Network;
    
    private bool setPanCenterEnabled = true;
    public bool SetPanCenterEnabled
    {
        get => setPanCenterEnabled;
        set { setPanCenterEnabled = value; Invalidate(); }
    }

    private bool stationMovingEnabled = true;
    public bool StationMovingEnabled
    {
        get => stationMovingEnabled;
        set { stationMovingEnabled = value; Invalidate(); }
    }
    private string? fixedStatusString;
    public string? FixedStatusString
    {
        get => fixedStatusString;
        set { fixedStatusString = value; Invalidate(); }
    }
    private bool disableTopBorder;
    public bool DisableTopBorder
    {
        get => disableTopBorder;
        set { disableTopBorder = value; Invalidate(); }
    }
    private int selectedRoute = Timetable.LINEAR_ROUTE_ID;
    public int SelectedRoute
    {
        get => selectedRoute;
        set { selectedRoute = value; Invalidate(); }
    }
    private bool highlightBetween;
    public bool HighlightBetweenStations
    {
        get => highlightBetween;
        set { highlightBetween = value; Invalidate(); }
    }

    private PathData? highlightedPath;

    public void SetHighlightedPath(IEnumerable<Station>? stations)
    {
        var stas = stations ?? Array.Empty<Station>();
        highlightedPath = new PathData(tt!, stas);
        Invalidate();
    }

    private PointF pan = PointF.Empty;
    public PointF Pan
    {
        get => pan;
        set { pan = value; Invalidate(); }
    }

    private float zoom = 1;
    public float Zoom
    {
        get => zoom;
        set
        {
            zoom = Math.Clamp(value, 0.5f, 2f);
            Invalidate();
        }
    }

    private readonly StationCanvasPositionHandler handler;
    private Dictionary<Station, Point>? stapos;
    private Route[]? routes;

    public event EventHandler? StationClicked;
    public event EventHandler? StationRightClicked;
    public event EventHandler? StationDoubleClicked;
    public event EventHandler<EventArgs<int>>? RoutesChanged;
    public event EventHandler? StationMoveEnd;

    private const int OFFSET_X = 20;
    private const int OFFSET_Y = 50;
    // ReSharper disable once InconsistentNaming
    private readonly Point OFFSET = new (OFFSET_X, OFFSET_Y);

    private Station? modeTempSta;
    private float modeTempKm;
    private Modes mode;

    public NetworkRenderer()
    {
        systemBgColor = SystemColors.ControlBackground;
        systemTextColor = SystemColors.ControlText;

        font = new Font(FontFamilies.SansFamilyName, 8);
        linePen = new Pen(systemTextColor, 2f);
        highlightPen = new Pen(Colors.Red, 2f);
        borderPen = new Pen(Brushes.Black) { DashStyle = new DashStyle(0, new[] { 2f, 2f, 2f, 2f }) };
        handler = new StationCanvasPositionHandler();

        MouseDown += (_, _) => PlaceStation();
        KeyDown += (_, e) => DispatchKeystroke(e);
    }

    public void SetTimetable(Timetable? newTt)
    {
        tt = newTt;
        routes = tt?.GetRoutes();
        if (tt != null)
        {
            if (tt.Type == TimetableType.Linear)
                stapos = handler.GenerateLinearPoints(tt, ClientSize.Width);
            else
                stapos = handler.LoadNetworkPoints(tt);
        }

        highlightedPath = PathData.Empty(tt!);

        Invalidate();
    }

    private void ReloadTimetable()
    {
        SetTimetable(tt);
        Invalidate();
    }

    #region Drawing
    protected override void OnPaint(PaintEventArgs e)
    {
        SuspendLayout();
        // Reset
        e.Graphics.Clear(systemBgColor);
        panels.Clear();

        e.Graphics.SaveTransform();
        e.Graphics.TranslateTransform(pan);
        e.Graphics.ScaleTransform(zoom);
        DrawNetwork(e.Graphics, - pan * 1/zoom, (- pan + Bounds.Size) * 1/zoom);
        e.Graphics.RestoreTransform();
            
        // Draw Status strings & border on top of network
        DrawStatus(e.Graphics);
        DrawBorder(e.Graphics);

        ResumeLayout();
        base.OnPaint(e);
    }

    private void DrawNetwork(Graphics g, PointF leftTop, PointF bottomRight)
    {
        if (routes == null || routes.Length == 0)
            return;

        Rectangle rec = new Rectangle((Point)leftTop, (Point)bottomRight);
        var btnSize = new Size(10, 10);

        Station? lastSta = null;
        foreach (var r in routes)
        {
            Point? lastP = null;
            foreach (var sta in r.Stations)
            {
                var pos = stapos![sta];
                var p = OFFSET + pos;
                var x = OFFSET_X + pos.X;
                var y = OFFSET_Y + pos.Y;
                bool doRender = rec.Contains(x, y);

                // Render text only when it is (reasonably) inside viewport
                if (doRender || rec.Intersects(new Rectangle(p, p + new Point(500, 500))))
                {
                    var text = sta.SName + " (";
                    foreach (var ri in sta.Routes)
                    {
                        var km = sta.Positions.GetPosition(ri)!.Value.ToString("0.0");
                        if (ri == SelectedRoute && sta.Routes.Length > 1)
                            km = "▶" + km;
                        text += km + "|";
                    }

                    text = text[..^1] + ")";
                        
                    g.SaveTransform();
                    g.TranslateTransform(x + 6, y + 7);
                    g.RotateTransform(60);
                    g.DrawText(font, systemTextColor, new Point(0, 0), text);
                    g.RestoreTransform();
                }

                if (lastP.HasValue && lastSta != null && (rec.Intersects(new Rectangle(OFFSET + lastP.Value, p)) || rec.Intersects(new Rectangle(p, OFFSET + lastP.Value))))
                {
                    var tPen = GetLinePen(r.Index, sta, lastSta);
                    g.DrawLine(tPen, p, OFFSET + lastP.Value);
                }

                lastP = pos;
                lastSta = sta;

                if (!doRender) continue;
                    
                var panelColor = highlightedPath!.ContainsStation(sta) ? Colors.Red : Colors.Gray;
                RenderBtn<Station>? btn = panels.FirstOrDefault(pa => pa.Tag == sta);
                if (btn == null)
                {
                    btn = new RenderBtn<Station>(sta, new Point(x, y) - btnSize / 2, btnSize, panelColor);
                    panels.Add(btn);
                    // Wire events
                    switch (mode)
                    {
                        case Modes.Normal: ApplyNormalMode(btn, sta); break;
                        case Modes.AddRoute: ApplyAddMode(btn, sta); break;
                        case Modes.JoinRoutes: ApplyJoinMode(btn, sta); break;
                        case Modes.BreakRoute: ApplyBreakMode(btn, sta); break;
                        default: throw new NotImplementedException(nameof(NetworkRenderer) + " encountered an unsupported mode!");
                    }
                }
                btn.BackgroundColor = panelColor;
            }
        }

        if (modeTempSta != null && ModeCanConnectStation() && stapos!.TryGetValue(modeTempSta, out var point))
        {
            var p = OFFSET + point;
            g.DrawLine(linePen, p, (mousePosition - pan) * (1 / zoom));

            var args = new RenderBtn<Station>(modeTempSta, p - btnSize / 2, btnSize, Colors.DarkCyan);
            panels.Add(args);
        }

        foreach (var args in panels)
            args.Draw(g);
    }

    private void DrawStatus(Graphics g)
    {
        if (!pan.IsZero || Math.Abs(zoom - 1) > 0.01f)
        {
            var statusL = T._("Ansicht verschoben, [R] für Reset");
            if (SetPanCenterEnabled && IsNetwork && !pan.IsZero)
                statusL += T._(", [S] zum Speichern");
            var sizeL = g.MeasureString(font, statusL);
            var pointL = new PointF(0, ClientSize.Height - sizeL.Height);
            g.FillRectangle(Brushes.Orange, new RectangleF(pointL, sizeL));
            g.DrawText(font, Brushes.Black, pointL, statusL);
        }

        var statusR = GetStatusString(mode);
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
        g.DrawLine(borderPen, Point.Empty, new Point(ClientSize.Width, 0));
    }

    private Pen GetLinePen(int route, Station sta, Station lastSta)
    {
        if (route == SelectedRoute || (highlightBetween && highlightedPath!.IsDirectlyConnected(sta, lastSta)))
            return highlightPen;
        return linePen;
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        Invalidate();
        base.OnSizeChanged(e);
    }
    #endregion

    #region Normal Mode
    private void ApplyNormalMode(RenderBtn<Station> p, Station sta)
    {
        p.DoubleClick += (_, _) => StationDoubleClicked?.Invoke(sta, EventArgs.Empty);
        p.Click += (_, _) => StationClicked?.Invoke(sta, EventArgs.Empty);
        p.RightClick += (_, _) => StationRightClicked?.Invoke(sta, EventArgs.Empty);
    }
    #endregion

    private void ResetToNormalMode()
    {
        modeTempSta = null;
        Cursor = Cursors.Default;
        mode = Modes.Normal;

        Invalidate();
    }

    #region Add Mode
    private void ApplyAddMode(RenderBtn<Station> p, Station sta)
    {
        p.Click += (_, _) => ConnectAddStation(sta);
    }

    private void ConnectAddStation(Station sta)
    {
        var rtIdx = tt!.AddRoute(sta, modeTempSta!, 0, modeTempKm);
        handler.WriteStapos(tt, stapos!);
        ResetToNormalMode();

        RoutesChanged?.Invoke(this, new EventArgs<int>(rtIdx));
        ReloadTimetable();
    }

    public void StartAddStation(Station rawSta, float km)
    {
        modeTempSta = rawSta;
        modeTempKm = km;
        mode = Modes.AddRoute;

        Cursor = Cursors.Crosshair;
        Focus();
        Invalidate();
    }

    private void PlaceStation()
    {
        if (!ModeCanConnectStation())
            return;
        if (modeTempSta == null || stapos!.TryGetValue(modeTempSta, out _))
            return;

        Cursor = Cursors.Default;
        var point = new Point((mousePosition - pan) * (1 / zoom)) - OFFSET;
        stapos[modeTempSta] = new Point(point);

        Invalidate();
    }
    #endregion

    #region Join Mode
    private void ApplyJoinMode(RenderBtn<Station> p, Station sta)
    {
        p.Click += (_, _) => ConnectJoinLines(sta);
    }

    public void StartJoinLines(float km)
    {
        modeTempKm = km;
        mode = Modes.JoinRoutes;

        Cursor = Cursors.Crosshair;
        Focus();
        Invalidate();
    }

    private void ConnectJoinLines(Station sta)
    {
        if (sta.Routes.Contains(SelectedRoute))
            MessageBox.Show(T._("Die Verbindung konnte nicht erstellt werden, da die gewählte Zielstation sich bereits auf der gleichen Strecke befindet!"));
        else if (!tt!.JoinRoutes(SelectedRoute, sta, modeTempKm))
            MessageBox.Show(T._("Die Verbindung konnte nicht erstellt werden, da sonst Routen zusammenfallen würden!"));

        ResetToNormalMode();
        ReloadTimetable();
    }
    #endregion

    #region BreakRoute
    private void ApplyBreakMode(RenderBtn<Station> p, Station sta)
    {
        p.Click += (_, _) => PerformBreakLine(sta);
    }

    public void StartBreakLine(Station origin)
    {
        modeTempSta = origin;
        mode = Modes.BreakRoute;

        Cursor = Cursors.Crosshair;
        Focus();
        Invalidate();
    }

    private void PerformBreakLine(Station target)
    {
        // First do a dry-run on a fresh copy of the current timetable.
        var shouldTryBreak = false;
        try
        {
            var testTt = tt!.Clone();
            var testModeTempSta = testTt.Stations.First(s => s.Id == modeTempSta!.Id);
            var testTarget = testTt.Stations.First(s => s.Id == target.Id);
            var testBreakResult = testTt.BreakRouteUnsafe(testModeTempSta, testTarget);
            if (!testBreakResult.success)
                MessageBox.Show(T._("Strecken-Trennen nicht möglich: {0}", testBreakResult.failReason ?? ""), "FPLedit", MessageBoxType.Error);

            shouldTryBreak = testBreakResult.success;
        }
        catch (Exception e)
        {
            MessageBox.Show(T._("Strecken-Trennen nicht möglich: {0}", e.Message), "FPLedit", MessageBoxType.Error);
        }

        if (shouldTryBreak)
        {
            var breakResult = tt!.BreakRouteUnsafe(modeTempSta!, target);
            if (!breakResult.success)
            {
                // still something went wrong?
                var message = T._("Fehler beim Strecken-Trennen: {0}", breakResult.failReason ?? "");
                if (!(breakResult.isSafeFailure ?? false))
                    message += "\n\n" + T._("WARNUNG: Potentiell befindet sich die Fahrplandatei jetzt in einem inkonsistenten Zustand!");
                MessageBox.Show(message, "FPLedit", MessageBoxType.Error);
            }
            if (breakResult.routeToReload.HasValue)
                RoutesChanged?.Invoke(this, new EventArgs<int>(breakResult.routeToReload.Value));
        }

        ResetToNormalMode();
        ReloadTimetable();
    }
    #endregion

    private string GetStatusString(Modes m)
    {
        return m switch
        {
            Modes.Normal => T._("Streckennetz bearbeiten"),
            Modes.AddRoute => T._("Klicken, um Station hinzuzufügen und diese mit einer bestehenden Station zu verbinden") + "; " + T._("[ESC] zum Abbrechen"),
            Modes.JoinRoutes => T._("Klicken, um die Zielstation der Verbindung auszuwählen") + "; " + T._("[ESC] zum Abbrechen"),
            Modes.BreakRoute => T._("Klicken, um die Nachbarstation auf der zu trennenden Strecke auszuwählen") + "; " + T._("[ESC] zum Abbrechen"),
            _ => throw new NotImplementedException(nameof(NetworkRenderer) + " encountered an unsupported mode!"),
        };
    }

    public void DispatchKeystroke(KeyEventArgs e)
    {
        if (e.Modifiers != Keys.None)
            return;
        switch (e.Key) // See DispatchableKeys
        {
            case Keys.Escape:
                ResetToNormalMode();
                e.Handled = true;
                break;
            case Keys.R:
                Pan = PointF.Empty;
                Zoom = 1f;
                e.Handled = true;
                break;
            case Keys.S:
                if (SetPanCenterEnabled && tt != null && stapos != null && IsNetwork)
                {
                    e.Handled = true;
                    var keys = stapos.Keys.ToArray();
                    foreach (var sta in keys)
                        stapos[sta] += (Point)(Pan * 1/zoom);
                    handler.WriteStapos(tt, stapos);
                    Pan = PointF.Empty;
                }
                break;
            case Keys.Equal: // Plus key
            case Keys.Add:
                Zoom += 0.1f;
                break;
            case Keys.Minus:
            case Keys.Subtract:
                Zoom -= 0.1f;
                break;
        }
    }

    #region Drag'n'Drop
    private RenderBtn<Station>? draggedControl;
    private bool hasDragged = false;
    private const int CLICK_TIME = 10^6; // 0.1*10^7s, 1 tick = 10^-7 seconds
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
        hasPanned = false;

        foreach (var args in panels.ToArray())
            args.HandleDoubleClick(new Point(e.Location), new Point(pan), zoom);

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
                    args.HandleRightClick(new Point(e.Location), new Point(pan), zoom);
            else if (e.Buttons == MouseButtons.Primary && StationMovingEnabled && IsNetwork)
            {
                foreach (var args in panels.ToArray())
                {
                    if (args.Rect.Contains(new Point((new Point(e.Location) - new Point(pan)) * (1/zoom))))
                    {
                        draggedControl = args;
                        Cursor = Cursors.Move;
                    }
                }
            }

            if (e.Buttons == MouseButtons.Primary && draggedControl == null && tt != null) // Only pan if we have data & we don't drag anything
            {
                hasPanned = true;
                originalPan = pan;
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
            stapos![draggedControl.Tag] = new Point((p - pan) * (1 / zoom)) - OFFSET;
            hasDragged = true;
            Invalidate();
        }
        if (hasPanned)
        {
            pan = originalPan + (e.Location - originalLocation) * (1 / zoom);
            Invalidate();
        }
        if (modeTempSta != null && ModeCanConnectStation())
            Invalidate();
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        if (e.Buttons == MouseButtons.Primary && DateTime.Now.Ticks >= lastClick + CLICK_TIME)
        {
            foreach (var args in panels.ToArray())
                args.HandleClick(new Point(e.Location), new Point(pan), zoom);
        }

        if (StationMovingEnabled && IsNetwork && draggedControl != null)
        {
            draggedControl = null;
            Cursor = Cursors.Default;
            handler.WriteStapos(tt!, stapos!);
            if (hasDragged)
            {
                Invalidate();
                StationMoveEnd?.Invoke(this, EventArgs.Empty);
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

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        Zoom += e.Delta.Height * 0.1f;
        base.OnMouseWheel(e);
    }

    #endregion

    private enum Modes
    {
        Normal,
        AddRoute,
        JoinRoutes,
        BreakRoute,
    }

    private bool ModeCanConnectStation() => mode is Modes.AddRoute or Modes.JoinRoutes;
}