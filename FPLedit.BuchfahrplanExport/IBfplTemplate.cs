using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport
{
    public interface IBfplTemplate
    {
        string Name { get; }
        string GetTranformedText(Timetable tt);
    }
}
