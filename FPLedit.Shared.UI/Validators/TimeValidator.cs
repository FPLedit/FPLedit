using Eto.Forms;

namespace FPLedit.Shared.UI.Validators
{
    public class TimeValidator : BaseValidator
    {
        private readonly TimeEntryFactory timeFactory;
        public bool AllowEmpty { get; set; }

        private readonly TimeEntry maximum;

        public TimeValidator(TextBox control, bool allowEmpty, TimeEntryFactory timeFactory, bool enableErrorColoring = true, string? errorMessage = null, TimeEntry? maximum = null)
            : base(control, true, enableErrorColoring, errorMessage)
        {
            this.timeFactory = timeFactory;
            AllowEmpty = allowEmpty;
            this.maximum = maximum ?? new TimeEntry(24, 0);
        }

        protected override bool IsValid()
        {
            if (AllowEmpty && Control.Text == "")
                return true;
            return timeFactory.TryParse(Control.Text, out var ts) && ts <= maximum;
        }
    }
}
