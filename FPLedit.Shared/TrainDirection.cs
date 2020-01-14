using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public enum TrainDirection
    {
        ti, // train in
        ta,  // train against
        tr // Allgemeiner Zug im Graph
    }
}
