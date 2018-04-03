using Eto.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Validators
{
    //[System.Security.SecurityCritical]
    public abstract class BaseValidator
    {
        private TextBox control;

        public BaseValidator(TextBox control)
        {
            this.control = control;
        }

        public TextBox Control => control;

        public bool Valid => IsValid();

        public string ErrorMessage { get; set; }

        private void Control_Validating(object sender, EventArgs e)
            => Validate();

        private void Validate()
        {
            bool valid = IsValid();
            string msg = "";
            if (!valid)
                msg = ErrorMessage;
        }

        internal abstract bool IsValid();
    }
}
