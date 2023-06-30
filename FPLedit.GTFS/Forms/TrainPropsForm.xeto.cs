using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;
using FPLedit.GTFS.GTFSLib;
using FPLedit.GTFS.Model;

namespace FPLedit.GTFS.Forms
{
    internal sealed class TrainPropsForm : FDialog<DialogResult>
    {
        private readonly IPluginInterface pluginInterface;
        private readonly object backupHandle;

#pragma warning disable CS0649,CA2213
        private readonly GridView gridView = default!;
#pragma warning restore CS0649,CA2213

        public TrainPropsForm(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            var tt = pluginInterface.Timetable;
            backupHandle = pluginInterface.BackupTimetable();

            Eto.Serialization.Xaml.XamlReader.Load(this);

            gridView.AddColumn<GtfsTrainAttrs>(t => t.Train.TName, T._("Zugnummer"));
            gridView.AddColumn<GtfsTrainAttrs>(t => t.DaysOverride,T._("Verkehrstage"), true);
            gridView.AddDropDownColumn<GtfsTrainAttrs, AccessibilityState>(t => t.BikesAllowed, T._("Fahrradmitn."), true);
            gridView.AddDropDownColumn<GtfsTrainAttrs, AccessibilityState>(t => t.WheelchairAccessible, T._("Rollstuhlmitn."), true);

            gridView.DataStore = tt.Trains.OfType<IWritableTrain>().Select(t => new GtfsTrainAttrs(t)).ToArray();

            this.AddCloseHandler();
        }
        
        private void ResetTrainStyle(bool message = true)
        {
            if (gridView.SelectedItem != null)
            {
                ((GtfsTrainAttrs)gridView.SelectedItem).ResetDefaults();
                gridView.ReloadData(gridView.SelectedRow);
            }
            else if (message)
                MessageBox.Show(T._("Zuerst muss ein Zug ausgewählt werden!"), T._("Zugdarstellung zurücksetzen"));
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            pluginInterface.RestoreTimetable(backupHandle);
            this.NClose();
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;
            pluginInterface.ClearBackup(backupHandle);
            this.NClose();
        }

        private void ResetButton_Click(object sender, EventArgs e)
            => ResetTrainStyle();
        
        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Schließen");
            public static readonly string Reset = T._("Eintrag &zurücksetzen");
            public static readonly string Title = T._("GTFS-Zugattribute ändern");
        }
    }
}
