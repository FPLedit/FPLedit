using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Buchfahrplan
{
    public partial class NewTrainForm : Form
    {
        public Train NewTrain { get; set; }

        public NewTrainForm()
        {
            InitializeComponent();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            NewTrain = new Train()
            {
                Name = nameTextBox.Text,
                Line = lineTextBox.Text, 
                Locomotive = locomotiveTextBox.Text,
                Negative = negativeCheckBox.Checked
            };
        }
    }
}
