using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NGettext;
using NGettext.Loaders;
using NGettext.Plural;

namespace FPLedit.Shared
{
    /// <summary>
    /// Helper class for localization / translation.
    /// </summary>
    public static class T
    {
        private const string DEFAULT_LOCALE = "de-DE";
        private const string DEFAULT_LOCALE_NAME = "Deutsch";
        
        private static string currentLocale = DEFAULT_LOCALE;
        private static string localeDir = ".";
        private static Dictionary<string, ICatalog> catalogs = new Dictionary<string, ICatalog>();

        public static void SetLocale(string localeRoot, string locale)
        {
            currentLocale = locale;
            localeDir = localeRoot;
            catalogs = new Dictionary<string, ICatalog>();
        }

        public static string _(string text)
        {
            var assembly = Assembly.GetCallingAssembly();
            var catalog = GetCatalog(assembly);
            return catalog.GetString(text);
        }
        public static string _(string text, params object[] args)
        {
            var assembly = Assembly.GetCallingAssembly();
            var catalog = GetCatalog(assembly);
            return catalog.GetString(text, args);
        }
        
        public static string _a(Assembly assembly, string text)
        {
            var catalog = GetCatalog(assembly);
            return catalog.GetString(text);
        }
        public static string _a(Assembly assembly, string text, params object[] args)
        {
            var catalog = GetCatalog(assembly);
            return catalog.GetString(text, args);
        }

        private static ICatalog GetCatalog(Assembly assembly)
        {
            var name = assembly.GetName().Name;
            if (!catalogs.TryGetValue(name, out var catalog))
            {
                catalog = new Catalog(new CustomMoLoader(name, localeDir), new CultureInfo(currentLocale));
                catalogs[name] = catalog;
            }
            return catalog;
        }

        public static Dictionary<string, string> GetAvailableLocales()
        {
            var ret = new Dictionary<string, string>();
            ret.Add(DEFAULT_LOCALE, DEFAULT_LOCALE_NAME);
            
            var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            var dir = new DirectoryInfo(localeDir);
            var files = dir.GetFiles("*.mo");
            foreach (var file in files)
            {
                var parts = file.Name.Split('.');
                if (parts.Length < 3)
                    continue;
                var locale = parts[parts.Length - 2].Replace('_','-');
                var valid = allCultures.FirstOrDefault(culture => string.Equals(culture.Name, locale, StringComparison.CurrentCultureIgnoreCase));
                if (valid == null)
                    continue;
                if (!ret.ContainsKey(locale))
                    ret[locale] = valid.NativeName;
            }

            return ret;
        }

        public static string GetCurrentLocale() => currentLocale;
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