using System.Text.RegularExpressions;
using Eto.Forms;

namespace FPLedit.Shared.UI.Validators;

public class RegexValidator : BaseValidator
{
    public bool AllowEmpty { get; set; }

    private readonly Regex regex;

    public RegexValidator(TextBox control, Regex regex, bool allowEmpty, bool enableErrorColoring = true, string? errorMessage = null)
        : base(control, true, enableErrorColoring, errorMessage)
    {
        AllowEmpty = allowEmpty;
        this.regex = regex;
    }

    protected override bool IsValid()
    {
        if (AllowEmpty && Control.Text == "")
            return true;
        return regex.IsMatch(Control.Text);
    }
}