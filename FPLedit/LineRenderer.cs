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
        private List<Station> stas;
        private List<Panel> panels = new List<Panel>();
        private int markedStation;
        private Font font;
        private Button btn;

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

        public void SetLine(List<Station> stas)
        {
            if (stas != null)
                btn.Enabled = true;
            this.stas = stas;
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


            if (stas == null || stas.Count == 0)
                return;

            var w = ClientSize.Width - 80; // 10px Padding on each side
            var d = w / Math.Max(stas.Count - 1, 1);
            var x = 40;
            var y = 50;
            foreach (var sta in stas)
            {
                var cont = e.Graphics.BeginContainer();
                e.Graphics.TranslateTransform(x + 6, y + 7);
                e.Graphics.RotateTransform(60);

                var text = sta.SName + " (" + sta.Kilometre.ToString("0.0") + ")";
                e.Graphics.DrawString(text, font, Brushes.Black, new Point(0,0));

                e.Graphics.EndContainer(cont);

                var p = new Panel() { Location = new Point(x - 5, y - 5), Size = new Size(10, 10), BackColor = Color.Gray };
                p.MouseDoubleClick += (s, args) => StationDoubleClicked?.Invoke(sta, args);
                p.MouseClick += (s, args) => StationClicked?.Invoke(sta, args);
                Controls.Add(p);
                panels.Add(p);

                x += d;
                if (stas.Last() != sta)
                    e.Graphics.DrawLine(Pens.Black, x - d, y, x, y);
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
