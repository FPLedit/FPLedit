using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Filters
{
    internal sealed class EditPatternForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly TextBox searchTextBox;
        private readonly Label propertyLabel;
        private readonly StackLayout typeSelectionStack;
        private readonly CheckBox negateCheckBox;
#pragma warning restore CS0649
        private readonly SelectionUI<PatternSelectionType> typeSelection;

        public FilterRule Pattern { get; private set; }

        public EditPatternForm(FilterRule rule, string property, FilterTarget target) : this(property, target)
        {
            Pattern = rule;
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
                MessageBox.Show("Bitte einen Suchwert eingeben!");
                Result = DialogResult.Cancel;
                return;
            }

            char type = (char)typeSelection.SelectedState;

            var negate = negateCheckBox.Checked == true ? "!" : "";

            Pattern = new FilterRule(negate + type + searchTextBox.Text);

            Close(DialogResult.Ok);
        }

        protected override void Dispose(bool disposing)
        {
            typeSelection?.Dispose();
            base.Dispose(disposing);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

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
    }
}
