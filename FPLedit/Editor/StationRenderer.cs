using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    public class StationRenderer : Drawable
    {
        const int INDENT = 20;
        const int LINE_HEIGHT = 30;
        private Font font = new Font(FontFamilies.SansFamilyName, 8);
        private Pen dashedPen = new Pen(Colors.Black, 1) { DashStyle = DashStyles.Dash };

        private List<RenderBtn<Track>> buttons = new List<RenderBtn<Track>>();
        private PixelLayout layout;

        private TextBox editingTextBox;
        private RenderBtn<Track> editingButton;

        private Station _station;
        public Station Station
        {
            get => _station;
            set
            {
                _station = value;
                this.Invalidate();
            }
        }

        private int _route;
        public int Route
        {
            get => _route;
            set
            {
                _route = value;
                this.Invalidate();
            }
        }

        public Dictionary<string, string> TrackRenames { get; } = new Dictionary<string, string>();

        public StationRenderer()
        {
            layout = new PixelLayout();
            Content = layout;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Reset
            e.Graphics.Clear(Colors.White);
            buttons.Clear();

            var width = (int)e.Graphics.ClipBounds.Width;
            int midx = width / 2;

            // Richtungsangaben ermitteln
            var route = _station._parent.GetRoute(_route).GetOrderedStations();
            var staIdx = route.IndexOf(_station);
            var prev = route.ElementAtOrDefault(staIdx - 1);
            var next = route.ElementAtOrDefault(staIdx + 1);

            bool disableRight = _station._parent.Type == TimetableType.Network && next == null;
            bool disableLeft = _station._parent.Type == TimetableType.Network && prev == null;

            // Richtungsangaben zeichnen
            if (prev != null)
                e.Graphics.DrawText(font, Colors.Black, 5, 5, "von " + prev.SName);
            if (next != null)
            {
                var nextSize = e.Graphics.MeasureString(font, "nach " + next.SName);
                e.Graphics.DrawText(font, Colors.Black, width - 5 - nextSize.Width, 5, "nach " + next.SName);
            }

            var leftdefaultTrack = _station.Tracks.IndexOf(_station.Tracks.FirstOrDefault(t => t.Name == _station.DefaultTrackLeft.GetValue(_route)));
            var rightdefaultTrack = _station.Tracks.IndexOf(_station.Tracks.FirstOrDefault(t => t.Name == _station.DefaultTrackRight.GetValue(_route)));

            // Netzwerk: Falls noch keine Angabe: Standardgleise setzen
            if (_station._parent.Type == TimetableType.Network && _station.Tracks.Any())
            {
                if (disableLeft)
                    leftdefaultTrack = 0;
                else if (leftdefaultTrack == -1)
                {
                    MoveDefaultTrack(_station.DefaultTrackLeft, _station.Tracks.First(), 0);
                    leftdefaultTrack = 0;
                }

                if (disableRight)
                    rightdefaultTrack = 0;
                else if (rightdefaultTrack == -1)
                {
                    MoveDefaultTrack(_station.DefaultTrackRight, _station.Tracks.First(), 0);
                    rightdefaultTrack = 0;
                }
            }

            int y = 30;
            int maxIndent = 0;

            foreach (var track in _station.Tracks)
            {
                var trackIndex = _station.Tracks.IndexOf(track);

                // Einrückung des Gleisvorfeldes berehcnen
                var leftIndent = Math.Abs(leftdefaultTrack - trackIndex) * INDENT + 60;
                if (leftdefaultTrack == trackIndex)
                    leftIndent = disableLeft ? 30 : 0;

                var rightIndent = Math.Abs(rightdefaultTrack - trackIndex) * INDENT + 60;
                if (rightdefaultTrack == trackIndex)
                    rightIndent = disableRight ? 30 : 0;
                maxIndent = Math.Max(maxIndent, rightIndent + leftIndent);
                rightIndent = width - rightIndent;

                // Gleiselinie zeichnen
                e.Graphics.DrawLine(Colors.Black, leftIndent, y, rightIndent, y);

                // Gleisverbindungen zeichnen
                if (trackIndex < leftdefaultTrack)
                    e.Graphics.DrawLine(Colors.Black, leftIndent, y, leftIndent - INDENT, y + LINE_HEIGHT);
                else if (trackIndex > leftdefaultTrack)
                    e.Graphics.DrawLine(Colors.Black, leftIndent, y, leftIndent - INDENT, y - LINE_HEIGHT);

                if (trackIndex < rightdefaultTrack)
                    e.Graphics.DrawLine(Colors.Black, rightIndent, y, rightIndent + INDENT, y + LINE_HEIGHT);
                else if (trackIndex > rightdefaultTrack)
                    e.Graphics.DrawLine(Colors.Black, rightIndent, y, rightIndent + INDENT, y - LINE_HEIGHT);

                // Gleisnamen als Button hinzufügen
                var textSize = e.Graphics.MeasureString(font, track.Name);
                var nameBtn = new RenderBtn<Track>(track, new Point(midx - (int)(textSize.Width / 2) - 5, y - 8), new Size((int)textSize.Width + 5, 16), Colors.White, track.Name);
                nameBtn.Click += NameBtn_Click;
                buttons.Add(nameBtn);

                // Netzwerk: Standardgleise anderer Routen gestrichelt zeichnen.
                if (_station.DefaultTrackLeft.ContainsValue(track.Name) && leftdefaultTrack != trackIndex)
                    e.Graphics.DrawLine(dashedPen, 0, y, leftIndent, y);
                if (_station.DefaultTrackRight.ContainsValue(track.Name) && rightdefaultTrack != trackIndex)
                    e.Graphics.DrawLine(dashedPen, rightIndent, y, width, y);

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
                    leftUpBtn.Click += (s, x) => MoveDefaultTrack(_station.DefaultTrackLeft, ((RenderBtn<Track>)s).Tag, -1);
                    var leftDownBtn = GetButton("▼", track, 30, y);
                    leftDownBtn.Click += (s, x) => MoveDefaultTrack(_station.DefaultTrackLeft, ((RenderBtn<Track>)s).Tag, 1);
                }

                if (trackIndex == rightdefaultTrack && !disableRight)
                {
                    var rightUpButton = GetButton("▲", track, width - 46, y);
                    rightUpButton.Click += (s, x) => MoveDefaultTrack(_station.DefaultTrackRight, ((RenderBtn<Track>)s).Tag, -1);
                    var rightDownBtn = GetButton("▼", track, width - 26, y);
                    rightDownBtn.Click += (s, x) => MoveDefaultTrack(_station.DefaultTrackRight, ((RenderBtn<Track>)s).Tag, 1);
                }

                y += LINE_HEIGHT;
            }

            // Button für neue Gleise
            var textWidth = (int)e.Graphics.MeasureString(font, "Neues Gleis hinzufügen").Width;
            var addBtn = new RenderBtn<Track>(null, new Point(midx - (textWidth / 2) - 5, y - 8), new Size(textWidth + 10, 16), Colors.LightGrey, "Neues Gleis hinzufügen");
            buttons.Add(addBtn);
            addBtn.Click += AddBtn_Click;

            var newHeight = (_station.Tracks.Count) * LINE_HEIGHT + 50;
            if (newHeight > Height)
                this.Height = newHeight;

            else if (maxIndent > Width - 30)
                Width = maxIndent + 30;

            foreach (var args in buttons)
                args.Draw(e.Graphics);

            base.OnPaint(e);
        }

        private void MoveDefaultTrack(RouteValueCollection<string> property, Track current, int offset)
        {
            if (!CommitNameEdit())
                return;

            var idx = _station.Tracks.IndexOf(current) + offset;
            if (idx < 0 || idx > _station.Tracks.Count - 1)
                return;
            var next = _station.Tracks[idx];
            property.SetValue(_route, next.Name);
            Invalidate();
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            if (!CommitNameEdit())
                return;

            var count = _station.Tracks.Count(t => t.Name.StartsWith("Neues Gleis "));
            var track = new Track(_station._parent);
            track.Name = "Neues Gleis " + (count + 1);

            if (!_station.Tracks.Any())
            {
                _station.DefaultTrackLeft.SetValue(_route, track.Name);
                _station.DefaultTrackRight.SetValue(_route, track.Name);
            }
            _station.Tracks.Add(track);

            Invalidate();
        }

        private void NameBtn_Click(object sender, EventArgs e)
        {
            if (!CommitNameEdit())
                return;

            editingButton = (RenderBtn<Track>)sender;
            editingTextBox = new TextBox() { Width = 30 + editingButton.Size.Width, Text = editingButton.Text, Font = font };
            layout.Add(editingTextBox, editingButton.Location - new Size(25, 0));
            editingTextBox.KeyDown += (s, args) =>
            {
                if (args.Key == Keys.Enter)
                {
                    args.Handled = true;
                    CommitNameEdit();
                }
            };
        }

        public bool CommitNameEdit()
        {
            if (editingTextBox == null)
                return true;

            var name = editingTextBox.Text;
            var oldName = editingButton.Tag.Name;

            var duplicate = _station.Tracks.Any(t => t != editingButton.Tag && t.Name == name);
            if (duplicate)
            {
                MessageBox.Show($"Ein Gleis mit der Bezeichnung {name} ist bereits vorhanden. Bitte wählen Sie einen anderen Namen!", MessageBoxType.Error);
                return false;
            }

            // Streckengleise umbenennen
            _station.DefaultTrackLeft.ReplaceAllValues(editingButton.Tag.Name, name);
            _station.DefaultTrackRight.ReplaceAllValues(editingButton.Tag.Name, name);

            // Ankunfts- und Abfhartsgleise zum umbenennen stagen
            if (TrackRenames.ContainsKey(name))
                TrackRenames.Remove(name);
            else if (TrackRenames.ContainsValue(oldName))
            {
                var k = TrackRenames.First(kvp => kvp.Value == oldName).Key;
                TrackRenames[k] = name;
            }
            else if (!TrackRenames.ContainsKey(name) && !TrackRenames.ContainsValue(name))
                TrackRenames.Add(oldName, name);

            editingButton.Tag.Name = name;
            Invalidate();

            layout.Remove(editingTextBox);
            editingTextBox = null;
            editingButton = null;
            return true;
        }

        private void DownBtn_Click(object sender, EventArgs e)
        {
            if (!CommitNameEdit())
                return;

            var btn = (RenderBtn<Track>)sender;
            var idx = _station.Tracks.IndexOf(btn.Tag);
            if (idx == _station.Tracks.Count - 1)
                return;
            _station.Tracks.Move(idx, idx + 1);
            Invalidate();
        }

        private void UpBtn_Click(object sender, EventArgs e)
        {
            if (!CommitNameEdit())
                return;

            var btn = (RenderBtn<Track>)sender;
            var idx = _station.Tracks.IndexOf(btn.Tag);
            if (idx == 0)
                return;
            _station.Tracks.Move(idx, idx - 1);
            Invalidate();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (!CommitNameEdit())
                return;

            var btn = (RenderBtn<Track>)sender;
            _station.Tracks.Remove(btn.Tag);

            var firstName = _station.Tracks.FirstOrDefault()?.Name;
            _station.DefaultTrackLeft.ReplaceAllValues(btn.Tag.Name, firstName);
            _station.DefaultTrackRight.ReplaceAllValues(btn.Tag.Name, firstName);

            Invalidate();
        }

        private RenderBtn<Track> GetButton(string text, Track track, int x, int y)
        {
            var btn = new RenderBtn<Track>(track, new Point(x, y - 8), new Size(16, 16), Colors.LightGrey, text);
            buttons.Add(btn);
            return btn;
        }

        #region DrawArgs click handling
        private long lastClick;
        private bool lastDoubleClick;
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (!CommitNameEdit())
                return;

            foreach (var args in buttons.ToArray())
                args.HandleDoubleClick(new Point(e.Location), Point.Empty);

            lastDoubleClick = true;
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (!CommitNameEdit())
                return;

            lastClick = DateTime.Now.Ticks;

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
    }
}
