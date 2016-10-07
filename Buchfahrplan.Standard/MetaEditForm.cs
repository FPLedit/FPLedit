using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Buchfahrplan.Standard
{
    public partial class MetaEditForm : Form
    {
        public KeyValuePair<string, string> Meta { get; set; }

        public MetaEditForm()
        {
            InitializeComponent();
        }

        public MetaEditForm(KeyValuePair<string, string> meta) : this()
        {
            Meta = meta;
            Text = "Eigenschaft bearbeiten";
            keyTextBox.Text = meta.Key;
            valueTextBox.Text = meta.Value;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            string key = keyTextBox.Text;
            string value = valueTextBox.Text;

            if (!keyValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            Meta = new KeyValuePair<string, string>(key, value);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
