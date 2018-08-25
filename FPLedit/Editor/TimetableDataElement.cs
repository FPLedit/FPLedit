using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor
{
    internal abstract class TimetableDataElement
    {
        public Train Train { get; set; }

        public Dictionary<Station, ArrDep> ArrDeps { get; set; }

        public bool IsSelectedArrival { get; set; }

        public TextBox SelectedTextBox { get; set; }

        public void SetTime(Station sta, bool arrival, string time)
        {
            var a = ArrDeps[sta];
            if (arrival)
                a.Arrival = TimeSpan.Parse(time);
            else
                a.Departure = TimeSpan.Parse(time);
            ArrDeps[sta] = a;
        }

        public void SetZlm(Station sta, string zlm)
        {
            var a = ArrDeps[sta];
            a.Zuglaufmeldung = zlm;
            ArrDeps[sta] = a;
        }

        public void SetTrapez(Station sta, bool trapez)
        {
            var a = ArrDeps[sta];
            a.TrapeztafelHalt = trapez;
            ArrDeps[sta] = a;
        }

        #region Errors
        private Dictionary<Station, bool?> Errors { get; set; } = new Dictionary<Station, bool?>();

        public bool HasError(Station sta, bool arrival)
            => Errors.TryGetValue(sta, out bool? val) && val.HasValue && val.Value == arrival;

        public bool HasAnyError => Errors.Any(e => e.Value.HasValue);

        public void SetError(Station sta, bool? arrival) => Errors[sta] = arrival;
        #endregion

        public bool IsLast(Station sta) => Train.GetPath().Last() == sta;

        public bool IsFirst(Station sta) => Train.GetPath().First() == sta;

        public abstract Station GetStation();
    }
}
