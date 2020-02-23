using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Bildfahrplan.Forms
{
    internal class TrainStyleForm : FDialog<DialogResult>
    {
        private readonly IPluginInterface pluginInterface;
        private readonly object backupHandle;

#pragma warning disable CS0649
        private readonly GridView gridView;
#pragma warning restore CS0649

        public TrainStyleForm(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            var tt = pluginInterface.Timetable;
            var attrs = new TimetableStyle(tt);
            backupHandle = pluginInterface.BackupTimetable();

            Eto.Serialization.Xaml.XamlReader.Load(this);

            var cc = new ColorCollection(pluginInterface.Settings);
            var ds = new DashStyleHelper();

            var lineWidths = Enumerable.Range(1, 5).Cast<object>().ToArray();

            gridView.AddColumn<TrainStyle>(t => t.Train.TName, "Zugnummer");
            gridView.AddDropDownColumn<TrainStyle>(t => t.HexColor, cc.ColorHexStrings, ExtBind.ColorBinding(cc), "Farbe", true);
            gridView.AddDropDownColumn<TrainStyle>(t => t.TrainWidthInt, lineWidths, Binding.Delegate<int, string>(i => i.ToString()), "Linienstärke", true);
            gridView.AddDropDownColumn<TrainStyle>(t => t.LineStyle, ds.Indices.Cast<object>(), Binding.Delegate<int, string>(i => ds.GetDescription(i)), "Linientyp", true);
            gridView.AddCheckColumn<TrainStyle>(t => t.Show, "Zug zeichnen", true);

            gridView.DataStore = tt.Trains.Select(t => new TrainStyle(t, attrs));

            this.AddCloseHandler();
        }
        
        private void ResetTrainStyle(bool message = true)
        {
            if (gridView.SelectedItem != null)
            {
                ((TrainStyle)gridView.SelectedItem).ResetDefaults();
                gridView.ReloadData(gridView.SelectedRow);
            }
            else if (message)
                MessageBox.Show("Zuerst muss ein Zug ausgewählt werden!", "Zugdarstellung zurücksetzen");
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
    }
}
