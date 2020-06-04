using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Bildfahrplan.Helpers
{
    internal sealed class DashStyleHelper
    {
        private readonly List<DashStyle> types = new List<DashStyle>
        {
            new DashStyle("Normal", new[] { 10000.0f, 0.01f }),
            new DashStyle("Gepunktet", new[] { 3.0f, 3.0f }),
            new DashStyle("Kurz gestrichelt", new[] { 6.0f, 3.0f }),
            new DashStyle("Länger gestrichelt (63)", new[] { 9.0f, 3.0f }),
            new DashStyle("Länger gestrichelt (93)", new[] { 12.0f, 3.0f }),
            new DashStyle("Lang gestrichelt", new[] { 15.0f, 3.0f })
        };

        public int[] Indices => Enumerable.Range(0, types.Count - 1).ToArray();

        public float[] ParseDashstyle(int index)
            => types[index].DashPattern;

        public Eto.Drawing.DashStyle ParseEtoDashstyle(int index)
            => new Eto.Drawing.DashStyle(0f, types[index].DashPattern);

        public string GetDescription(int index)
            => types[index].Description;

        private class DashStyle
        {
            public string Description { get; set; }

            public float[] DashPattern { get; set; }

            public DashStyle(string description, float[] pattern)
            {
                Description = description;
                DashPattern = pattern;
            }
        }
    }
}
