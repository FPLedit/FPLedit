using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.AushangfahrplanExport
{
    public interface IAfplTemplate
    {
        string Name { get; }
        string GetTranformedText(Timetable tt);
    }
}
