using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.UI.Validators
{
    public sealed class NotEmptyValidator : BaseValidator
    {
        public NotEmptyValidator(TextBox control, bool enableErrorColoring = true) : base(control, true, enableErrorColoring)
        {
        }

        protected override bool IsValid()
        {
            return Control.Text != "";
        }
    }
}
