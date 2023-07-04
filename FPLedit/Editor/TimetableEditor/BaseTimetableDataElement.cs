#nullable enable
using Eto.Forms;
using FPLedit.Shared;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor.TimetableEditor
{
    internal abstract class BaseTimetableDataElement
    {
        /// <summary>
        /// HACK for unixy systems that do not support directly editing in cells ("MP Mode" = "Multiplatform Mode").
        /// A last line has to be added that has no contents (but is not null). This is accomplished by creating a
        /// DataElement with IsMpDummy = true.
        /// </summary>
        public bool IsMpDummy { get; protected init; }

        public ITrain? Train { get; init; }

        public Dictionary<Station, ArrDep>? ArrDeps { get; init; }

        public bool IsSelectedArrival { get; set; }

        public TextBox? SelectedTextBox { get; set; }
        
        public DropDown? SelectedDropDown { get; set; }

        public void SetTime(Station sta, bool arrival, string time)
        {
            var a = ArrDeps![sta];
            if (arrival)
                a.Arrival = TimeEntry.Parse(time);
            else
                a.Departure = TimeEntry.Parse(time);
            ArrDeps[sta] = a;
        }

        public void SetZlm(Station sta, string zlm)
        {
            var a = ArrDeps![sta];
            a.Zuglaufmeldung = zlm;
            ArrDeps[sta] = a;
        }

        public void SetTrapez(Station sta, bool trapez)
        {
            var a = ArrDeps![sta];
            a.TrapeztafelHalt = trapez;
            ArrDeps[sta] = a;
        }

        #region Errors
        private readonly List<ErrorEntry> errors = new();

        public bool HasError(Station sta, bool arrival)
        {
            var err = errors.FirstOrDefault(e => e.Station == sta && e.Arrival == arrival);
            return err != null && !string.IsNullOrEmpty(err.Text);
        }

        public bool HasAnyError => errors.Any(e => !string.IsNullOrEmpty(e.Text));

        public void SetError(Station sta, bool arrival, string? text)
        {
            var err = errors.FirstOrDefault(e => e.Station == sta && e.Arrival == arrival);
            if (string.IsNullOrEmpty(text))
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
            => errors.FirstOrDefault(e => e.Station == sta && e.Arrival == arrival)?.Text ?? "";
        #endregion

        public bool IsLast(Station sta) => Train!.GetPath().Last() == sta;

        public bool IsFirst(Station sta) => Train!.GetPath().First() == sta;

        public abstract Station? GetStation();

        private class ErrorEntry
        {
            public readonly Station Station;
            public bool Arrival;
            public string? Text;

            public ErrorEntry(Station station, bool arrival, string? text)
            {
                Station = station;
                Arrival = arrival;
                Text = text;
            }
        }
    }
    
    /// <summary>
    /// A CCCO (=CustomCellControlObject) is hardwired to a single control. It contains information that is not dependant
    /// on the edited DataElement.
    /// </summary>
    internal class CCCO
    {
        public bool InhibitEvents { get; set; } = true;
        public BaseTimetableDataElement Data { get; set; } = null!;
    }
}
