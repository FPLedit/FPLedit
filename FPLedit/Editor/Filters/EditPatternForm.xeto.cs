using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Filters
{
    internal class EditPatternForm : Dialog<DialogResult>
    {
        #pragma warning disable CS0649
        private TextBox searchTextBox;
        private Label propertyLabel;
        private RadioButton startsWithRadioButton, endsWithRadioButton, containsRadioButton, equalsRadioButton;
        #pragma warning restore CS0649

        public string Pattern { get; set; }

        public EditPatternForm(string pattern, string property)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            Pattern = pattern;
            var type = pattern[0];
            var rest = pattern.Substring(1);

            searchTextBox.Text = rest;
            propertyLabel.Text = property;

            switch (type)
            {
                case '^': startsWithRadioButton.Checked = true; break;
                case '$': endsWithRadioButton.Checked = true; break;
                case ' ': containsRadioButton.Checked = true; break;
                case '=': equalsRadioButton.Checked = true; break;
            }
        }

        public EditPatternForm(string property)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            propertyLabel.Text = property;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (searchTextBox.Text.Length == 0)
            {
                MessageBox.Show("Bitte einen Suchwert eingeben!");
                Result = DialogResult.Cancel;
                return;
            }

            Result = DialogResult.Ok;

            char type;

            if (startsWithRadioButton.Checked)
                type = '^';
            else if (endsWithRadioButton.Checked)
                type = '$';
            else if (containsRadioButton.Checked)
                type = ' ';
            else
                type = '=';

            Pattern = type + searchTextBox.Text;

            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            Close();
        }
    }
}
