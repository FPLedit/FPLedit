using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Aushangfahrplan
{
    public interface IAfplTemplate
    {
        string Name { get; }
        string GetResult(Timetable tt);
    }
}
