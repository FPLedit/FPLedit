using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public static class FontCollection
    {
        public static string[] Families { get; private set; } = new string[0];

        public static void InitAsync()
        {
            new Task(() =>
            {
                Families = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
            }).Start();
        }
    }
}
