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
        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

#pragma warning disable CS0649
        private LineTimetableEditControl editor;
#pragma warning restore CS0649

        private IInfo info;

        public LineTimetableEditForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;
            info.BackupTimetable();

            editor.Initialize(info.Timetable);

            KeyDown += editor.HandleControlKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();

            if (!Platform.IsWpf)
                DefaultButton = null; // Bugfix, Window closes on [Enter]
                                      // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
        }

        #region Events
        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;

            if (!editor.ApplyChanges())
                return;

            info.ClearBackup();
            this.NClose();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();

            this.NClose();
        }
        #endregion
    }
}
