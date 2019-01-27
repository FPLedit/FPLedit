using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.Rendering
{
    public class MColor
    {
        public byte R { get; set; }

        public byte G { get; set; }

        public byte B { get; set; }

        public MColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public static bool operator ==(MColor c1, MColor c2) => c1.Equals(c2);

        public static bool operator !=(MColor c1, MColor c2) => !c1.Equals(c2);

        public override bool Equals(object obj)
            => obj is MColor c2 && this.R == c2.R && this.G == c2.G && this.B == c2.B;

        public override int GetHashCode() => R + (G << 8) + (B << 16);

        public static explicit operator MColor(Eto.Drawing.Color sc)
            => new MColor((byte)sc.Rb, (byte)sc.Gb, (byte)sc.Bb);

        public static explicit operator Eto.Drawing.Color(MColor m)
            => Eto.Drawing.Color.FromArgb(m.R, m.G, m.B, 255);

        public static MColor White => new MColor(255, 255, 255);
    }
}
