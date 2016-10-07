using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Standard.Validators
{
    internal sealed class NotEmptyValidator : BaseValidator
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
