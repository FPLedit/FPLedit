using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor.Linear
{
    internal class LineTimetableEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly LineTimetableEditControl editor;
#pragma warning restore CS0649

        private readonly IInfo info;
        private readonly object backupHandle;

        public LineTimetableEditForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;
            backupHandle = info.BackupTimetable();

            editor.Initialize(info.Timetable);

            KeyDown += editor.HandleControlKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();

            if (!Platform.IsWpf)
                DefaultButton = null; // Bugfix, Window closes on [Enter]
                                      // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
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
