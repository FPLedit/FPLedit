using Eto.Forms;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public class DropDownBind
    {
        public static void Color<T>(ISettings settings, DropDown dropDown, string property)
        {
            var cc = new ColorCollection(settings);

            var p = GetProperty<T>(property);
            dropDown.DataStore = cc.ColorHexStrings;
            dropDown.ItemTextBinding = ExtBind.ColorBinding(cc);
            dropDown.SelectedValueBinding.BindDataContext<T>(a => ColorFormatter.ToString((MColor)p.GetValue(a)),
                (a, val) => p.SetValue(a, ColorFormatter.FromHexString((string)val)));
        }

        public static void Width<T>(DropDown dropDown, string property)
        {
            var lineWidths = Enumerable.Range(1, 5).Cast<object>().ToArray();

            var p = GetProperty<T>(property);
            dropDown.DataStore = lineWidths;
            dropDown.SelectedValueBinding.BindDataContext<T>(a => (int)p.GetValue(a),
                (a, val) => p.SetValue(a, (int)val));
        }

        public static void Font<T>(DropDown familyDropDown, DropDown sizeDropDown, string property)
        {
            var fontSizes = Enumerable.Range(5, 15).Cast<object>().ToArray();

            var p = GetProperty<T>(property);
            Func<T, MFont> f = a => (MFont)p.GetValue(a);

            familyDropDown.DataStore = new string[] { "<Lade>" };
            familyDropDown.SelectedIndex = 0;

            // Asynchrones Laden der Font-Liste, um Performance-Problemen vorzubeugen
            Application.Instance.AsyncInvoke(() =>
            {
                familyDropDown.DataStore = FontCollection.Families;
                familyDropDown.SelectedValueBinding.BindDataContext<T>(
                    a => f(a).Family,
                    (a, val) => { var x = f(a); x.Family = (string)val; p.SetValue(a, x); });
            });

            sizeDropDown.DataStore = fontSizes;
            sizeDropDown.SelectedValueBinding.BindDataContext<T>(
                a => f(a).Size,
                (a, val) => { var x = f(a); x.Size = (int)val; p.SetValue(a, x); });
        }

        public static void Enum<T, TEnum>(DropDown dropDown, string property, Dictionary<TEnum, string> display) where TEnum : Enum
        {
            var p = GetProperty<T>(property);
            dropDown.DataStore = display.Keys.Cast<object>();
            dropDown.ItemTextBinding = Binding.Property<TEnum, string>(s => display[s]);
            dropDown.SelectedValueBinding.BindDataContext<T>(s => (TEnum)p.GetValue(s), (s, v) => p.SetValue(s, v));
        }

        private static PropertyInfo GetProperty<T>(string property)
            => typeof(T).GetProperty(property);
    }
}
