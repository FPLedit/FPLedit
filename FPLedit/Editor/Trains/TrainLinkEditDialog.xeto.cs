using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using FPLedit.Shared.UI.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.TrainLinks;

namespace FPLedit.Editor.Trains
{
    internal sealed class TrainLinkEditDialog : FDialog<DialogResult>
    {
#pragma warning disable CS0649,CA2213
        private readonly TextBox startOffsetTextBox = default!, differenceTextBox = default!, nameTextBox = default!, changeTextBox = default!, countTextBox = default!;
        private readonly TableLayout autoTrainNameTableLayout = default!, specialTrainNameTableLayout = default!;
        private readonly GridView specialNameGridView = default!;
#pragma warning restore CS0649,CA2213
        private readonly NumberValidator countValidator, changeValidator;
        private readonly TimeValidator differenceValidator, offsetValidator;

        private readonly Train train;
        private readonly Timetable tt;
        private readonly TrainLink origLink;

        /// <summary>
        /// Create a new dialog to edit (= discard + create new) the given train link.
        /// </summary>
        /// <param name="link">Link object that is used as blueprint. Note: The referenced object will not be mutated!</param>
        /// <param name="tt">Current timetable instance.</param>
        public TrainLinkEditDialog(TrainLink link, Timetable tt)
        {
            if (tt.Type == TimetableType.Network && tt.Version.CompareTo(TimetableVersion.Extended_FPL2) < 0)
                throw new TimetableTypeNotSupportedException("train links");
            
            Eto.Serialization.Xaml.XamlReader.Load(this);

            differenceValidator = new TimeValidator(differenceTextBox, false, errorMessage: T._("Bitte die Verschiebung als Zeitangabe angeben!"));
            offsetValidator = new TimeValidator(startOffsetTextBox, false, errorMessage: T._("Bitte die Startverschiebung als Zeitangabe angeben!"));
            countValidator = new NumberValidator(countTextBox, false, true, allowNegative: false, errorMessage: T._("Bitte eine gültige Anzahl >0 neuer Züge eingeben!"));
            changeValidator = new NumberValidator(changeTextBox, false, true, errorMessage: T._("Bitte eine gültige Veränderung der Zugnummer eingeben!"));

            origLink = link;
            train = link.ParentTrain;
            this.tt = tt;
            
            startOffsetTextBox.Text = link.TimeOffset.ToTimeString();
            differenceTextBox.Text = link.TimeDifference.ToTimeString();
            countTextBox.Text = link.TrainCount.ToString();
            
            switch (link.TrainNamingScheme)
            {
                case AutoTrainNameGen atnc:
                    autoTrainNameTableLayout.Visible = true;
                    changeTextBox.Text = atnc.Increment.ToString();
                    nameTextBox.Text = atnc.BaseTrainName.FullName;
                    break;
                case SpecialTrainNameGen stnc:
                    specialTrainNameTableLayout.Visible = true;
                    specialNameGridView.AddFuncColumn((SpecialNameEntry spn) => spn.RowNumber.ToString(), "");
                    specialNameGridView.AddColumn((SpecialNameEntry spn) => spn.Name, T._("Zugname"), true);
                   
                    var ds = new SpecialNameEntry[stnc.Names.Length];
                    for (int i = 0; i < stnc.Names.Length; i++)
                        ds[i] = new SpecialNameEntry(i + 1, stnc.Names[i]);
                    specialNameGridView.DataStore = ds;
                    break;
                default:
                    throw new NotSupportedException("Not Implemented: Name calculator not supported!");
            }
        }
        
        private void CountTextBox_TextChanged(object sender, EventArgs e)
        {
            if (!countValidator.Valid || specialNameGridView.DataStore == null)
                return;
            var count = int.Parse(countTextBox.Text);
            var oldStore = (SpecialNameEntry[])specialNameGridView.DataStore;
            IEnumerable<SpecialNameEntry> newStore;
            if (count <= oldStore.Length)
                newStore = oldStore.Take(count);
            else
            {
                var newList = new List<SpecialNameEntry>(oldStore);
                for (int i = oldStore.Length; i < count; i++)
                    newList.Add(new SpecialNameEntry(i + 1, ""));
                newStore = newList;
            }

            specialNameGridView.DataStore = newStore.ToArray();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            changeValidator.Enabled = origLink.TrainNamingScheme is AutoTrainNameGen;
            var validators = new ValidatorCollection(differenceValidator, differenceValidator, changeValidator, offsetValidator);
            if (!validators.IsValid)
            {
                MessageBox.Show(T._("Bitte erst alle Felder korrekt ausfüllen:\n{0}", validators.Message));
                Result = DialogResult.None;
                return;
            }

            var th = new TrainEditHelper();
            var offset = TimeEntry.Parse(startOffsetTextBox.Text);
            var diff = TimeEntry.Parse(differenceTextBox.Text);

            var count = int.Parse(countTextBox.Text);

            ITrainNameGen tnc;
            switch (origLink.TrainNamingScheme)
            {
                // Create new link.
                case AutoTrainNameGen _:
                    var add = int.Parse(changeTextBox.Text);
                    tnc = new AutoTrainNameGen(nameTextBox.Text, add);
                    break;
                case SpecialTrainNameGen _:
                    var entries = ((SpecialNameEntry[]) specialNameGridView.DataStore).Select(en => en.Name).ToArray();
                    if (entries.Any(string.IsNullOrEmpty))
                    {
                        MessageBox.Show(T._("Es wurden keinen Namen für alle Züge angegeben!"));
                        return;
                    }
                    tnc = new SpecialTrainNameGen(entries);
                    break;
                default:
                    throw new NotSupportedException("Not Implemented: Name calculator not supported!");
            }
            
            tt.RemoveLink(origLink); // Remove old link.
            th.LinkTrainMultiple(train, offset, diff, count, tnc); // Create new link.
            
            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Übernehmen");
            public static readonly string StartOffset = T._("Verschiebung des ersten Zugs");
            public static readonly string Difference = T._("Taktverschiebung in Minuten");
            public static readonly string Count = T._("Anzahl der neuen Züge");
            public static readonly string BaseName = T._("Basiszugnummer");
            public static readonly string NumberChange = T._("Änderung der Zugnummer");
            public static readonly string Title = T._("Verlinkung bearbeiten");
        }
    }
    
    internal record SpecialNameEntry(int RowNumber, string Name);
}
