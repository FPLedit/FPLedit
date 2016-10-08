using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Validators
{
    public sealed class NumberValidator : BaseValidator
    {
        public NumberValidator() : base()
        {
        }

        internal override bool IsValid()
        {
            float num;
            return float.TryParse(Control.Text, out num);
        }
    }
}
