using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.BfplImport
{
    [Plugin("Importer für alte BFPL-Dateien", "1.5.0", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IInfo info)
            => info.Register<IImport>(new BfplImport());
    }
}
