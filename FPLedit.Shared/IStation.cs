using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public interface IStation : IEntity
    {
        int Id { get; set; }

        string SName { get; set; }

        PositionCollection Positions { get; }

        RouteValueCollection<int> Wellenlinien { get;}

        RouteValueCollection<string> Vmax { get; }

        int[] Routes { get; set; }
    }
}
