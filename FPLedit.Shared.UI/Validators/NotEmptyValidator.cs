using Eto.Forms;

namespace FPLedit.Shared.UI.Validators;

public class NotEmptyValidator : BaseValidator
{
    public NotEmptyValidator(TextBox control, bool enableErrorColoring = true, string? errorMessage = null)
        : base(control, true, enableErrorColoring, errorMessage)
    {
    }

    protected override bool IsValid() => Control.Text != "";
}