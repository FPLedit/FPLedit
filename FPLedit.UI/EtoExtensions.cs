using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public static class EtoExtensions
    {
        public static Stream GetResource(this Window dialog, string dotFilePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }

        public static void AddLegacyFilter(this FileDialog dialog, string filter)
        {
            var parts = filter.Split('|');
            var f = new FileFilter(parts[0], parts[1]);
            dialog.Filters.Add(f);
        }

        public static ButtonMenuItem CreateItem(this ISubmenu parent, string text)
        {
            var itm = new ButtonMenuItem();
            itm.Text = text;
            parent.Items.Add(itm);
            return itm;
        }

        public static void WordWrap(this Label label, int maxWidth)
        {
            var lines = new List<string>();
            var words = label.Text.Split(' ');
            var line = "";
            for (int i = 0; i < words.Length; i++)
            {
                var nline = line + words[i] + " ";
                if (label.Font.MeasureString(nline.Substring(0, nline.Length - 1)).Width > maxWidth)
                {
                    lines.Add(line);
                    line = words[i] + " ";
                }
                else
                    line = nline;
            }
            lines.Add(line);
            label.Text = string.Join(Environment.NewLine, lines);
        }
    }
}
