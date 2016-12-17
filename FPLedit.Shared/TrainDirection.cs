using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public enum TrainDirection
    {
        ti, // false
        ta  // true
    }

    public static class TrainDirectionExt
    {
        public static void Set(this TrainDirection td, bool b)
        {
            td = b ? TrainDirection.ta : TrainDirection.ti;
        }

        public static bool Get(this TrainDirection td)
        {
            return td == TrainDirection.ta;
        }
    }
}
