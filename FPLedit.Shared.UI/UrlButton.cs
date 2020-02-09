using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared.UI
{
    public sealed class UrlButton : Eto.Forms.LinkButton
    {
        public string Url { get; set; }

        public bool AllowNonHttpProtocols { get; set; } = false;

        protected override void OnClick(EventArgs e)
        {
            if (!string.IsNullOrEmpty(Url))
            {
                var url = new Uri(Url);
                if (url.Scheme.StartsWith("http") || AllowNonHttpProtocols)
                    OpenHelper.Open(Url);
            }
            base.OnClick(e);
        }
    }
}
