using Eto.Forms;

namespace FPLedit.Shared.UI.Validators;

public class TimeValidator : BaseValidator
{
    public bool AllowEmpty { get; set; }

    private readonly TimeEntry maximum;

    public TimeValidator(TextBox control, bool allowEmpty, bool enableErrorColoring = true, string? errorMessage = null, TimeEntry? maximum = null)
        : base(control, true, enableErrorColoring, errorMessage)
    {
        AllowEmpty = allowEmpty;
        this.maximum = maximum ?? new TimeEntry(24, 0);
    }

    protected override bool IsValid()
    {
        if (AllowEmpty && Control.Text == "")
            return true;
        return TimeEntry.TryParse(Control.Text, out var ts) && ts <= maximum;
    }
}