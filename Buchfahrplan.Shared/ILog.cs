using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Shared
{
    public interface ILog
    {
        void Error(string message);

        void Warning(string message);

        void Info(string message);
    }
}
