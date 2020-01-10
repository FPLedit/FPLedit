using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public class UrlButton : Eto.Forms.LinkButton
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
