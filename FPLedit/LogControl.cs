using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FPLedit
{
    public class LogControl : RichTextArea, ILog
    {
        private ContextMenu menu;

        public LogControl() : base()
        {
            ReadOnly = true;

            menu = new ContextMenu();
            var btn = menu.CreateItem("Alles löschen");
            btn.Click += (s, e) => Text = "";
        }

        #region Log
        public void Error(string message)
            => WriteMl("[FEHLER] " + message, Colors.Red);

        public void Warning(string message)
            => WriteMl("[WARNUNG] " + message, Colors.Orange);

        public void Info(string message)
            => WriteMl("[INFO] " + message, Colors.Black);

        public void LogException(Exception e)
        {
        }

        public void Debug(string message)
        {
        }

        private void WriteMl(string message, Color c)
        {
            var lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in lines)
                Write(l, c);
        }

        private void Write(string message, Color c)
        {
            Append(message + Environment.NewLine, true);

            int idx = -1, last = -1;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                idx = Text.LastIndexOf(message);
                last = Text.Length;
            }
            else
            {
                var lines = string.Join("", Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                idx = lines.LastIndexOf(message);
                last = lines.Length;
            }

            if (idx == -1)
                return;

            Selection = new Range<int>(idx, idx + message.Length);
            SelectionForeground = c;
            Selection = new Range<int>(last, last);
        }
        #endregion

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Buttons == MouseButtons.Alternate)
            {
                menu.Show(this);
                e.Handled = true;
            }
            base.OnMouseUp(e);
        }
    }
}
