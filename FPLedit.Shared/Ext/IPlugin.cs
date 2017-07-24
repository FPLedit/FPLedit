using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IPlugin
    {
        void Init(IInfo info);
    }
}
