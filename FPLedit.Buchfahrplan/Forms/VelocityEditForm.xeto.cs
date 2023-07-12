using Eto.Forms;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;

namespace FPLedit.Buchfahrplan.Forms;

internal sealed class VelocityEditForm : FDialog<DialogResult>
{
#pragma warning disable CS0649,CA2213
    private readonly TextBox nameTextBox = null!, positionTextBox = null!, velocityTextBox = null!;
    private readonly DropDown wellenComboBox = null!, directionComboBox = null!;
#pragma warning restore CS0649,CA2213
    private readonly ValidatorCollection validators;

    public IStation Station { get; init; } = null!;

    private readonly int route;

    private VelocityEditForm()
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        var velocityValidator = new NumberValidator(velocityTextBox, true, true, errorMessage: T._("Bitte eine Zahl als Geschwindikgeit eingeben!"));
        var positionValidator = new NumberValidator(positionTextBox, false, false, errorMessage: T._("Bitte eine Zahl als Position eingeben!"));
        validators = new ValidatorCollection(velocityValidator, positionValidator);

        for (int i = 0; i < 4; i++)
            wellenComboBox.Items.Add(i.ToString());
        wellenComboBox.SelectedIndex = 0;

        Dictionary<string, string> directions = new()
        {
            [""] = T._("beide Richtungen"),
            ["ti"] = T._("nur in Kilometrierungsrichtung"),
            ["ta"] = T._("nur entgegen Kilometrierungsrichtung"),
        };
        directionComboBox.DataStore = directions.Keys;
        directionComboBox.ItemTextBinding = Binding.Delegate<string, string>(s => directions[s]);
        directionComboBox.SelectedIndex = 0;
    }

    public VelocityEditForm(Timetable tt, int route) : this()
    {
        var point = new BfplPoint(tt);
        // Add route if it's a network timetbale
        if (tt.Type == TimetableType.Network)
            point._InternalAddRoute(route);
        Station = point;
        this.route = route;
    }

    public VelocityEditForm(IStation sta, int route) : this()
    {
        Station = sta;
        this.route = route;

        velocityTextBox.Text = sta.Vmax.GetValue(route);
        positionTextBox.Text = sta.Positions.GetPosition(route).ToString();
        nameTextBox.Text = sta.SName;
        wellenComboBox.SelectedIndex = sta.Wellenlinien.GetValue(route); // Achtung: Keine Datenbindung, Index!

        if (sta is BfplPoint p)
            directionComboBox.SelectedValue = p.Direction.GetValue(route);

        if (sta is not BfplPoint)
        {
            positionTextBox.Enabled = false;
            nameTextBox.Enabled = false;

            directionComboBox.SelectedIndex = 0;
            directionComboBox.Enabled = false;
        }
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        if (!validators.IsValid)
        {
            MessageBox.Show(T._("Bitte erst alle Fehler beheben:\n{0}", validators.Message));
            return;
        }

        Station.Vmax.SetValue(route, velocityTextBox.Text);
        Station.Wellenlinien.SetValue(route, int.Parse(((IListItem)wellenComboBox.SelectedValue).Text));

        if (Station is BfplPoint point)
        {
            point.Positions.SetPosition(route, float.Parse(positionTextBox.Text));
            point.SName = nameTextBox.Text;
            point.Direction.SetValue(route, (string)directionComboBox.SelectedValue);
        }
        Close(DialogResult.Ok);
    }

    private void CancelButton_Click(object sender, EventArgs e)
        => Close(DialogResult.Cancel);
        
    public static class L
    {
        public static readonly string Title = T._("Höchstgeschwindigkeit ändern");
        public static readonly string Position = T._("Position [km]");
        public static readonly string Name = T._("Name");
        public static readonly string Vmax = T._("Höchstgeschwindigkeit");
        public static readonly string Wavelines = T._("Wellenlinien");
        public static readonly string Direction = T._("Streckenrichtung");
        public static readonly string Length = T._("Länge der Änderung [km]");
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
    }
}