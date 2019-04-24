using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    internal class ZlmEditForm : FDialog<DialogResult>
    {
        #pragma warning disable CS0649
        private readonly TextBox zlmTextBox;
        #pragma warning restore CS0649

        public string Zlm { get; set; }

        public ZlmEditForm(string zlm)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            Zlm = zlm;
            zlmTextBox.Text = zlm;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Zlm = zlmTextBox.Text;
            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
