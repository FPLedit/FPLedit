using System;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public sealed class TimeEntryFactory
    {
        public bool AllowSeconds { get; set; }
        public TimeNormalizer Normalizer { get; }

        public TimeEntryFactory(bool allowSeconds)
        {
            AllowSeconds = allowSeconds;
            Normalizer = new TimeNormalizer(allowSeconds);
        }

        public bool TryParse(string s, out TimeEntry entry)
        {
            entry = new TimeEntry();
            var time = Normalizer.ParseTime(s, true);
            if (!time.HasValue)
                return false;
            entry = new TimeEntry(time.Value.hours, time.Value.minutes, time.Value.seconds, time.Value.decimals);
            return true;
        }
        
        public TimeEntry Parse(string s)
        {
            if (!TryParse(s, out var entry))
                throw new FormatException("TimeEntry Format not recognized");
            return entry;
        }
    }
}