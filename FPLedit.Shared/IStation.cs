using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IStation : IEntity
    {
        int Id { get; set; }

        string SName { get; set; }

        float LinearKilometre { get; set; }

        PositionCollection Positions { get; }

        int Wellenlinien { get; set; }

        string Vmax { get; set; }

        int[] Routes { get; set; }
    }
}
