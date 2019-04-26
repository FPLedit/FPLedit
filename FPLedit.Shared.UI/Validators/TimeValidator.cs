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

        public TimeValidator(TextBox control, bool allowEmpty, bool enableErrorColoring = true) : base(control, true, enableErrorColoring)
        {
            AllowEmpty = allowEmpty;
        }

        protected override bool IsValid()
        {
            if (AllowEmpty && Control.Text == "")
                return true;
            return TimeSpan.TryParse(Control.Text.Replace("24:", "1.00:"), out var ts) && ts < new TimeSpan(1, 0, 0, 1);
        }
    }
}
