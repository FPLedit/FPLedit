using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.UI;

namespace FPLedit.Editor.Network
{
    internal class TrainTimetableEditor : Dialog<DialogResult>
    {
#pragma warning disable CS0649
        private TrainTimetableControl editor;
#pragma warning restore CS0649

        private IInfo info;

        private TrainTimetableEditor()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            KeyDown += editor.HandleControlKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();

            if (!Platform.IsWpf)
                DefaultButton = null; // Bugfix, Window closes on enter [Enter]
                                      // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
        }

        public TrainTimetableEditor(IInfo info, Train t) : this()
        {
            this.info = info;
            info.BackupTimetable();

            editor.Initialize(info.Timetable, t);
            Title = Title.Replace("{train}", t.TName);
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
