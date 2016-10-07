using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Buchfahrplan.Shared.Validators
{
    public abstract class BaseValidator : Component
    {
        private TextBox control;
        private ErrorProvider provider;

        public BaseValidator()
        {
            provider = new ErrorProvider();
        }

        public TextBox Control {
            get { return control; }
            set
            {
                control = value;

                if (control != null && !DesignMode)
                {
                    control.Validating += Control_Validating;
                }
            }
        }

        public bool Valid { get { return IsValid(); } }

        public string ErrorMessage { get; set; }

        private void Control_Validating(object sender, CancelEventArgs e)
        {
            Validate();
        }

        private void Validate()
        {
            bool valid = IsValid();
            string msg = "";
            if (!valid)
                msg = ErrorMessage;
            provider.SetError(Control, msg);
        }

        internal abstract bool IsValid();
    }
}
