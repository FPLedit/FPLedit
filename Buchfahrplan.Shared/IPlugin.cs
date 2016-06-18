using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Shared
{
    public interface IPlugin
    {
        void Init(IInfo info);
    }
}
