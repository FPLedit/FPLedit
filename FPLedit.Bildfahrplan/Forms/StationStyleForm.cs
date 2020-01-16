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
    public class StationStyleForm : FDialog<DialogResult>
    {
        private readonly IPluginInterface pluginInterface;
        private readonly Timetable tt;
        private readonly TimetableStyle attrs;
        private readonly ColorCollection cc;
        private readonly DashStyleHelper ds;
        private readonly object backupHandle;

#pragma warning disable CS0649
        private readonly GridView gridView;
#pragma warning restore CS0649

        public StationStyleForm(IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            tt = pluginInterface.Timetable;
            attrs = new TimetableStyle(tt);
            backupHandle = pluginInterface.BackupTimetable();

            Eto.Serialization.Xaml.XamlReader.Load(this);

            cc = new ColorCollection(pluginInterface.Settings);
            ds = new DashStyleHelper();

            gridView.AddColumn<Station>(t => t.SName, "Name");
            gridView.AddColumn<Station>(t => cc.ToName(new StationStyle(t).StationColor ?? attrs.StationColor), "Farbe");
            gridView.AddColumn<Station>(t => (new StationStyle(t).StationWidth ?? attrs.StationWidth).ToString(), "Linienstärke");
            gridView.AddColumn<Station>(t => ds.GetDescription(new StationStyle(t).LineStyle), "Linientyp");
            gridView.AddColumn<Station>(t => new StationStyle(t).Show.ToString(), "Station zeichnen");

            gridView.CellDoubleClick += (s, e) => EditColor(false);

            UpdateStations();

            this.AddCloseHandler();
        }

        private void UpdateStations()
        {
            gridView.DataStore = tt.Stations;
        }

        private void EditColor(bool message = true)
        {
            if (gridView.SelectedItem != null)
            {
                var station = (Station)gridView.SelectedItem;

                using (var sef = new StationStyleEditForm(station, pluginInterface.Settings))
                    if (sef.ShowModal(this) == DialogResult.Ok)
                        UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Stationsdarstellung ändern");
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

        private void EditButton_Click(object sender, EventArgs e)
            => EditColor();
    }
}
