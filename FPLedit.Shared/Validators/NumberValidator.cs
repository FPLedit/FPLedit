using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Validators
{
    public sealed class NumberValidator : BaseValidator
    {
        public bool AllowEmpty { get; set; }

        public bool UseInt { get; set; }

        public NumberValidator() : base()
        {
        }

        internal override bool IsValid()
        {
            if (AllowEmpty && Control.Text == "")
                return true;
            if (!UseInt)
                return float.TryParse(Control.Text, out var f);
            return int.TryParse(Control.Text, out var i);
        }
    }
}
