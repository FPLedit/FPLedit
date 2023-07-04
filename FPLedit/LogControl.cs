using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;

namespace FPLedit
{
    internal sealed class LogControl : RichTextArea, ILog
    {
        private readonly Color systemText;
        private readonly ContextMenu? menu;
        private bool showDebug;

        public LogControl()
        {
            ReadOnly = true;

            menu = new ContextMenu();
#pragma warning disable CA2000
            menu.CreateItem(T._("Alles löschen"), clickHandler: (_, _) => Text = "");
            menu.CreateCheckItem(T._("Debug-Informationen anzeigen"), changeHandler: (s, _) => showDebug = ((CheckMenuItem)s!).Checked);
#pragma warning restore CA2000

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

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Buttons == MouseButtons.Alternate)
            {
                menu!.Show(this);
                e.Handled = true;
            }
            base.OnMouseDown(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (menu != null && !menu.IsDisposed)
            {
                foreach (var topLevelItem in menu.Items)
                    topLevelItem.DisposeMenu();
                menu.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
