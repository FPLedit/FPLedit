using System;

namespace FPLedit.Shared
{
    /// <summary>
    /// General type of the timetable file. This is based on the <see cref="TimetableVersion"/>.
    /// </summary>
    [Templating.TemplateSafe]
    public enum TimetableType
    {
        Linear,
        Network
    }

    public class TimetableTypeNotSupportedException : NotSupportedException
    {
        public TimetableTypeNotSupportedException(TimetableType type, string feature) 
            : base("Timetable file of type " + type + " does not support the following feature: " + feature)
        {
        }
    }
}
