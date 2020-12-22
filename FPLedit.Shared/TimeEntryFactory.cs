using System;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public sealed class TimeEntryFactory
    {
        public TimeNormalizer Normalizer { get; }

        public TimeEntryFactory()
        {
            Normalizer = new TimeNormalizer();
        }

        
    }
}