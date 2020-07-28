using System.IO;
using NGettext;
using NGettext.Loaders;
using NGettext.Plural;

namespace FPLedit
{
    public class LocalizationHelper
    {
        public static ICatalog catalog;

        public static string _(string text) => catalog.GetString(text);
        public static string _(string text, params object[] args) => catalog.GetString(text, args);
    }

    internal class CustomMoLoader : MoLoader
    {
        protected override string GetFileName(string localeDir, string domain, string locale)
        {
            return Path.Combine(localeDir, domain + "." + locale + ".mo");
        }

        public CustomMoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(domain, localeDir, pluralRuleGenerator, parser)
        {
        }

        public CustomMoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(filePath, pluralRuleGenerator, parser)
        {
        }

        public CustomMoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator, MoFileParser parser) : base(moStream, pluralRuleGenerator, parser)
        {
        }

        public CustomMoLoader(string domain, string localeDir, IPluralRuleGenerator pluralRuleGenerator) : base(domain, localeDir, pluralRuleGenerator)
        {
        }

        public CustomMoLoader(string domain, string localeDir, MoFileParser parser) : base(domain, localeDir, parser)
        {
        }

        public CustomMoLoader(string domain, string localeDir) : base(domain, localeDir)
        {
        }

        public CustomMoLoader(string filePath, IPluralRuleGenerator pluralRuleGenerator) : base(filePath, pluralRuleGenerator)
        {
        }

        public CustomMoLoader(string filePath, MoFileParser parser) : base(filePath, parser)
        {
        }

        public CustomMoLoader(string filePath) : base(filePath)
        {
        }

        public CustomMoLoader(Stream moStream, IPluralRuleGenerator pluralRuleGenerator) : base(moStream, pluralRuleGenerator)
        {
        }

        public CustomMoLoader(Stream moStream, MoFileParser parser) : base(moStream, parser)
        {
        }

        public CustomMoLoader(Stream moStream) : base(moStream)
        {
        }
    }
}