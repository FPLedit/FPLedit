using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.DefaultImplementations
{
    public abstract class BaseFilterRuleContainer
    {
        protected abstract IPatternSource GetProvider(Timetable tt);
        protected abstract IPatternSource CreateProvider(Timetable tt);

        public IEnumerable<FilterRule> LoadStationRules(Timetable tt) => Parse(GetProvider(tt)?.StationPatterns ?? "");

        public IEnumerable<FilterRule> LoadTrainRules(Timetable tt) => Parse(GetProvider(tt)?.TrainPatterns ?? "");

        private IEnumerable<FilterRule> Parse(string s)
        {
            string lastPattern = "";
            char last = '\0';
            bool hadLastEscape = false;
            var chars = s.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                var hle = hadLastEscape;
                hadLastEscape = false;
                if (last == '\\' && !hle) // Escape sequence handling, only if we had not an escape sequence the
                                          // last time ('\\' woulb be the second char of an escaped backslash)
                {
                    switch (chars[i])
                    {
                        case '\\':
                            lastPattern += '\\';
                            break;
                        case '|':
                            lastPattern += '|';
                            break;
                        default: // Lenient handling for unknown escape sequences.
                            lastPattern += '\\';
                            lastPattern += chars[i];
                            break;
                    }

                    hadLastEscape = true;
                }
                else if (chars[i] == '|' && !string.IsNullOrEmpty(lastPattern)) // Normal end sequence.
                {
                    yield return new FilterRule(lastPattern);
                    lastPattern = "";
                }
                else if (chars[i] != '\\') // prevent directly adding escape char.
                    lastPattern += chars[i];

                last = chars[i];
            }
            
            if (!string.IsNullOrEmpty(lastPattern))
                yield return new FilterRule(lastPattern);
        }

        private string Serialize(IEnumerable<FilterRule> rules)
        {
            var patterns = rules
                .Select(r => r.Pattern
                    .Replace("\\", "\\\\") // Escape backslashes.
                    .Replace("|", "\\|")); // Escape pipes.
            return string.Join("|", patterns);
        }

        public void SaveFilter(Timetable tt, IEnumerable<FilterRule> newStationRules, IEnumerable<FilterRule> newTrainRules)
        {
            IPatternSource attrs = GetProvider(tt) ?? CreateProvider(tt);
            attrs.TrainPatterns = Serialize(newTrainRules);
            attrs.StationPatterns = Serialize(newStationRules);
        }
    }

    public sealed class DefaultFilterRuleContainer : BaseFilterRuleContainer, IFilterRuleContainer
    {
        private readonly Func<Timetable, IPatternSource> getProvider, createProvider;
        public string DisplayName { get; }
        
        public DefaultFilterRuleContainer(string displayName, Func<Timetable, IPatternSource> getProvider, Func<Timetable,IPatternSource> createProvider)
        {
            this.getProvider = getProvider;
            this.createProvider = createProvider;
            DisplayName = displayName;
        }
        
        protected override IPatternSource GetProvider(Timetable tt) => getProvider(tt);

        protected override IPatternSource CreateProvider(Timetable tt) => createProvider(tt);
    }
}