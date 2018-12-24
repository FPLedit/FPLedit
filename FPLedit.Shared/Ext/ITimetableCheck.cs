using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    public interface ITimetableCheck
    {
        void Check(Timetable tt, ILog log);
    }
}
