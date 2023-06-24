using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Rendering
{
    /// <summary>
    /// MetaFont to internally represent fonts.
    /// </summary>
    public sealed class MFont : IDisposable
    {
        private string family;
        private int size;
        private MFontStyles style;

        private MFont(string family, int size, MFontStyles style)
        {
            this.family = family;
            this.size = size;
            this.style = style;
        }

        public static MFont Create(string family, int size, MFontStyles style = MFontStyles.Regular)
        {
            var cacheEntry = cachedM.FirstOrDefault(m => m != null && m.Family == family && m.Size == size && m.Style == style);
            if (cacheEntry == null)
            {
                cacheEntry = new MFont(family, size, style);
                cachedM.Add(cacheEntry);
            }
            return cacheEntry;
        }

        public string Family { get => family; set => ClearInstanceCache(family = value); }

        public int Size { get => size; set => ClearInstanceCache(size = value); }

        public MFontStyles Style { get => style; set => ClearInstanceCache(style = value); }

        public static explicit operator Eto.Drawing.Font(MFont m)
        {
            if (m.instanceCachedEto == null)
            {
                var family = FontCollection.Families.Contains(m.Family) ? m.Family : FontCollection.GenericSans;
                m.instanceCachedEto = new Eto.Drawing.Font(family, m.Size, (Eto.Drawing.FontStyle)m.Style);
            }
            return m.instanceCachedEto;
        }

#if ENABLE_SYSTEM_DRAWING
        public static explicit operator System.Drawing.Font(MFont m)
        {
            if (m.instanceCachedSd == null)
                m.instanceCachedSd = new System.Drawing.Font(m.Family, m.Size, (System.Drawing.FontStyle)m.Style);
            return m.instanceCachedSd;
        }
#endif
        public static explicit operator SixLabors.Fonts.Font(MFont m)
        {
            if (m.instanceCachedIs == null)
                m.instanceCachedIs = SixLabors.Fonts.SystemFonts.CreateFont(m.Family == "SANS-SERIF" ? "Liberation Sans" : m.Family, m.Size, (SixLabors.Fonts.FontStyle)m.Style);
            return m.instanceCachedIs;
        }

        #region Caching (instance & global)
        private Eto.Drawing.Font? instanceCachedEto;
#if ENABLE_SYSTEM_DRAWING
        private System.Drawing.Font? instanceCachedSd;
#endif
        private SixLabors.Fonts.Font? instanceCachedIs;
        private static readonly List<MFont> cachedM = new List<MFont>();

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        private void ClearInstanceCache(object? o = null)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
        {
            if (instanceCachedEto != null && !instanceCachedEto.IsDisposed)
                instanceCachedEto.Dispose();
            instanceCachedEto = null;
#if ENABLE_SYSTEM_DRAWING
            instanceCachedSd?.Dispose();
            instanceCachedSd = null;
#endif
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this); // We don't need the finaliser any more.
            ClearInstanceCache();
        }
        ~MFont() => Dispose();

        // Extra method, normally not needed in normal operation.
        public static void ClearGlobalCache()
        {
            for (int i = cachedM.Count; i >= 0; i++)
            {
                var obj = cachedM[i];
                obj?.Dispose();
            }
            cachedM.Clear();
        }
        #endregion

        #region Conversion from/to string
        public static MFont Parse(string? def)
        {
            if (def == null || !def.StartsWith("font(", StringComparison.InvariantCulture) || !def.EndsWith(")", StringComparison.InvariantCulture))
                return Create(Eto.Drawing.FontFamilies.SansFamilyName, 9); // Keine valide Font-Definition gefunden

            var parts = def.Substring(5, def.Length - 6).Split(';');
            var family = GetFontFamily(parts[0]);
            var style = (MFontStyles)int.Parse(parts[1]);
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
            var serializedFamily = GetSerializableFontName(Family);
            var serializedStyle = (int)Style;
            return "font(" + serializedFamily + ";" + serializedStyle + ";" + Size + ")";
        }

        private string GetSerializableFontName(string familyToSerialize)
        {
            return familyToSerialize switch
            {
                Eto.Drawing.FontFamilies.SansFamilyName => "SansSerif",
                Eto.Drawing.FontFamilies.SerifFamilyName => "Serif",
                Eto.Drawing.FontFamilies.MonospaceFamilyName => "Monospaced",
                _ => familyToSerialize
            };
        }
        #endregion
    }

    [Flags]
    public enum MFontStyles
    {
        Regular = 0x0,
        Bold = 0x1,
        Italic = 0x2,
        Underline = 0x4,
        Strikeout = 0x8
    }
}
