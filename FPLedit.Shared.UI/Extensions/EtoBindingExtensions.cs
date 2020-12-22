using Eto.Forms;
using FPLedit.Shared.Rendering;
using System;
using System.Globalization;
using System.Linq.Expressions;

namespace FPLedit.Shared.UI
{
    public static class EtoBindingExtensions
    {
        public static IIndirectBinding<string> ColorBinding(ColorCollection cc)
            => Binding.Delegate<string, string>(s =>
            {
                var c = ColorFormatter.FromHexString(s) ?? throw new Exception($"Encountered invalid hex string \"{s}\" in color conversion!");
                return cc.ToName(c);
            });

        public static void AddIntConvBinding<TValue, T1>(this BindableBinding<T1, string> binding, Func<TValue, int> property)
            where T1 : IBindable
        {
            var shadowBinding = Binding.Delegate(property);

            binding.BindDataContext<TValue>(s => shadowBinding.GetValue(s).ToString(), (s, str) =>
            {
                if (int.TryParse(str, out int i))
                    shadowBinding.SetValue(s, i);
            });
        }

        public static void AddFloatConvBinding<TValue, T1>(this BindableBinding<T1, string> binding, Func<TValue, float> property)
            where T1 : IBindable
        {
            var shadowBinding = Binding.Delegate(property);

            binding.BindDataContext<TValue>(s => shadowBinding.GetValue(s).ToString(CultureInfo.CurrentCulture), (s, str) =>
            {
                if (float.TryParse(str, out float i))
                    shadowBinding.SetValue(s, i);
            });
        }

        public static void AddTimeEntryConvBinding<TValue, T1>(this BindableBinding<T1, string> binding, Expression<Func<TValue, TimeEntry>> property, TimeEntryFactory timeFactory)
            where T1 : IBindable
        {
            string convFromTs(TimeEntry ts) => ts.ToTimeString();
            TimeEntry convToTs(string s)
            {
                timeFactory.TryParse(s.Replace("24:", "1.00:"), out var ts);
                return ts;
            }

            binding.Convert(convToTs, convFromTs).BindDataContext(property);
        }
    }
}
