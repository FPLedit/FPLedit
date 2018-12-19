using Eto.Forms;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public static class ExtBind
    {
        public static IIndirectBinding<string> ColorBinding(ColorCollection cc)
            => Binding.Property<string, string>(c => cc.ToName(ColorFormatter.FromHexString(c)));
    }
}
