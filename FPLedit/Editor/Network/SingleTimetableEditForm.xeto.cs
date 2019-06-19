using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.UI;

namespace FPLedit.Editor.Network
{
    internal class SingleTimetableEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly SingleTimetableEditControl editor;
#pragma warning restore CS0649

        private readonly IInfo info;
        private readonly object backupHandle;

        private SingleTimetableEditForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            KeyDown += editor.HandleControlKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();

            if (!Platform.IsWpf)
                DefaultButton = null; // Bugfix, Window closes on enter [Enter]
                                      // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
        }

        public SingleTimetableEditForm(IInfo info, Train t) : this()
        {
            this.info = info;
            backupHandle = info.BackupTimetable();

            editor.Initialize(info.Timetable, t);
            Title = Title.Replace("{train}", t.TName);
        }

        #region Events
        private void CloseButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;

            if (!editor.ApplyChanges())
                return;

            info.ClearBackup(backupHandle);
            this.NClose();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable(backupHandle);

            this.NClose();
        }
        #endregion
    }
}
