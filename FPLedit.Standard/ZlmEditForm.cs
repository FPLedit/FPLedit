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
    public partial class ZlmEditForm : Form
    {
        public string Zlm { get; set; }

        public ZlmEditForm()
        {
            InitializeComponent();
        }

        public ZlmEditForm(string zlm) : this()
        {
            Zlm = zlm;
            zlmTextBox.Text = zlm;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Zlm = zlmTextBox.Text;
            DialogResult = DialogResult.OK;
        }
    }
}
