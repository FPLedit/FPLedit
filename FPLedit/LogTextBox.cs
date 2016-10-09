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
        }

        public void Error(string message)
        {
            this.Text += "Fehler: " + message + Environment.NewLine;
        }

        public void Info(string message)
        {
            this.Text += message + Environment.NewLine;
        }

        public void Warning(string message)
        {
            this.Text += "Warnung: " + message + Environment.NewLine;
        }
    }
}
