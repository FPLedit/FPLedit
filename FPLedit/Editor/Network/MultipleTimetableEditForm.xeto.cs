using Eto.Forms;
using FPLedit.Shared;
using System;
using FPLedit.Shared.UI;
using FPLedit.Editor.TimetableEditor;

namespace FPLedit.Editor.Network
{
    internal sealed class MultipleTimetableEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly DropDown trainDropDown;
        private readonly SingleTimetableEditControl editor;
#pragma warning restore CS0649

        private readonly IPluginInterface pluginInterface;
        private readonly object backupHandle;

        private MultipleTimetableEditForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            KeyDown += editor.HandleControlKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();

            // Bugfix, Window closes on enter [Enter]
            // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
            if (!Platform.IsWpf)
                DefaultButton = null;
        }

        public MultipleTimetableEditForm(IPluginInterface pluginInterface) : this()
        {
            this.pluginInterface = pluginInterface;
            backupHandle = pluginInterface.BackupTimetable();

            //editor.Initialize(info.Timetable, t);
            //Title = Title.Replace("{train}", t.TName);

            trainDropDown.ItemTextBinding = Binding.Property<ITrain, string>(tr => tr.TName);
            trainDropDown.DataStore = pluginInterface.Timetable.Trains;
            trainDropDown.SelectedIndexChanged += TrainDropDown_SelectedIndexChanged;
            trainDropDown.SelectedIndex = 0;
        }

        private void TrainDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = (Train)trainDropDown.SelectedValue;
            if (editor.Initialized)
                editor.ApplyChanges();

            editor.Initialize(pluginInterface.Timetable, t);
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
    }
}
