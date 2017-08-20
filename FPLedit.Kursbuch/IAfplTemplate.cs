using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Kursbuch
{
    public interface IKfplTemplate
    {
        string Name { get; }
        string GetResult(Timetable tt);
    }
}
