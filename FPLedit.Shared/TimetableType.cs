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
        
        public TimetableTypeNotSupportedException(TimetableType type, string feature, Exception innerException) 
            : base("Timetable file of type " + type + " does not support the following feature: " + feature, innerException)
        {
        }
        
        public TimetableTypeNotSupportedException(string feature) 
            : base("Current timetable file does not support a required feature: " + feature)
        {
        }
        
        public TimetableTypeNotSupportedException(string feature, Exception innerEexception) 
            : base("Current timetable file does not support a required feature: " + feature, innerEexception)
        {
        }

        public TimetableTypeNotSupportedException()
            : base("Current timetable file does not support a required feature.")
        {
        }
    }
}
