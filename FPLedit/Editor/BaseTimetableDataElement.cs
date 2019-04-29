using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor
{
    internal abstract class BaseTimetableDataElement
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
        private readonly List<ErrorEntry> errors = new List<ErrorEntry>();

        public bool HasError(Station sta, bool arrival)
        {
            var err = errors.FirstOrDefault(e => e.Station == sta && e.Arrival == arrival);
            return err != null && !string.IsNullOrEmpty(err.Text);
        }

        public bool HasAnyError => errors.Any(e => !string.IsNullOrEmpty(e.Text));

        public void SetError(Station sta, bool arrival, string text)
        {
            var err = errors.FirstOrDefault(e => e.Station == sta && e.Arrival == arrival);
            if (text == null || text == "")
            {
                if (err != null)
                    errors.Remove(err);
                return;
            }
            if (err == null)
            {
                err = new ErrorEntry(sta, arrival, null);
                errors.Add(err);
            }
            err.Arrival = arrival;
            err.Text = text;
        }

        internal string GetErrorText(Station sta, bool arrival)
            => errors.FirstOrDefault(e => e.Station == sta && e.Arrival == arrival)?.Text;
        #endregion

        public bool IsLast(Station sta) => Train.GetPath().Last() == sta;

        public bool IsFirst(Station sta) => Train.GetPath().First() == sta;

        public abstract Station GetStation();

        private class ErrorEntry
        {
            public Station Station;
            public bool Arrival;
            public string Text;

            public ErrorEntry(Station station, bool arrival, string text)
            {
                Station = station;
                Arrival = arrival;
                Text = text;
            }
        }
    }
}
