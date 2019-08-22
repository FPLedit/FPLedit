using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.UI;

namespace FPLedit.Editor.Network
{
    internal class MultipleTimetableEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly DropDown trainDropDown;
        private readonly SingleTimetableEditControl editor;
#pragma warning restore CS0649

        private readonly IInfo info;
        private readonly object backupHandle;

        private MultipleTimetableEditForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            KeyDown += editor.HandleControlKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();

            if (!Platform.IsWpf)
                DefaultButton = null; // Bugfix, Window closes on enter [Enter]
                                      // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
        }

        public MultipleTimetableEditForm(IInfo info) : this()
        {
            this.info = info;
            backupHandle = info.BackupTimetable();

            //editor.Initialize(info.Timetable, t);
            //Title = Title.Replace("{train}", t.TName);

            trainDropDown.ItemTextBinding = Binding.Property<Train, string>(tr => tr.TName);
            trainDropDown.DataStore = info.Timetable.Trains;
            trainDropDown.SelectedIndexChanged += TrainDropDown_SelectedIndexChanged;
            trainDropDown.SelectedIndex = 0;
        }

        private void TrainDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = (Train)trainDropDown.SelectedValue;
            if (editor.Initialized)
                editor.ApplyChanges();

            editor.Initialize(info.Timetable, t);
        }

        #region Events
        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;

            if (!editor.ApplyChanges())
                return;

            info.ClearBackup(backupHandle);
            this.NClose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable(backupHandle);

            this.NClose();
        }
        #endregion
    }
}
