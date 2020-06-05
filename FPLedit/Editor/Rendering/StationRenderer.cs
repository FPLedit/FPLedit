using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace FPLedit.Editor.Rendering
{
    internal sealed class StationRenderer : Drawable
    {
        private const int INDENT = 20;
        private const int LINE_HEIGHT = 30;
        private readonly Font font = new Font(FontFamilies.SansFamilyName, 8);
        private readonly Pen dashedPen = new Pen(Colors.Black, 1) { DashStyle = DashStyles.Dash };
        private readonly Color textColor, bgColor;

        private readonly List<RenderBtn<Track>> buttons = new List<RenderBtn<Track>>();

        private RenderBtn<Track> editingButton;

        // Injected by InitializeWithStation.
        private int routeIndex;
        private Station station;
        private Timetable tt;

        #region Result properties
        
        public IRouteValueCollection<string> DefaultTrackLeft { get; private set; }

        public IRouteValueCollection<string> DefaultTrackRight { get; private set; }

        public ObservableCollection<Track> Tracks { get; private set; }

        public Dictionary<string, string> TrackRenames { get; } = new Dictionary<string, string>();
        
        private readonly List<string> trackRemoves = new List<string>();
        public IEnumerable<string> TrackRemoves => trackRemoves.AsReadOnly();
        
        #endregion

        public StationRenderer()
        {
            textColor = SystemColors.ControlText;
            bgColor = SystemColors.ControlBackground;
        }
        
        public void InitializeWithStation(int route, Station value)
        {
            routeIndex = route;
            station = value;

            tt = value._parent;

            DefaultTrackLeft = value.DefaultTrackLeft.ToStandalone();
            DefaultTrackRight = value.DefaultTrackLeft.ToStandalone();
            Tracks = new ObservableCollection<Track>(value.Tracks.Select(t => t.Copy()));

            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Reset
            e.Graphics.Clear(bgColor);
            buttons.Clear();

            int midx = Width / 2;
            
            // Richtungsangaben ermitteln
            var route = station._parent.GetRoute(routeIndex).Stations;
            var staIdx = route.IndexOf(station);
            var prev = route.ElementAtOrDefault(staIdx - 1);
            var next = route.ElementAtOrDefault(staIdx + 1);

            bool disableRight = tt.Type == TimetableType.Network && next == null; //TODO: Why only disable on network?
            bool disableLeft = tt.Type == TimetableType.Network && prev == null;

            // Richtungsangaben zeichnen
            if (prev != null)
                e.Graphics.DrawText(font, textColor, 5, 5, "von " + prev.SName);
            if (next != null)
            {
                var nextSize = e.Graphics.MeasureString(font, "nach " + next.SName);
                e.Graphics.DrawText(font, textColor, Width - 5 - nextSize.Width, 5, "nach " + next.SName);
            }

            var leftdefaultTrack = Tracks.IndexOf(Tracks.FirstOrDefault(t => t.Name == DefaultTrackLeft.GetValue(routeIndex)));
            var rightdefaultTrack = Tracks.IndexOf(Tracks.FirstOrDefault(t => t.Name == DefaultTrackRight.GetValue(routeIndex)));

            // Netzwerk: Falls noch keine Angabe: Standardgleise setzen
            if (tt.Type == TimetableType.Network && Tracks.Any())
            {
                if (disableLeft)
                    leftdefaultTrack = 0;
                else if (leftdefaultTrack == -1)
                {
                    MoveDefaultTrack(DefaultTrackLeft, Tracks.First(), 0);
                    leftdefaultTrack = 0;
                }

                if (disableRight)
                    rightdefaultTrack = 0;
                else if (rightdefaultTrack == -1)
                {
                    MoveDefaultTrack(DefaultTrackRight, Tracks.First(), 0);
                    rightdefaultTrack = 0;
                }
            }

            int y = 30;
            int maxIndent = 0;

            foreach (var track in Tracks)
            {
                var trackIndex = Tracks.IndexOf(track);

                // Einrückung des Gleisvorfeldes berehcnen
                var leftIndent = Math.Abs(leftdefaultTrack - trackIndex) * INDENT + 60;
                if (leftdefaultTrack == trackIndex)
                    leftIndent = disableLeft ? 30 : 0;

                var rightIndent = Math.Abs(rightdefaultTrack - trackIndex) * INDENT + 60;
                if (rightdefaultTrack == trackIndex)
                    rightIndent = disableRight ? 30 : 0;
                maxIndent = Math.Max(maxIndent, rightIndent + leftIndent);
                rightIndent = Width - rightIndent;

                // Gleiselinie zeichnen
                e.Graphics.DrawLine(textColor, leftIndent, y, rightIndent, y);

                // Gleisverbindungen zeichnen
                if (trackIndex < leftdefaultTrack)
                    e.Graphics.DrawLine(textColor, leftIndent, y, leftIndent - INDENT, y + LINE_HEIGHT);
                else if (trackIndex > leftdefaultTrack)
                    e.Graphics.DrawLine(textColor, leftIndent, y, leftIndent - INDENT, y - LINE_HEIGHT);

                if (trackIndex < rightdefaultTrack)
                    e.Graphics.DrawLine(textColor, rightIndent, y, rightIndent + INDENT, y + LINE_HEIGHT);
                else if (trackIndex > rightdefaultTrack)
                    e.Graphics.DrawLine(textColor, rightIndent, y, rightIndent + INDENT, y - LINE_HEIGHT);

                // Gleisnamen als Button hinzufügen
                var textSize = e.Graphics.MeasureString(font, track.Name);
                var nameBtn = new RenderBtn<Track>(track, new Point(midx - (int)(textSize.Width / 2) - 5, y - 8), new Size((int)textSize.Width + 5, 16), bgColor, track.Name, textColor);
                nameBtn.Click += NameBtn_Click;
                buttons.Add(nameBtn);

                // Netzwerk: Standardgleise anderer Routen gestrichelt zeichnen.
                if (DefaultTrackLeft.ContainsValue(track.Name) && leftdefaultTrack != trackIndex)
                    e.Graphics.DrawLine(dashedPen, 0, y, leftIndent, y);
                if (DefaultTrackRight.ContainsValue(track.Name) && rightdefaultTrack != trackIndex)
                    e.Graphics.DrawLine(dashedPen, rightIndent, y, Width, y);

                // Aktions-Buttons hinzufügen
                var btnLeft = midx + (int)(textSize.Width / 2) + 10;
                var deleteBtn = GetButton("X", track, btnLeft, y);
                deleteBtn.Click += DeleteBtn_Click;

                var upBtn = GetButton("▲", track, btnLeft + 20, y);
                upBtn.Click += UpBtn_Click;

                var downBtn = GetButton("▼", track, btnLeft + 40, y);
                downBtn.Click += DownBtn_Click;

                // Aktionsbuttons für Standardgleise
                if (trackIndex == leftdefaultTrack && !disableLeft)
                {
                    var leftUpBtn = GetButton("▲", track, 10, y);
                    leftUpBtn.Click += (s, x) => MoveDefaultTrack(DefaultTrackLeft, ((RenderBtn<Track>)s).Tag, -1);
                    var leftDownBtn = GetButton("▼", track, 30, y);
                    leftDownBtn.Click += (s, x) => MoveDefaultTrack(DefaultTrackLeft, ((RenderBtn<Track>)s).Tag, 1);
                }

                if (trackIndex == rightdefaultTrack && !disableRight)
                {
                    var rightUpButton = GetButton("▲", track, Width - 46, y);
                    rightUpButton.Click += (s, x) => MoveDefaultTrack(DefaultTrackRight, ((RenderBtn<Track>)s).Tag, -1);
                    var rightDownBtn = GetButton("▼", track, Width - 26, y);
                    rightDownBtn.Click += (s, x) => MoveDefaultTrack(DefaultTrackRight, ((RenderBtn<Track>)s).Tag, 1);
                }

                y += LINE_HEIGHT;
            }

            // Button für neue Gleise
            var textWidth = (int)e.Graphics.MeasureString(font, "Neues Gleis hinzufügen").Width;
            var addBtn = new RenderBtn<Track>(null, new Point(midx - (textWidth / 2) - 5, y - 8), new Size(textWidth + 10, 16), Colors.LightGrey, "Neues Gleis hinzufügen");
            buttons.Add(addBtn);
            addBtn.Click += AddBtn_Click;

            var newHeight = (Tracks.Count) * LINE_HEIGHT + 50;
            if (newHeight > Height)
                this.Height = newHeight;

            else if (maxIndent > Width - 30)
                Width = maxIndent + 30;

            foreach (var args in buttons)
                args.Draw(e.Graphics);
            
            base.OnPaint(e);
        }

        private void MoveDefaultTrack(IRouteValueCollection<string> property, Track current, int offset)
        {
            var idx = Tracks.IndexOf(current) + offset;
            if (idx < 0 || idx > Tracks.Count - 1)
                return;
            var next = Tracks[idx];
            property.SetValue(routeIndex, next.Name);
            Invalidate();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            var regex = new Regex(@"^Gleis (\d+)$", RegexOptions.Compiled);
            var maxTrack = 0;
            var matchedTracks = Tracks.Select(t => regex.Match(t.Name)).Where(m => m.Success).ToArray();
            if (matchedTracks.Any())
                maxTrack = matchedTracks.Select(m => int.Parse(m.Groups[1].Value)).Max();
            var track = new Track(tt)
            {
                Name = "Gleis " + (maxTrack + 1)
            };

            if (!Tracks.Any())
            {
                DefaultTrackLeft.SetValue(routeIndex, track.Name);
                DefaultTrackRight.SetValue(routeIndex, track.Name);
            }
            Tracks.Add(track);

            Invalidate();
        }

        private void NameBtn_Click(object sender, EventArgs e)
        {
            editingButton = (RenderBtn<Track>) sender;
            var oldName = editingButton.Tag.Name;
            var newName = Shared.UI.InputBox.Query(ParentWindow, "Gleisnamen bearbeiten", oldName);
            if (newName != null && newName != oldName)
                CommitNameEdit(oldName, newName);
        }

        private void CommitNameEdit(string oldName, string newName)
        {
            var duplicate = Tracks.Any(t => t != editingButton.Tag && t.Name == newName);
            if (duplicate)
            {
                MessageBox.Show($"Ein Gleis mit der Bezeichnung {newName} ist bereits vorhanden. Bitte wählen Sie einen anderen Namen!", MessageBoxType.Error);
                return;
            }

            // Streckengleise umbenennen
            DefaultTrackLeft.ReplaceAllValues(editingButton.Tag.Name, newName);
            DefaultTrackRight.ReplaceAllValues(editingButton.Tag.Name, newName);

            // Ankunfts- und Abfahrtsgleise zum umbenennen stagen
            if (TrackRenames.ContainsKey(newName))
                TrackRenames.Remove(newName);
            else if (TrackRenames.ContainsValue(oldName))
            {
                var k = TrackRenames.First(kvp => kvp.Value == oldName).Key;
                TrackRenames[k] = newName;
            }
            else if (!TrackRenames.ContainsKey(oldName) && !TrackRenames.ContainsValue(newName))
                TrackRenames.Add(oldName, newName);

            editingButton.Tag.Name = newName;
            Invalidate();

            editingButton = null;
        }

        private void DownBtn_Click(object sender, EventArgs e)
        {
            var btn = (RenderBtn<Track>)sender;
            var idx = Tracks.IndexOf(btn.Tag);
            if (idx == Tracks.Count - 1)
                return;
            Tracks.Move(idx, idx + 1);
            Invalidate();
        }

        private void UpBtn_Click(object sender, EventArgs e)
        {
            var btn = (RenderBtn<Track>)sender;
            var idx = Tracks.IndexOf(btn.Tag);
            if (idx == 0)
                return;
            Tracks.Move(idx, idx - 1);
            Invalidate();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            var btn = (RenderBtn<Track>)sender;
            Tracks.Remove(btn.Tag);

            var firstName = Tracks.FirstOrDefault()?.Name;
            DefaultTrackLeft.ReplaceAllValues(btn.Tag.Name, firstName);
            DefaultTrackRight.ReplaceAllValues(btn.Tag.Name, firstName);
            
            trackRemoves.Add(btn.Tag.Name);

            Invalidate();
        }

        private RenderBtn<Track> GetButton(string text, Track track, int x, int y)
        {
            var btn = new RenderBtn<Track>(track, new Point(x, y - 8), new Size(16, 16), Colors.LightGrey, text);
            buttons.Add(btn);
            return btn;
        }

        #region DrawArgs click handling
        private bool lastDoubleClick;
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            foreach (var args in buttons.ToArray())
                args.HandleDoubleClick(new Point(e.Location), Point.Empty);

            lastDoubleClick = true;
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!lastDoubleClick)
            {
                if (e.Buttons == MouseButtons.Alternate)
                    foreach (var args in buttons.ToArray())
                        args.HandleRightClick(new Point(e.Location), Point.Empty);
                if (e.Buttons == MouseButtons.Primary)
                    foreach (var args in buttons.ToArray())
                        args.HandleClick(new Point(e.Location), Point.Empty);
            }

            lastDoubleClick = false;
            base.OnMouseDown(e);
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (font != null && !font.IsDisposed)
                font.Dispose();
            dashedPen?.Dispose();
            base.Dispose(disposing);
        }
    }
}
