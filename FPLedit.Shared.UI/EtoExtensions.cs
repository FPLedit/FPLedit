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
        public static void Initialize(IPluginInterface pluginInterface)
        {
            sizeManager = new SizeManager(pluginInterface.Settings);
        }

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        public static Stream GetResource(this Control dialog, string dotFilePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen

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
            var origLines = label.Text.Split('\n');
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

        #region Grid Columns

        public static GridColumn AddColumn<T>(this GridView view, Expression<Func<T, string>> value, string header, bool editable = false)
            => view.AddColumn(new TextBoxCell { Binding = Binding.Property(value) }, header, editable);

        public static GridColumn AddColumn<T, TVal>(this GridView view, Expression<Func<T, TVal>> value, Func<TVal, string> to, Func<string, TVal> from, string header, bool editable = false)
            => view.AddColumn(new TextBoxCell { Binding = Binding.Property(value).Convert(to, from) }, header, editable);

        public static GridColumn AddCheckColumn<T>(this GridView view, Expression<Func<T, bool>> value, string header, bool editable = false)
            => view.AddColumn(new CheckBoxCell { Binding = Binding.Property(value).Convert<bool?>(b => b, b => b.HasValue && b.Value), }, header, editable);

        public static GridColumn AddDropDownColumn<T>(this GridView view, Expression<Func<T, object>> value, IEnumerable<object> dataStore, string header, bool editable = false)
            => view.AddColumn(new ComboBoxCell { Binding = Binding.Property(value), DataStore = dataStore }, header, editable);

        public static GridColumn AddColumn(this GridView view, Cell cell, string header, bool editable = false)
        {
            var col = new GridColumn()
            {
                DataCell = cell,
                HeaderText = header,
                AutoSize = true,
                Sortable = false,
                Editable = editable,
            };
            view.Columns.Add(col);
            return col;
        }

        #endregion
    }
}
