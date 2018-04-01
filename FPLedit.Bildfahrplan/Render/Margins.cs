using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BildfahrplanExport.Render
{
    internal class Margins
    {
        public float Left { get; set; }

        public float  Top { get; set; }

        public float Right { get; set; }

        public float Bottom { get; set; }

        public Margins(float left, float top, float right, float bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}
