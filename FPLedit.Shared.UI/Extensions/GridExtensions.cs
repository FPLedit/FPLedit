using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Eto;
using Eto.Forms;

namespace FPLedit.Shared.UI
{
    public static class GridExtensions
    {
        public static GridColumn AddColumn<T>(this GridView view, Expression<Func<T, string>> value, string header, bool editable = false)
            => view.AddColumn(new TextBoxCell { Binding = Binding.Property(value) }, header, editable);

        public static GridColumn AddFuncColumn<T>(this GridView view, Func<T, string> value, string header)
            => view.AddColumn(new TextBoxCell { Binding = Binding.Delegate(value) }, header);

        public static GridColumn AddColumn<T, TVal>(this GridView view, Expression<Func<T, TVal>> value, Func<TVal?, string> to, Func<string, TVal> from, string header, bool editable = false)
            => view.AddColumn(new TextBoxCell { Binding = Binding.Property(value).Convert(to, from) }, header, editable);

        public static GridColumn AddCheckColumn<T>(this GridView view, Expression<Func<T, bool>> value, string header, bool editable = false)
            => view.AddColumn(new CheckBoxCell { Binding = Binding.Property(value).Convert<bool?>(b => b, b => b.HasValue && b.Value), }, header, editable);

        public static GridColumn AddDropDownColumn<T>(this GridView view, Expression<Func<T, object>> value, IEnumerable<object> dataStore, string header, bool editable = false)
            => view.AddColumn(new ComboBoxCell { Binding = Binding.Property(value), DataStore = dataStore }, header, editable);

        public static GridColumn AddDropDownColumn<T, TEnum>(this GridView view, Expression<Func<T, TEnum>> value, string header, bool editable = false)
            where TEnum : Enum
        {
            var dataStore = Enum.GetNames(typeof(TEnum));
            var bind = Binding.Property(value).Convert<object>(e => e.ToString(), s => (TEnum) Enum.Parse(typeof(TEnum), (string) s));
            return view.AddColumn(new ComboBoxCell { Binding = bind, DataStore = dataStore }, header, editable);
        }

        public static GridColumn AddDropDownColumn<T>(this GridView view, Expression<Func<T, object?>> value, IEnumerable<object> dataStore, IIndirectBinding<string> textBinding, string header, bool editable = false)
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
    }
}