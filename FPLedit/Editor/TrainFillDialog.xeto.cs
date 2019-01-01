﻿using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor
{
    internal class TrainFillDialog : Dialog<DialogResult>
    {
#pragma warning disable CS0649
        private DropDown trainsComboBox;
        private TextBox offsetTextBox;
#pragma warning restore CS0649
        private NumberValidator offsetValidator;

        public Train ReferenceTrain { get; private set; }

        public int Offset { get; private set; }

        public TrainFillDialog(Timetable tt, Train train)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            offsetValidator = new NumberValidator(offsetTextBox, false, true);
            offsetValidator.ErrorMessage = "Bitte die Verschiebung als Zahl in Minuten angeben!";

            offsetTextBox.Text = "+20";

            trainsComboBox.ItemTextBinding = Binding.Property<Train, string>(t => t.TName);
            trainsComboBox.DataStore = new TrainEditHelper().FillCandidates(train);
            trainsComboBox.SelectedIndex = 0;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!offsetValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Felder korrekt ausfüllen:" + Environment.NewLine + offsetValidator.ErrorMessage);
                Result = DialogResult.None;
                return;
            }

            ReferenceTrain = (Train)trainsComboBox.SelectedValue;
            Offset = int.Parse(offsetTextBox.Text);

            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}