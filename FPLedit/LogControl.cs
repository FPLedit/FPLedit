using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit
{
    class LogControl : Control, ILog
    {
        List<Line> strings = new List<Line>();
        private VScrollBar vScrollBar1;
        private TextBox txt;
        private int drawIndex = 0;
        private VScrollBar scroll;
        private bool scrollToEnd = false;

        public LogControl() : base()
        {
            scroll = new VScrollBar();
            scroll.Dock = DockStyle.Right;
            scroll.Scroll += Scroll_Scroll;
            this.Controls.Add(scroll);

            txt = new TextBox();
            txt.Visible = false;
            txt.ReadOnly = true;
            txt.Multiline = true;
            txt.BackColor = Color.White;
            txt.ScrollBars = ScrollBars.Vertical;
            txt.Dock = DockStyle.Fill;
            txt.DoubleClick += (s, e) =>
            {
                SwitchMode();
            };
            txt.LostFocus += (s, e) => SwitchMode();
            this.Controls.Add(txt);

            var menu = new ContextMenuStrip();
            var itm = menu.Items.Add("Alles Kopieren");
            itm.Click += (s, e) => CopyAll();
            this.ContextMenuStrip = menu;
            this.DoubleBuffered = true;
            this.DoubleClick += (s, e) => SwitchMode();
        }

        private void Scroll_Scroll(object sender, ScrollEventArgs e)
        {
            drawIndex = scroll.Value;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var h = e.Graphics.MeasureString("H", this.Font); // typischer Großbuchstabe
            scroll.Minimum = 0;
            scroll.Maximum = strings.Count;

            float y = this.Padding.Top;
            float x = this.Padding.Left;
            for (int i = drawIndex; i < strings.Count; i++)
            {
                var dimens = e.Graphics.MeasureString(strings[i].Text, this.Font);
                //bool selected = (y+h.Height) > mouseStart.Y && (y) < mouseCur.Y;

                //if (selected)
                //    e.Graphics.FillRectangle(new SolidBrush(SystemColors.Highlight), 0, y, dimens.Width, dimens.Height);


                e.Graphics.DrawString(strings[i].Text, this.Font, strings[i].Brush, x, y);


                y += h.Height + 3;
                if (scrollToEnd && y > Height - this.Padding.Bottom)
                {
                    drawIndex++;
                    scroll.Value = drawIndex;
                    this.Invalidate();
                    return;
                }
            }
            if (scrollToEnd && y <= Height)
                scrollToEnd = false;
            ControlPaint.DrawBorder3D(e.Graphics, new Rectangle(0, 0, Width, Height), Border3DStyle.Etched);

            //e.Graphics.DrawLine(Pens.OrangeRed, mouseStart, mouseCur);

            base.OnPaint(e);
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Up && drawIndex > 0)
            {
                drawIndex--;
                scroll.Value = drawIndex;
                Invalidate();
            }
            if (e.KeyCode == Keys.Down && drawIndex < strings.Count)
            {
                drawIndex++;
                scroll.Value = drawIndex;
                Invalidate();
            }
            if (e.Control && e.KeyCode == Keys.C)
                CopyAll();

            base.OnPreviewKeyDown(e);
        }

        private void CopyAll()
        {
            Clipboard.SetText(string.Join(Environment.NewLine, strings.Select(l => l.Text).ToArray()));
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int idxBefore = drawIndex;

            drawIndex += -(e.Delta / 120);
            if (drawIndex < 0)
                drawIndex = 0;
            var max = scroll.Maximum - scroll.LargeChange + 1;
            if (drawIndex > max)
                drawIndex = max;
            scroll.Value = drawIndex;

            if (idxBefore != drawIndex)
                Invalidate();
            base.OnMouseWheel(e);
        }

        private void SwitchMode()
        {
            txt.Visible = !txt.Visible;
            scroll.Visible = !scroll.Visible;
        }

        #region Log
        public void Error(string message)
            => Write(message, Brushes.Red);

        public void Warning(string message)
            => Write(message, Brushes.Orange);

        public void Info(string message)
            => Write(message, Brushes.Black);

        private void Write(string message, Brush b)
        {
            txt.Text += message + Environment.NewLine;

            var origWordCount = message.Split(' ').Length;
            string text = message;
            bool exit = false;
            var max = Width - Padding.Left - Padding.Right;
            while (TextRenderer.MeasureText(text, Font).Width > max && !exit)
            {
                string[] words = text.Split(' ');

                text = string.Join(" ", words.Take(words.Length - 1));
                if (words.Length == 1)
                {
                    text = message;
                    exit = true;
                }
            }
            var textWordCount = text.Split(' ').Length;

            strings.Add(new Line(text, b));
            if (textWordCount < origWordCount && !exit)
                Write(message.Replace(text, "").Trim(), b);
            scrollToEnd = true;
            Invalidate();
        }
        #endregion

        //bool enabled;
        //Point mouseStart, mouseCur;
        //protected override void OnMouseDown(MouseEventArgs e)
        //{
        //    enabled = e.Button == MouseButtons.Left;
        //    mouseStart = e.Location;
        //    base.OnMouseDown(e);
        //}

        //protected override void OnMouseUp(MouseEventArgs e)
        //{
        //    enabled = false;
        //    base.OnMouseUp(e);
        //}

        //protected override void OnMouseMove(MouseEventArgs e)
        //{
        //    if (!enabled)
        //        return;
        //    mouseCur = e.Location;
        //    Invalidate();

        //    base.OnMouseMove(e);
        //}

        struct Line
        {
            public string Text;
            public Brush Brush;

            public Line(string text, Brush b)
            {
                Text = text;
                Brush = b;
            }
        }
    }
}
