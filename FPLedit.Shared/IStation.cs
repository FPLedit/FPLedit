using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IStation : IEntity
    {
        string SName { get; set; }

        float Kilometre { get; set; }

        int Wellenlinien { get; set; }

        string Vmax { get; set; }
    }
}
