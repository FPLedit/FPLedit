using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.UI.Validators
{
    public abstract class BaseValidator
    {
        private TextBox control;

        public BaseValidator(TextBox control, bool validateOnType, bool enableErrorColoring = true)
        {
            this.control = control;
            EnableErrorColoring = enableErrorColoring;
            ErrorColor = new Color(Colors.Red, 0.4f);

            if (validateOnType)
                Control.TextChanged += (s, e) => Validate();
            else
                Control.LostFocus += (s, e) => Validate();
        }

        public TextBox Control => control;

        public Color ErrorColor { get; set; }

        public bool EnableErrorColoring { get; set; }

        public bool Valid => IsValid();

        public string ErrorMessage { get; set; }

        private void Validate()
        {
            bool valid = IsValid();

            if (EnableErrorColoring)
                Control.BackgroundColor = valid ? Colors.White : ErrorColor;
            Control.ToolTip = valid ? null : ErrorMessage;
        }

        protected abstract bool IsValid();
    }
}
