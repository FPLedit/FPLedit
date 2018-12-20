using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public static class EtoExtensions
    {
        private static SizeManager sizeManager;
        public static void Initialize(IInfo info)
        {
            sizeManager = new SizeManager(info.Settings);
        }

        public static Stream GetResource(this Window dialog, string dotFilePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }

        #region Close Handlers
        public static void AddCloseHandler(this Dialog dialog)
            => AddCloseHandler(dialog, dialog.DefaultButton, dialog.AbortButton);

        public static void AddCloseHandler(this Window dialog, Button accept, Button cancel)
            => new CloseHandler(dialog, accept, cancel);

        public static void NClose(this Window dialog) => CloseHandler.NClose(dialog);
        #endregion

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

        public static ButtonMenuItem CreateItem(this ISubmenu parent, string text)
        {
            var itm = new ButtonMenuItem();
            itm.Text = text;
            parent.Items.Add(itm);
            return itm;
        }

        public static CheckMenuItem CreateCheckItem(this ISubmenu parent, string text)
        {
            var itm = new CheckMenuItem();
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

        public static GridColumn AddColumn<T>(this GridView view, Expression<Func<T, string>> value, string header)
            => view.AddColumn(new TextBoxCell { Binding = Binding.Property(value) }, header);

        public static GridColumn AddColumn(this GridView view, Cell cell, string header)
        {
            var col = new GridColumn()
            {
                DataCell = cell,
                HeaderText = header,
                AutoSize = true,
                Sortable = false,
            };
            view.Columns.Add(col);
            return col;
        }
    }
}
