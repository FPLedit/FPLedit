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
    internal class EditPatternForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly TextBox searchTextBox;
        private readonly Label propertyLabel;
        private readonly StackLayout typeSelectionStack;
        private readonly CheckBox negateCheckBox;
#pragma warning restore CS0649
        private readonly SelectionUI typeSelection;

        public FilterRule Pattern { get; set; }

        public EditPatternForm(FilterRule rule, string property) : this(property)
        {
            Pattern = rule;
            searchTextBox.Text = rule.SearchString;
            negateCheckBox.Checked = rule.Negate;

            switch (rule.FilterType)
            {
                case FilterType.StartsWith: typeSelection.ChangeSelection(0); break;
                case FilterType.EndsWidth: typeSelection.ChangeSelection(1); break;
                case FilterType.Contains: typeSelection.ChangeSelection(2); break;
                case FilterType.Equals: typeSelection.ChangeSelection(3); break;
            }
        }

        public EditPatternForm(string property)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            propertyLabel.Text = property;

            typeSelection = new SelectionUI(null, typeSelectionStack, "beginnt mit", "endet mit", "enthält", "ist");
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            if (searchTextBox.Text.Length == 0)
            {
                MessageBox.Show("Bitte einen Suchwert eingeben!");
                Result = DialogResult.Cancel;
                return;
            }

            char type = '=';
            if (typeSelection.SelectedState == 0)
                type = '^';
            else if (typeSelection.SelectedState == 1)
                type = '$';
            else if (typeSelection.SelectedState == 2)
                type = ' ';

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
    }
}
