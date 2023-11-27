using Eto;
using Eto.Drawing;
using Eto.Forms;

namespace FPLedit.Shared.UI.Validators;

public abstract class BaseValidator
{
    private readonly Color defaultColor;

    public TextBox Control { get; }

    public Color ErrorColor { get; }

    public bool EnableErrorColoring { get; }

    public string? ErrorMessage { get; }

    public bool Enabled { get; set; } = true;

    public bool Valid => !Enabled || IsValid();

    protected BaseValidator(TextBox control, bool validateOnType, bool enableErrorColoring = true, string? errorMessage = null)
    {
        Control = control;
        //TODO: find a better solution for WPF background color
        defaultColor = Platform.Instance.IsWpf ? Colors.White : control.BackgroundColor;
        EnableErrorColoring = enableErrorColoring;
        ErrorColor = new Color(Colors.Red, 0.4f);
        ErrorMessage = errorMessage;

        if (validateOnType)
            Control.TextChanged += (_, _) => Validate();
        else
            Control.LostFocus += (_, _) => Validate();
    }

    private void Validate()
    {
        var valid = IsValid();

        //TODO: control.BackgroundColor also sets the selection background color on Gtk.
        if (EnableErrorColoring)
            Control.BackgroundColor = valid ? defaultColor : ErrorColor;
        Control.ToolTip = valid ? null : ErrorMessage;
    }

    protected abstract bool IsValid();
}