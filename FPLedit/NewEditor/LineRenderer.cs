using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit
{
    internal class LineRenderer : Control
    {
        private List<Panel> panels = new List<Panel>();
        private int markedStation;
        private Font font;
        private Button btn;

        private Dictionary<Station, Point> stapos;
        private List<List<Station>> routes;

        public event EventHandler<MouseEventArgs> StationClicked;
        public event EventHandler<MouseEventArgs> StationDoubleClicked;
        public event EventHandler NewButtonClicked;

        public LineRenderer()
        {
            this.DoubleBuffered = true;
            font = new Font(Font.FontFamily, 8);

            btn = new Button()
            {
                Text = "Neue Station",
                AutoSize = true,
                Location = new Point(10, 10),
                Enabled = false,
            };
            this.Controls.Add(btn);
            btn.Click += (s, e) => NewButtonClicked?.Invoke(btn, null);
        }

        public void SetLine(Timetable tt)
        {
            if (tt != null)
                btn.Enabled = true;
            else
            {
                routes = null;
                this.Invalidate();
                return;
            }

            if (tt.Type == TimetableType.Linear)
            {
                routes = new List<List<Station>>();
                routes.Add(tt.Stations);

                stapos = new StaPosReader().GenerateLinearPoints(tt, ClientSize.Width);
            }
            else
            {
                var routesIndices = tt.Stations.SelectMany(s => s.Routes).Distinct();
                routes = new List<List<Station>>();
                foreach (var ri in routesIndices)
                {
                    var rt = tt.Stations.Where(s => s.Routes.Contains(ri)).ToList();
                    routes.Add(rt);
                }

                stapos = new StaPosReader().LoadNetworkPoints(tt);
            }

            this.Invalidate();
        }

        public void UpdateLine()
        {
            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.SuspendLayout();
            e.Graphics.Clear(Color.White);
            foreach (var p in panels)
                this.Controls.Remove(p);
            panels.Clear();


            if (routes == null || routes.Count == 0)
                return;

            var xOffset = 40;
            var yOffset = 50;

            foreach (var r in routes)
            {
                Point? lastP = null;
                foreach (var sta in r)
                {
                    var pos = stapos[sta];
                    var x = xOffset + pos.X;
                    var y = yOffset + pos.Y;

                    var cont = e.Graphics.BeginContainer();
                    e.Graphics.TranslateTransform(x + 6, y + 7);
                    e.Graphics.RotateTransform(60);

                    var text = sta.SName + " (" + sta.Kilometre.ToString("0.0") + ")";
                    e.Graphics.DrawString(text, font, Brushes.Black, new Point(0, 0));

                    e.Graphics.EndContainer(cont);

                    var p = new Panel() { Location = new Point(x - 5, y - 5), Size = new Size(10, 10), BackColor = Color.Gray };
                    p.MouseDoubleClick += (s, args) => StationDoubleClicked?.Invoke(sta, args);
                    p.MouseClick += (s, args) => StationClicked?.Invoke(sta, args);
                    Controls.Add(p);
                    panels.Add(p);

                    if (lastP.HasValue)
                        e.Graphics.DrawLine(Pens.Black, x, y, xOffset + lastP.Value.X, yOffset + lastP.Value.Y);
                    lastP = pos;
                }
            }

            this.ResumeLayout();
            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            this.Invalidate();
            base.OnResize(e);
        }
    }
}
