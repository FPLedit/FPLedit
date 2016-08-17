using Buchfahrplan.Properties;
using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Buchfahrplan
{
    public partial class TimetableEditForm : Form
    {
        private IInfo info;

        public TimetableEditForm()
        {
            InitializeComponent();

            this.Icon = Resources.programm;

            topDataGridView.AllowUserToAddRows = false;
            bottomDataGridView.AllowUserToAddRows = false;
        }

        public void Init(IInfo info)
        {
            this.info = info;
            info.BackupTimetable();

            topFromToLabel.Text = "Züge " + info.Timetable.GetLineName(true);
            bottomFromToLabel.Text = "Züge " + info.Timetable.GetLineName(false);

            UpdateGrid();
        }

        private void UpdateGrid()
        {
            // Obere DataGridView
            var topStations = info.Timetable.Stations.OrderByDescending(s => s.Kilometre);
            foreach (var sta in topStations)
            {
                if (topStations.First() != sta)
                    topDataGridView.Columns.Add(sta.Name + "ar", sta.Name + " an");
                if (topStations.Last() != sta)
                    topDataGridView.Columns.Add(sta.Name + "dp", sta.Name + " ab");
            }

            foreach (var tra in info.Timetable.Trains.Where(t => t.Direction))
            {
                DataGridViewRow trainRow = topDataGridView.Rows[topDataGridView.Rows.Add()];

                foreach (var sta in tra.Arrivals.Keys.OrderByDescending(s => s.Kilometre))
                    trainRow.Cells[sta.Name + "ar"].Value = tra.Arrivals[sta].ToShortTimeString();

                foreach (var sta in tra.Departures.Keys.OrderByDescending(s => s.Kilometre))
                    trainRow.Cells[sta.Name + "dp"].Value = tra.Departures[sta].ToShortTimeString();

                trainRow.Tag = tra;
                trainRow.HeaderCell = new DataGridViewRowHeaderCell() { Value = tra.Name };
            }

            foreach (DataGridViewColumn column in topDataGridView.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // Untere DataGridView
            var bottomStations = info.Timetable.Stations.OrderBy(s => s.Kilometre);
            foreach (var sta in bottomStations)
            {
                if (bottomStations.First() != sta)
                    bottomDataGridView.Columns.Add(sta.Name + "ar", sta.Name + " an");
                if (bottomStations.Last() != sta)
                    bottomDataGridView.Columns.Add(sta.Name + "dp", sta.Name + " ab");
            }

            foreach (var tra in info.Timetable.Trains.Where(t => !t.Direction))
            {
                DataGridViewRow trainRow = bottomDataGridView.Rows[bottomDataGridView.Rows.Add()];

                foreach (var sta in tra.Arrivals.Keys.OrderBy(s => s.Kilometre))
                    trainRow.Cells[sta.Name + "ar"].Value = tra.Arrivals[sta].ToShortTimeString();

                foreach (var sta in tra.Departures.Keys.OrderBy(s => s.Kilometre))
                    trainRow.Cells[sta.Name + "dp"].Value = tra.Departures[sta].ToShortTimeString();

                trainRow.Tag = tra;
                trainRow.HeaderCell = new DataGridViewRowHeaderCell() { Value = tra.Name };
            }

            foreach (DataGridViewColumn column in bottomDataGridView.Columns)
            {
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void UpdateTrainData(Train train)
        {
            var ar = new Dictionary<Station, TimeSpan>();
            var dp = new Dictionary<Station, TimeSpan>();

            bool found = false;

            // Obere DataGridView
            foreach (DataGridViewRow row in topDataGridView.Rows)
            {
                Train t = (Train)row.Tag;

                if (t != train)
                    continue;
                if (found)
                    break;

                foreach (var sta in info.Timetable.Stations)
                {
                    if (topDataGridView.Columns.Contains(sta.Name + "ar"))
                    {
                        DataGridViewCell cellAr = row.Cells[sta.Name + "ar"];

                        if ((string)cellAr.Value != "" && cellAr.Value != null)
                        {
                            TimeSpan tsAr = TimeSpan.Parse((string)cellAr.Value);
                            ar.Add(sta, tsAr);
                        }
                    }

                    if (topDataGridView.Columns.Contains(sta.Name + "dp"))
                    {
                        DataGridViewCell cellDp = row.Cells[sta.Name + "dp"];

                        if ((string)cellDp.Value != "" && cellDp.Value != null)
                        {
                            TimeSpan tsDp = TimeSpan.Parse((string)cellDp.Value);
                            dp.Add(sta, tsDp);
                        }
                    }
                }

                found = true;
            }

            // Untere DataGridView
            foreach (DataGridViewRow row in bottomDataGridView.Rows)
            {
                Train t = (Train)row.Tag;

                if (t != train)
                    continue;
                if (found)
                    break;

                foreach (var sta in info.Timetable.Stations)
                {
                    if (bottomDataGridView.Columns.Contains(sta.Name + "ar"))
                    {
                        DataGridViewCell cellAr = row.Cells[sta.Name + "ar"];

                        if ((string)cellAr.Value != "" && cellAr.Value != null)
                        {
                            TimeSpan tsAr = TimeSpan.Parse((string)cellAr.Value);
                            ar.Add(sta, tsAr);
                        }
                    }

                    if (bottomDataGridView.Columns.Contains(sta.Name + "dp"))
                    {
                        DataGridViewCell cellDp = row.Cells[sta.Name + "dp"];

                        if ((string)cellDp.Value != null && cellDp.Value != null)
                        {
                            TimeSpan tsDp = TimeSpan.Parse((string)cellDp.Value);
                            dp.Add(sta, tsDp);
                        }
                    }
                }

                found = true;
            }

            if (found)
            {
                train.Arrivals = ar;
                train.Departures = dp;
            }
            else
                throw new Exception("In der Anwendung ist ein interner Fehler aufgetreten!");
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            foreach (var t in info.Timetable.Trains)
                UpdateTrainData(t);

            topDataGridView.Rows.Clear();
            topDataGridView.Columns.Clear();

            bottomDataGridView.Rows.Clear();
            bottomDataGridView.Columns.Clear();

            info.ClearBackup();
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            info.RestoreTimetable();

            Close();
        }
    }
}
