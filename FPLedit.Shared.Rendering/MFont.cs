using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Rendering
{
    /// <summary>
    /// MetaFont to internally represent fonts.
    /// </summary>
    public sealed class MFont : IDisposable
    {
        private string family;
        private int size;
        private MFontStyle style;

        public static MFont Create(string family, int size, MFontStyle style = MFontStyle.Regular)
        {
            var cacheEntry = cachedM.FirstOrDefault(m => m != null && m.Family == family && m.Size == size && m.Style == style);
            if (cacheEntry == null)
            {
                cacheEntry = new MFont
                {
                    Family = family,
                    Size = size,
                    Style = style
                };
                cachedM.Add(cacheEntry);
            }
            return cacheEntry;
        }

        public string Family { get => family; set => ClearInstanceCache(family = value); }

        public int Size { get => size; set => ClearInstanceCache(size = value); }

        public MFontStyle Style { get => style; set => ClearInstanceCache(style = value); }

        public static explicit operator Eto.Drawing.Font(MFont m)
        {
            if (m.instanceCachedEto == null)
            {
                var family = FontCollection.Families.Contains(m.Family) ? m.Family : FontCollection.GenericSans;
                m.instanceCachedEto = new Eto.Drawing.Font(family, m.Size, (Eto.Drawing.FontStyle)m.Style);
            }
            return m.instanceCachedEto;
        }

        #region Caching (instance & global)
        private Eto.Drawing.Font instanceCachedEto;
        private static readonly List<MFont> cachedM = new List<MFont>();

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        private void ClearInstanceCache(object o)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
        {
            instanceCachedEto?.Dispose();
            instanceCachedEto = null;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this); // We don't need the finaliser any more.
            ClearInstanceCache(null);
        }
        ~MFont() => Dispose();

        // Extra method, normally not needed in normal operation.
        public static void ClearGlobalCache()
        {
            for (int i = cachedM.Count; i >= 0; i++)
            {
                var obj = cachedM[i];
                obj?.Dispose();
                cachedM[i] = null;
            }
            cachedM.Clear();
        }
        #endregion

        #region Conversion from/to string
        public static MFont Parse(string def)
        {
            if (def == null || !def.StartsWith("font(") || !def.EndsWith(")"))
                return Create(Eto.Drawing.FontFamilies.SansFamilyName, 9); // Keine valide Font-Definition gefunden

            var parts = def.Substring(5, def.Length - 6).Split(';');
            var family = GetFontFamily(parts[0]);
            var style = (MFontStyle)int.Parse(parts[1]);
            var size = int.Parse(parts[2]);

            return Create(family, size, style);
        }

        private static string GetFontFamily(string name)
        {
            switch (name.ToLower())
            {
                case "sansserif":
                case "dialog":
                case "dialoginput":
                    return Eto.Drawing.FontFamilies.SansFamilyName;
                case "monospaced":
                    return Eto.Drawing.FontFamilies.MonospaceFamilyName;
                case "serif":
                    return Eto.Drawing.FontFamilies.SerifFamilyName;
            }
            return name;
        }

        public string FontToString()
        {
            var family = GetFontName(Family);
            var style = (int)Style;
            return "font(" + family + ";" + style + ";" + Size + ")";
        }

        private string GetFontName(string family)
        {
            if (family == Eto.Drawing.FontFamilies.SansFamilyName)
                return "SansSerif";
            if (family == Eto.Drawing.FontFamilies.SerifFamilyName)
                return "Serif";
            if (family == Eto.Drawing.FontFamilies.MonospaceFamilyName)
                return "Monospaced";
            return family;
        }
        #endregion
    }

    [Flags]
    public enum MFontStyle
    {
        Regular = 0x0,
        Bold = 0x1,
        Italic = 0x2,
        Underline = 0x4,
        Strikeout = 0x8
    }
}
