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
        private TextBox txt;
        private RichTextBox rtf;
        private Mode mode;

        public LogControl() : base()
        {
            rtf = new RichTextBox();
            rtf.Dock = DockStyle.Fill;
            rtf.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
            rtf.ReadOnly = true;
            rtf.BackColor = Color.White;
            rtf.LinkClicked += (s, e) => Process.Start(e.LinkText);
            this.Controls.Add(rtf);

            txt = new TextBox();
            txt.ReadOnly = true;
            txt.Multiline = true;
            txt.BackColor = Color.White;
            txt.ScrollBars = ScrollBars.Vertical;
            txt.Dock = DockStyle.Fill;
            this.Controls.Add(txt);

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                SwitchMode(Mode.RTF);
            else
                SwitchMode(Mode.Plain);
        }

        private void SwitchMode(Mode mode)
        {
            this.mode = mode;
            txt.Visible = mode == Mode.Plain;
            rtf.Visible = mode == Mode.RTF;
        }

        #region Log
        public void Error(string message)
            => Write(message, Color.Red);

        public void Warning(string message)
            => Write(message, Color.Orange);

        public void Info(string message)
            => Write(message, Color.Black);

        private void Write(string message, Color c)
        {
            var msg = message + Environment.NewLine;
            if (mode == Mode.Plain)
            {
                txt.AppendText(msg);
                return;
            }
            rtf.AppendText(msg);
            msg = msg.Trim();
            var idx = rtf.Find(msg);
            if (idx == -1)
                return;
            rtf.Select(idx, msg.Length);
            rtf.SelectionColor = c;
            rtf.Select(rtf.Text.Length, 0);
            rtf.ScrollToCaret();
        }
        #endregion

        enum Mode
        {
            RTF,
            Plain
        }
    }
}
