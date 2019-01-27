using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    public interface ITimetableInitAction
    {
        void Init(Timetable tt);
    }
}
