using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Eto;

namespace FPLedit.Shared.UI
{
    public static class EtoExtensions
    {
        private static SizeManager sizeManager;
        public static void Initialize(IPluginInterface pluginInterface)
        {
            sizeManager = new SizeManager(pluginInterface.Settings);
        }

        public static Stream GetResource(this Control dialog, string dotFilePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }

        public static void AddSizeStateHandler(this Window w) => sizeManager.Apply(w);

        public static void AddLegacyFilter(this FileDialog dialog, params string[] filters)
        {
            foreach (var filter in filters)
                dialog.AddLegacyFilter(filter);
        }

        public static void AddLegacyFilter(this FileDialog dialog, string filter)
        {
            var parts = filter.Split('|');
            for (int i = 0; i < parts.Length; i += 2)
            {
                var f = new FileFilter(parts[i], parts[i + 1]);
                dialog.Filters.Add(f);
            }
        }

        public static ButtonMenuItem CreateItem(this ISubmenu parent, string text, bool enabled = true, EventHandler<EventArgs> clickHandler = null)
        {
            var itm = new ButtonMenuItem
            {
                Text = text,
                Enabled = enabled,
            };
            if (clickHandler != null)
                itm.Click += clickHandler;
            parent.Items.Add(itm);
            return itm;
        }

        public static MenuItem GetItem(this ISubmenu parent, string text) => parent.Items.FirstOrDefault(i => i.Text == text);

        public static CheckMenuItem CreateCheckItem(this ISubmenu parent, string text, bool isChecked = false, EventHandler<EventArgs> changeHandler = null)
        {
            var itm = new CheckMenuItem
            {
                Text = text,
                Checked = isChecked,
            };
            if (changeHandler != null)
                itm.CheckedChanged += changeHandler;
            parent.Items.Add(itm);
            return itm;
        }

        public static void WordWrap(this Label label, int maxWidth)
        {
            // hack as eto currently somehow does not permit multi-line texts in xaml.
            var origLines = label.Text.Split(new [] {"\n", "\\n "}, StringSplitOptions.None);
            var lines = new List<string>();
            foreach (var origLine in origLines)
            {
                var words = origLine.Split(' ');
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
            }

            label.Text = string.Join(Environment.NewLine, lines);
        }
    }
}
