using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Validators
{
    //[System.Security.SecurityCritical]
    public sealed class NotEmptyValidator : BaseValidator
    {
        public NotEmptyValidator(TextBox control) : base(control)
        {
        }

        internal override bool IsValid()
        {
            return Control.Text != "";
        }
    }
}
