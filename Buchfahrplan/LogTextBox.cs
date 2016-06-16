using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Buchfahrplan
{
    public partial class LogTextBox : TextBox, ILog
    {
        public LogTextBox()
        {
            this.Multiline = true;
            this.ReadOnly = true;
            this.BackColor = Color.White;
        }

        public void Log(string message)
        {
            this.Text += "\n" + message;
        }
    }
}
