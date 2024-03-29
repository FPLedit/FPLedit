﻿using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;

namespace FPLedit.Editor.Filters;

internal sealed class EditPatternForm : FDialog<FilterRule?>
{
#pragma warning disable CS0649,CA2213
    private readonly TextBox searchTextBox = default!;
    private readonly Label propertyLabel = default!;
    private readonly StackLayout typeSelectionStack = default!;
    private readonly CheckBox negateCheckBox = default!;
#pragma warning restore CS0649,CA2213
    private readonly SelectionUI<PatternSelectionType> typeSelection;

    public EditPatternForm(FilterRule rule, string property, FilterTarget target) : this(property, target)
    {
        searchTextBox.Text = rule.SearchString;
        negateCheckBox.Checked = rule.Negate;

        typeSelection.ChangeSelection((PatternSelectionType)rule.FilterType);
    }

    public EditPatternForm(string property, FilterTarget target)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        propertyLabel.Text = property;

        typeSelection = new SelectionUI<PatternSelectionType>(null, typeSelectionStack);
        if (target == FilterTarget.Train)
            typeSelection.DisableOption(PatternSelectionType.StationType);
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        if (searchTextBox.Text.Length == 0)
        {
            MessageBox.Show(T._("Bitte einen Suchwert eingeben!"));
            Result = null;
            return;
        }

        char type = (char)typeSelection.SelectedState;
        var negate = negateCheckBox.Checked == true ? "!" : "";
        var pattern = new FilterRule(negate + type + searchTextBox.Text);

        Close(pattern);
    }

    protected override void Dispose(bool disposing)
    {
        typeSelection.Dispose();
        base.Dispose(disposing);
    }

    private void CancelButton_Click(object sender, EventArgs e)
        => Close(null);

    // Hinweis: Speigelung von FPLedit.Shared.FilterType
    private enum PatternSelectionType
    {
        [SelectionName("beginnt mit")]
        StartsWith = '^',
        [SelectionName("endet mit")]
        EndsWith = '$',
        [SelectionName("enthält")]
        Contains = ' ',
        [SelectionName("ist")]
        Equals = '=',
        [SelectionName("Betriebsst.-Typ ist")]
        StationType = '#',
    }

    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Title = T._("Regel bearbeiten");
        public static readonly string Negate = T._("Bedingung umkehren");
    }
}