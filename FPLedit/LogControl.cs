using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FPLedit
{
    public class LogControl : RichTextArea, ILog
    {
        public LogControl() : base()
        {
            ReadOnly = true;
        }

        #region Log
        public void Error(string message)
            => WriteMl("[FEHLER] " + message, Colors.Red);

        public void Warning(string message)
            => WriteMl("[WARNUNG] " + message, Colors.Orange);

        public void Info(string message)
            => WriteMl("[INFO] " + message, Colors.Black);

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
    }
}
