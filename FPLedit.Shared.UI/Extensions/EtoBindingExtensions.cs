using Eto.Forms;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public static class EtoBindingExtensions
    {
        public static IIndirectBinding<string> ColorBinding(ColorCollection cc)
            => Binding.Delegate<string, string>(c => cc.ToName(ColorFormatter.FromHexString(c)));

        public static void AddIntConvBinding<TValue, T1>(this BindableBinding<T1, string> binding, Expression<Func<TValue, int>> property)
            where T1 : IBindable
        {
            var shadowBinding = Binding.Property(property);

            binding.BindDataContext<TValue>(s => shadowBinding.GetValue(s).ToString(), (s, str) =>
            {
                if (int.TryParse(str, out int i))
                    shadowBinding.SetValue(s, i);
            });
        }

        public static void AddFloatConvBinding<TValue, T1>(this BindableBinding<T1, string> binding, Expression<Func<TValue, float>> property)
            where T1 : IBindable
        {
            var shadowBinding = Binding.Property(property);

            binding.BindDataContext<TValue>(s => shadowBinding.GetValue(s).ToString(), (s, str) =>
            {
                if (float.TryParse(str, out float i))
                    shadowBinding.SetValue(s, i);
            });
        }

        public static void AddTimeEntryConvBinding<TValue, T1>(this BindableBinding<T1, string> binding, Expression<Func<TValue, TimeEntry>> property)
            where T1 : IBindable
        {
            string convFromTs(TimeEntry ts) => ts.ToShortTimeString();
            TimeEntry convToTs(string s)
            {
                TimeEntry.TryParse(s.Replace("24:", "1.00:"), out var ts);
                return ts;
            };

            binding.Convert(convToTs, convFromTs).BindDataContext(property);
        }
    }
}
