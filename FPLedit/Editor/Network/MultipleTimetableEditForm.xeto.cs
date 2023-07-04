using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Linq;
using FPLedit.Shared.UI;
using FPLedit.Editor.TimetableEditor;

namespace FPLedit.Editor.Network
{
    internal sealed class MultipleTimetableEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649,CA2213
        private readonly DropDown trainDropDown = default!;
        private readonly SingleTimetableEditControl editor = default!;
#pragma warning restore CS0649,CA2213

        private readonly IPluginInterface pluginInterface;
        private readonly object backupHandle;

        public MultipleTimetableEditForm(IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            KeyDown += (_, e) => editor.HandleControlKeystroke(e);

            this.AddCloseHandler();
            this.AddSizeStateHandler();

            // Bugfix, Window closes on enter [Enter]
            // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
            if (!Platform.IsWpf)
                DefaultButton = null;

            this.pluginInterface = pluginInterface;
            backupHandle = pluginInterface.BackupTimetable();

            //editor.Initialize(info.Timetable, t);
            //Title = Title.Replace("{train}", t.TName);

            trainDropDown.ItemTextBinding = Binding.Delegate<ITrain, string>(tr => tr.TName);
            trainDropDown.DataStore = pluginInterface.Timetable.Trains.Where(tr => tr is IWritableTrain);
            trainDropDown.SelectedIndexChanged += TrainDropDown_SelectedIndexChanged;
            trainDropDown.SelectedIndex = 0;
        }

        private void TrainDropDown_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (editor.Initialized)
                editor.ApplyChanges(); // Save old train data.

            var t = (IWritableTrain)trainDropDown.SelectedValue;
            editor.Initialize(t);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Control && (e.Key == Keys.D0 || e.Key == Keys.Keypad0))
            {
                trainDropDown.Focus();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        #region Events
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;

            if (!editor.ApplyChanges())
                return;

            pluginInterface.ClearBackup(backupHandle);
            this.NClose();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            pluginInterface.RestoreTimetable(backupHandle);

            this.NClose();
        }
        #endregion
        
        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Schließen");
            public static readonly string TrainName = T._("Zugnummer:");
            public static readonly string Title = T._("Fahrpläne bearbeiten");
        }
    }
}
