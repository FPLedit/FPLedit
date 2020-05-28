using Eto.Forms;

namespace FPLedit.Shared.UI.Validators
{
    public sealed class NumberValidator : BaseValidator
    {
        public bool AllowEmpty { get; set; }

        public bool UseInt { get; set; }
        
        public bool AllowNegative { get; set; }

        public NumberValidator(TextBox control, bool allowEmpty, bool useInt, bool allowNegative = true, bool enableErrorColoring = true, string? errorMessage = null)
            : base(control, true, enableErrorColoring, errorMessage)
        {
            AllowEmpty = allowEmpty;
            UseInt = useInt;
            AllowNegative = allowNegative;
        }

        protected override bool IsValid()
        {
            if (AllowEmpty && Control.Text == "")
                return true;
            if (!UseInt)
                return float.TryParse(Control.Text, out var valFloat) && (AllowNegative || valFloat >= 0);
            return int.TryParse(Control.Text, out var valInt) && (AllowNegative || valInt >= 0);
        }
    }
}
