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

#pragma warning disable IDE0060 // Nicht verwendete Parameter entfernen
        public static Stream GetResource(this Control dialog, string dotFilePath)
        {
            var assembly = Assembly.GetCallingAssembly();
            return assembly.GetManifestResourceStream("FPLedit." + dotFilePath);
        }
#pragma warning restore IDE0060 // Nicht verwendete Parameter entfernen

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
            var origLines = label.Text.Split(new [] {"\n", "\\n"}, StringSplitOptions.None);
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

        public static GridColumn AddDropDownColumn<T>(this GridView view, Expression<Func<T, object>> value, IEnumerable<object> dataStore, IIndirectBinding<string> textBinding, string header, bool editable = false)
        {
            var internalBinding = Binding.Property(value);
            Cell cell;

            // hack for eto/gtk not supporting ItemTextBinding on ComboBoxCells
            if (Platform.Instance.IsGtk)
            {
                const char zws = '\u200B'; // This is a zero width (ZWS) space Unicode character.
                
                var lDataStore = dataStore.ToList();
                
                var tDataStore = lDataStore.Select(i => 
                    string.Empty.PadLeft(lDataStore.IndexOf(i) + 1, zws) // Pad with index+1 ZWS chars to be able to check later. If no ZWS is there, this did not work.
                    + textBinding.GetValue(i))
                    .ToList();
                var hasNonUnique = tDataStore.Distinct().Count() != tDataStore.Count; // Check if textBinding produced the same string for more than one data item.
                
                var binding = Binding.Delegate<T, string>(
                    (T s) => textBinding.GetValue(internalBinding.GetValue(s)),
                    (T t, string s) =>
                    {
                        int idx = s.Split(zws).Length - 2;
                        if (idx == -1)
                        {
                            idx = tDataStore.IndexOf(s); // Fallback if ZWS is not supported.
                            if (hasNonUnique)
                                throw new Exception("ComboBoxCell ComboTextBinding Polyfill: Duplicate text entry encountered and Zero-Width-Space Hack not supported by platform!");
                        }

                        internalBinding.SetValue(t, lDataStore[idx]);
                    }).Cast<object>();

                cell = new ComboBoxCell { Binding = binding, DataStore = tDataStore };
            }
            else
                cell = new ComboBoxCell { Binding = internalBinding, DataStore = dataStore, ComboTextBinding = textBinding };
            
            return view.AddColumn(cell, header, editable);
        }

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
