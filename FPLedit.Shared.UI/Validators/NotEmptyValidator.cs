using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.UI.Validators
{
    public sealed class NotEmptyValidator : BaseValidator
    {
        public NotEmptyValidator(TextBox control, bool enableErrorColoring = true, string errorMessage = null)
            : base(control, true, enableErrorColoring, errorMessage)
        {
        }

        protected override bool IsValid() => Control.Text != "";
    }
}
