using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit
{
    public partial class LogTextBox : TextBox, ILog
    {
        public LogTextBox()
        {
            this.Multiline = true;
            this.ReadOnly = true;
            this.BackColor = Color.White;
            this.ScrollBars = ScrollBars.Vertical;
        }

        public void Error(string message)
        {
            this.Text += "Fehler: " + message + Environment.NewLine;
            this.ScrollToEnd();
        }

        public void Info(string message)
        {
            this.Text += message + Environment.NewLine;
            this.ScrollToEnd();
        }

        public void Warning(string message)
        {
            this.Text += "Warnung: " + message + Environment.NewLine;
            this.ScrollToEnd();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            this.ScrollToEnd();
            base.OnLostFocus(e);
        }

        private void ScrollToEnd()
        {
            this.Select(this.Text.Length, 0);
            this.ScrollToCaret();
        }
    }
}
