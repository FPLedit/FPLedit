﻿using FPLedit.Shared;
using System;
using System.Linq;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    public partial class TrainEditForm : Form
    {
        public Train Train { get; set; }

        private CheckBox[] daysBoxes;

        public TrainEditForm()
        {
            InitializeComponent();
            daysBoxes = new[] { mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox };

            KeyDown += (s, e) =>
            {
                if (!e.Control)
                    return;

                if (new[] { Keys.A, Keys.W, Keys.S, Keys.D0, Keys.NumPad0 }.Contains(e.KeyCode))
                {
                    daysBoxes.All(c => { c.Checked = false; return true; });
                    e.Handled = true;
                }
                if (e.KeyCode == Keys.A)
                    daysBoxes.All(c => c.Checked = true);
                else if (e.KeyCode == Keys.W && e.Shift)
                    daysBoxes.Take(5).All(c => c.Checked = true);
                else if (e.KeyCode == Keys.W)
                    daysBoxes.Take(6).All(c => c.Checked = true);
                else if (e.KeyCode == Keys.S)
                    daysBoxes.Last().Checked = true;
            };
        }

        public TrainEditForm(Train train) : this()
        {
            Train = train;
            nameTextBox.Text = train.TName;
            locomotiveComboBox.Text = train.Locomotive;
            locomotiveComboBox.Items.AddRange(train._parent.GetAllTfzs());
            mbrTextBox.Text = train.Mbr;
            lastTextBox.Text = train.Last;
            commentTextBox.Text = train.Comment;

            for (int i = 0; i < Train.Days.Length; i++)
                daysBoxes[i].Checked = Train.Days[i];

            Text = "Zug bearbeiten";
        }

        public TrainEditForm(Timetable tt, TrainDirection direction) : this()
        {
            Train = new Train(direction, tt);
            locomotiveComboBox.Items.AddRange(tt.GetAllTfzs());
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!nameValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            var name_exists = Train._parent.Trains.Select(t => t.TName).Contains(nameTextBox.Text);

            if (name_exists && Train.TName == null)
            {
                if (MessageBox.Show("Ein Zug mit dem Namen \"" + nameTextBox.Text + "\" ist bereits vorhanden. Wirklich fortfahren?", "FPLedit",
                    MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            Train.TName = nameTextBox.Text;
            Train.Locomotive = locomotiveComboBox.Text;
            Train.Mbr = mbrTextBox.Text;
            Train.Last = lastTextBox.Text;
            Train.Comment = commentTextBox.Text;
            Train.Days = daysBoxes.Select(b => b.Checked).ToArray();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
