using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Validators
{
    public sealed class NotEmptyValidator : BaseValidator
    {
        public NotEmptyValidator() : base()
        {
        }

        internal override bool IsValid()
        {
            return Control.Text != "";
        }
    }
}
