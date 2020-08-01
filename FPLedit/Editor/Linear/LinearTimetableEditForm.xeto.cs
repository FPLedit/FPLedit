using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;

namespace FPLedit.Editor.Linear
{
    internal sealed class LinearTimetableEditForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly LinearTimetableEditControl editor;
#pragma warning restore CS0649

        private readonly IPluginInterface pluginInterface;
        private readonly object backupHandle;

        public LinearTimetableEditForm(IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.pluginInterface = pluginInterface;
            backupHandle = pluginInterface.BackupTimetable();

            editor.Initialize(pluginInterface.Timetable);

            KeyDown += editor.HandleControlKeystroke;

            this.AddCloseHandler();
            this.AddSizeStateHandler();

            // Bugfix, Window closes on [Enter]
            // Important: After AddCloseHandler, otherwise it will destroy Timetable instance in mpmode!
            if (!Platform.IsWpf)
                DefaultButton = null;
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
            public static readonly string Title = T._("Fahrplan bearbeiten");
        }
    }
}
