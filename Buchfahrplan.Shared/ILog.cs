using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface ILog
    {
        void Error(string message);

        void Warning(string message);

        void Info(string message);
    }
}
