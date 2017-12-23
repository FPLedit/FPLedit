using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Editor.Network
{
    internal class DividerPanel : Panel
    {
        public DividerPanel() : base()
        {
            BackColor = System.Drawing.Color.Gray;
            Location = new System.Drawing.Point(222, 3);
            Size = new System.Drawing.Size(2, 23);
        }
    }
}
