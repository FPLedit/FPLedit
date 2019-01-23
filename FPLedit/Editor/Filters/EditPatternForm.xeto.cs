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
        private TextBox searchTextBox;
        private Label propertyLabel;
        private StackLayout typeSelectionStack;
#pragma warning restore CS0649
        private SelectionUI typeSelection;

        public string Pattern { get; set; }

        public EditPatternForm(string pattern, string property) : this(property)
        {
            Pattern = pattern;
            var type = pattern[0];
            var rest = pattern.Substring(1);

            searchTextBox.Text = rest;

            switch (type)
            {
                case '^': typeSelection.ChangeSelection(0); break;
                case '$': typeSelection.ChangeSelection(1); break;
                case ' ': typeSelection.ChangeSelection(2); break;
                case '=': typeSelection.ChangeSelection(3); break;
            }
        }

        public EditPatternForm(string property)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            propertyLabel.Text = property;

            typeSelection = new SelectionUI(null, typeSelectionStack, "beginnt mit", "endet mit", "enthält", "ist");
        }

        private void closeButton_Click(object sender, EventArgs e)
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

            Pattern = type + searchTextBox.Text;

            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
