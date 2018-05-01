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
        public static Stream GetResource(this Dialog<DialogResult> dialog, string dotFilePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }

        public static Stream GetResource(this Form dialog, string dotFilePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }

        public static void AddLegacyFilter(this OpenFileDialog ofd, string filter)
        {
            var parts = filter.Split('|');
            var f = new FileFilter(parts[0], parts[1]);
            ofd.Filters.Add(f);
        }

        public static void AddLegacyFilter(this SaveFileDialog sfd, string filter)
        {
            var parts = filter.Split('|');
            var f = new FileFilter(parts[0], parts[1]);
            sfd.Filters.Add(f);
        }

        public static ButtonMenuItem CreateItem(this MenuBar menu, string text)
        {
            var itm = new ButtonMenuItem();
            itm.Text = text;
            menu.Items.Add(itm);
            return itm;
        }

        public static ButtonMenuItem CreateItem(this ButtonMenuItem parent, string text)
        {
            var itm = new ButtonMenuItem();
            itm.Text = text;
            parent.Items.Add(itm);
            return itm;
        }
    }
}
