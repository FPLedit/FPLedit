﻿using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Bildfahrplan.Forms
{
    public partial class StationStyleForm : Dialog<DialogResult>
    {
        private IInfo info;
        private Timetable tt;
        private TimetableStyle attrs;
        private ColorCollection cc;
        private DashStyleHelper ds;

#pragma warning disable CS0649
        private GridView gridView;
#pragma warning restore CS0649

        public StationStyleForm(IInfo info)
        {
            this.info = info;
            tt = info.Timetable;
            attrs = new TimetableStyle(tt);
            info.BackupTimetable();

            Eto.Serialization.Xaml.XamlReader.Load(this);

            cc = new ColorCollection(info.Settings);
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

                var sef = new StationStyleEditForm(station, info.Settings);
                if (sef.ShowModal(this) == DialogResult.Ok)
                    UpdateStations();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Station ausgewählt werden!", "Stationsdarstellung ändern");
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            info.RestoreTimetable();
            this.NClose();
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;
            info.ClearBackup();
            this.NClose();
        }

        private void editButton_Click(object sender, EventArgs e)
            => EditColor();
    }
}