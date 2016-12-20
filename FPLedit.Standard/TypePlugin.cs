using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Standard
{
    public class TypePlugin : IPlugin
    {
        public string Name
        {
            get
            {
                return "Standard-Dateitypen";
            }
        }

        public void Init(IInfo info)
        {
            info.RegisterImport(new JTrainGraphImport());
            info.RegisterExport(new JTrainGraphExport());
        }
    }
}
