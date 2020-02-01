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
        private readonly Color defaultColor;
        
        public TextBox Control { get; }

        public Color ErrorColor { get; }

        public bool EnableErrorColoring { get; }

        public string ErrorMessage { get; }
        
        public bool Valid => IsValid();

        public BaseValidator(TextBox control, bool validateOnType, bool enableErrorColoring = true, string errorMessage = null)
        {
            Control = control;
            defaultColor = control.BackgroundColor;
            EnableErrorColoring = enableErrorColoring;
            ErrorColor = new Color(Colors.Red, 0.4f);
            ErrorMessage = errorMessage;

            if (validateOnType)
                Control.TextChanged += (s, e) => Validate();
            else
                Control.LostFocus += (s, e) => Validate();
        }

        private void Validate()
        {
            var valid = IsValid();

            if (EnableErrorColoring)
                Control.BackgroundColor = valid ? defaultColor : ErrorColor;
            Control.ToolTip = valid ? null : ErrorMessage;
        }

        protected abstract bool IsValid();
    }
}
