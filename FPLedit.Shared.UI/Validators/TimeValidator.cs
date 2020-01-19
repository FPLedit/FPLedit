using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.UI.Validators
{
    public class TimeValidator : BaseValidator
    {
        public bool AllowEmpty { get; set; }

        public TimeValidator(TextBox control, bool allowEmpty, bool enableErrorColoring = true, string errorMessage = null)
            : base(control, true, enableErrorColoring, errorMessage)
        {
            AllowEmpty = allowEmpty;
        }

        protected override bool IsValid()
        {
            if (AllowEmpty && Control.Text == "")
                return true;
            return TimeEntry.TryParse(Control.Text, out var ts) && ts <= new TimeEntry(24, 0);
        }
    }
}
