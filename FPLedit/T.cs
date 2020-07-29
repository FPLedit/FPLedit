using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NGettext;
using NGettext.Loaders;
using NGettext.Plural;

namespace FPLedit
{
    public static class T
    {
        private static string locale = "de-DE";
        private static Dictionary<string, ICatalog> catalogs = new Dictionary<string, ICatalog>();

        internal static void SetLocale(string locale)
        {
            T.locale = locale;
            catalogs = new Dictionary<string, ICatalog>();
        }

        public static string _(string text)
        {
            var assembly = Assembly.GetCallingAssembly();
            var name = assembly.GetName().Name;
            if (catalogs.TryGetValue(name, out var catalog))
                return catalog.GetString(text);
            var newCatalog = new Catalog(new CustomMoLoader(name, "Languages"), new System.Globalization.CultureInfo(locale));
            return newCatalog.GetString(text);
        }
        public static string _(string text, params object[] args)
        {
            var assembly = Assembly.GetCallingAssembly();
            var name = assembly.GetName().Name;
            if (catalogs.TryGetValue(name, out var catalog))
                return catalog.GetString(text, args);
            var newCatalog = new Catalog(new CustomMoLoader(name, "Languages"), new System.Globalization.CultureInfo(locale));
            return newCatalog.GetString(text, args);
        }
    }

    internal sealed class CustomMoLoader : MoLoader
    {
        protected override string GetFileName(string localeDir, string domain, string locale) => Path.Combine(localeDir, domain + "." + locale + ".mo");
        
        public CustomMoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(domain, localeDir, pluralRuleGenerator, parser) { }
        public CustomMoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(filePath, pluralRuleGenerator, parser) { }
        public CustomMoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(moStream, pluralRuleGenerator, parser) { }
        public CustomMoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator) : base(domain, localeDir, pluralRuleGenerator) { }
        public CustomMoLoader(string domain, string localeDir, MoFileParser parser) : base(domain, localeDir, parser) { }
        public CustomMoLoader(string domain, string localeDir) : base(domain, localeDir) { }
        public CustomMoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator) : base(filePath, pluralRuleGenerator) { }
        public CustomMoLoader(string filePath, MoFileParser parser) : base(filePath, parser) { }
        public CustomMoLoader(string filePath) : base(filePath) { }
        public CustomMoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator) : base(moStream, pluralRuleGenerator) { }
        public CustomMoLoader(Stream moStream, MoFileParser parser) : base(moStream, parser) { }
        public CustomMoLoader(Stream moStream) : base(moStream) { }
    }
}