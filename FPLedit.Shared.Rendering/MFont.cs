using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Rendering
{
    /// <summary>
    /// MetaFont to internally represent fonts that can be converted to System.Drawing, Eto.Drawing or Sixlabors.Fonts font instances.
    /// </summary>
    /// <remarks>All usages of this class are cached. Repeated font creation with the same parameters or </remarks>
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
        private System.Drawing.Font? instanceCachedSd;
        public static explicit operator System.Drawing.Font(MFont m)
        {
            if (m.instanceCachedSd == null)
                m.instanceCachedSd = new System.Drawing.Font(m.Family, m.Size, (System.Drawing.FontStyle)m.Style);
            return m.instanceCachedSd;
        }
#endif
        public static explicit operator SixLabors.Fonts.Font(MFont m)
        {
            if (m.instanceCachedIs != null)
                return m.instanceCachedIs;

            // Initialize font collection with system fonts.
            if (imageSharpFontCollection == null)
            {
                imageSharpFontCollection = new SixLabors.Fonts.FontCollection();
                SixLabors.Fonts.FontCollectionExtensions.AddSystemFonts(imageSharpFontCollection);
                //TODO: Add a local path relative to the application directory, if it exists!
            }

            SixLabors.Fonts.FontFamily? GetFamily(params string[] families)
            {
                foreach (var family in families)
                {
                    try { return imageSharpFontCollection!.Get(family); }
                    catch (SixLabors.Fonts.FontFamilyNotFoundException) {}
                }
                return null;
            }

            // This list is based on the font discovery code of Eto, removing the split on the different platforms.
            // I also added some fonts (e.g. the Liberation families) that I often find on Linux systems.
            var family = m.Family.ToUpperInvariant() switch
            {
                Eto.Drawing.FontFamilies.SansFamilyName => GetFamily("Arial", "Liberation Sans", "Helvetica", "Tahoma", "Verdana", "sans", "Trebuchet", "FreeSans"),
                Eto.Drawing.FontFamilies.SerifFamilyName => GetFamily("Times New Roman", "Liberation Serif", "serif", "FreeSerif", "Times"),
                Eto.Drawing.FontFamilies.MonospaceFamilyName => GetFamily("Courier New", "Liberation Mono", "monospace", "FreeMono", "Courier"),
                Eto.Drawing.FontFamilies.CursiveFamilyName => GetFamily("URW Chancery L", "Comic Sans MS", "Purisa", "Vemana2000", "Domestic Manners", "Papyrus", "Monotype Corsiva", "serif"),
                Eto.Drawing.FontFamilies.FantasyFamilyName => GetFamily("Impact", "Juice ITC", "Penguin Attack", "Balker", "Marked Fool", "Junkyard", "Linux Biolinum", "serif"),
                _ => GetFamily(m.Family),
            };
            if (!family.HasValue)
                throw new Exception(T._("Schriftart {0} wurde nicht gefunden!", m.Family));

            m.instanceCachedIs = new SixLabors.Fonts.Font(family.Value, m.Size * 4f / 3, (SixLabors.Fonts.FontStyle) m.Style);
            return m.instanceCachedIs;
        }

        #region Caching (instance & global)
        private Eto.Drawing.Font? instanceCachedEto;
        private SixLabors.Fonts.Font? instanceCachedIs;
        private static readonly List<MFont> cachedM = new List<MFont>();

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        private void ClearInstanceCache(object? o = null)
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen
        {
            if (instanceCachedEto != null && !instanceCachedEto.IsDisposed)
                instanceCachedEto.Dispose();
            instanceCachedEto = null;
            instanceCachedIs = null;
#if ENABLE_SYSTEM_DRAWING
            instanceCachedSd?.Dispose();
            instanceCachedSd = null;
#endif
        }

        private static SixLabors.Fonts.FontCollection? imageSharpFontCollection;

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

        #region Conversion from/to java string representation
        public static MFont ParseJavaString(string? def)
        {
            if (def == null || !def.StartsWith("font(", StringComparison.InvariantCulture) || !def.EndsWith(")", StringComparison.InvariantCulture))
                return Create(Eto.Drawing.FontFamilies.SansFamilyName, 9); // Keine valide Font-Definition gefunden

            var parts = def.Substring(5, def.Length - 6).Split(';');
            var family = GetFontFamilyFromJava(parts[0]);
            var style = (MFontStyles)int.Parse(parts[1]);
            var size = int.Parse(parts[2]);

            return Create(family, size, style);
        }

        private static string GetFontFamilyFromJava(string name)
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

        public string FontToJavaString()
        {
            var serializedFamily = Family.ToUpperInvariant() switch
            {
                Eto.Drawing.FontFamilies.SansFamilyName => "SansSerif",
                Eto.Drawing.FontFamilies.SerifFamilyName => "Serif",
                Eto.Drawing.FontFamilies.MonospaceFamilyName => "Monospaced",
                Eto.Drawing.FontFamilies.CursiveFamilyName => throw new NotSupportedException("Font conversion to Java string failed!"),
                Eto.Drawing.FontFamilies.FantasyFamilyName => throw new NotSupportedException("Font conversion to Java string failed!"),
                _ => Family
            };
            return $"font({serializedFamily};{(int)Style};{Size})";
        }
        #endregion
    }

    /// <summary>
    /// Font styles as used for <see cref="MFont"/>.
    /// </summary>
    /// <remarks>Not all of these values can be used in conversions to other font implementations!</remarks>
    [Flags]
    public enum MFontStyles
    {
        Regular = 0x0,
        Bold = 0x1,
        Italic = 0x2,
        /// <remarks>Not applicable for Sixlabors.Fonts and Eto.Drawing conversion.</remarks>
        Underline = 0x4,
        /// <remarks>Not applicable for Sixlabors.Fonts and Eto.Drawing conversion.</remarks>
        Strikeout = 0x8
    }
}
