using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    public partial class EditPatternForm : Form
    {
        public string Pattern { get; set; }

        public EditPatternForm()
        {
            InitializeComponent();
        }

        public EditPatternForm(string pattern, string property) : this()
        {
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

        public EditPatternForm(string property) : this()
        {
            propertyLabel.Text = property;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (searchTextBox.Text.Length == 0)
            {
                MessageBox.Show("Bitte einen Suchwert eingeben!");
                DialogResult = DialogResult.Cancel;
                return;
            }

            DialogResult = DialogResult.OK;

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

        }
    }
}
