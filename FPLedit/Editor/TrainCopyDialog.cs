using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Editor
{
    public partial class TrainCopyDialog : Form
    {
        private Train train;
        private Timetable tt;

        private TrainCopyDialog()
        {
            InitializeComponent();
        }

        public TrainCopyDialog(Train t, Timetable tt) : this()
        {
            train = t;
            this.tt = tt;
            nameTextBox.Text = t.TName;
            offsetTextBox.Text = "+20";
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!numberValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Felder korrekt ausfüllen!");
                DialogResult = DialogResult.None;
                return;
            }

            var offset = int.Parse(offsetTextBox.Text);

            var th = new TrainCopyHelper();
            var newTrain = th.CopyTrain(train, offset, nameTextBox.Text, copyAllCheckBox.Checked);
            tt.AddTrain(newTrain, true);
        }
    }
}
