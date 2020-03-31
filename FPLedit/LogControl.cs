using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit
{
    internal sealed class LogControl : RichTextArea, ILog
    {
        private readonly Color systemText;
        private readonly ContextMenu menu;
        private bool showDebug;

        public LogControl()
        {
            ReadOnly = true;

            menu = new ContextMenu();
            menu.CreateItem("Alles löschen", clickHandler: (s, e) => Text = "");
            menu.CreateCheckItem("Debug-Informationen anzeigen", changeHandler: (s, e) => showDebug = ((CheckMenuItem)s).Checked);

            systemText = SystemColors.ControlText;
        }

        #region Log
        public bool CanAttach => false;

        public void AttachLogger(ILog other)
        {
        }
        
        public void Error(string message)
            => WriteMl("[FEHLER] " + message, Colors.Red);

        public void Warning(string message)
            => WriteMl("[WARNUNG] " + message, Colors.Orange);

        public void Info(string message)
            => WriteMl("[INFO] " + message, systemText);

        public void LogException(Exception e)
        {
            WriteMl("[EXCEPTION] " + e.GetExceptionDetails(), Colors.Red);
        }

        public void Debug(string message)
        {
            if (showDebug) WriteMl("[DEBUG] " + message, Colors.Blue);
        }

        private void WriteMl(string message, Color c)
        {
            if (IsDisposed)
                return;
            var lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var l in lines)
                Write(l, c);
        }

        private void Write(string message, Color c)
        {
            Append(message + Environment.NewLine, true);
            int idx, last;
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                idx = Text.LastIndexOf(message, StringComparison.Ordinal);
                last = Text.Length;
            }
            else
            {
                var lines = string.Join("", Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                idx = lines.LastIndexOf(message, StringComparison.Ordinal);
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

        protected override void Dispose(bool disposing)
        {
            if (menu != null && !menu.IsDisposed)
                menu.Dispose();
            base.Dispose(disposing);
        }
    }
}
