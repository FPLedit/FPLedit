using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
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
