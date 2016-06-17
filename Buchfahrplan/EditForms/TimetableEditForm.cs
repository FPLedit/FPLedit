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
        private Timetable tt;
        private Timetable tt_undo;

        public TimetableEditForm()
        {
            InitializeComponent();

            this.Icon = Resources.programm;

            topDataGridView.AllowUserToAddRows = false;
            bottomDataGridView.AllowUserToAddRows = false;
        }

        public void Init(Timetable tt)
        {
            this.tt = tt;
            this.tt_undo = tt;

            topFromToLabel.Text = "Züge " + tt.GetLineName(true);
            bottomFromToLabel.Text = "Züge " + tt.GetLineName(false);

            UpdateGrid();
        }

        private void UpdateGrid()
        {
            // Obere DataGridView
            var topStations = tt.Stations.OrderByDescending(s => s.Kilometre);
            foreach (var sta in topStations)
            {
                if (topStations.First() != sta)
                    topDataGridView.Columns.Add(sta.Name + "ar", sta.Name + " an");
                if (topStations.Last() != sta)
                    topDataGridView.Columns.Add(sta.Name + "dp", sta.Name + " ab");
            }

            foreach (var tra in tt.Trains.Where(t => t.Direction))
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
            var bottomStations = tt.Stations.OrderBy(s => s.Kilometre);
            foreach (var sta in bottomStations)
            {
                if (bottomStations.First() != sta)
                    bottomDataGridView.Columns.Add(sta.Name + "ar", sta.Name + " an");
                if (bottomStations.Last() != sta)
                    bottomDataGridView.Columns.Add(sta.Name + "dp", sta.Name + " ab");
            }

            foreach (var tra in tt.Trains.Where(t => !t.Direction))
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
            var ar = new Dictionary<Station, DateTime>();
            var dp = new Dictionary<Station, DateTime>();

            bool found = false;

            // Obere DataGridView
            foreach (DataGridViewRow row in topDataGridView.Rows)
            {
                Train t = (Train)row.Tag;

                if (t != train)
                    continue;
                if (found)
                    break;

                foreach (var sta in tt.Stations)
                {
                    if (topDataGridView.Columns.Contains(sta.Name + "ar"))
                    {
                        DataGridViewCell cellAr = row.Cells[sta.Name + "ar"];

                        if ((string)cellAr.Value != "" && cellAr.Value != null)
                        {
                            DateTime dtAr;
                            DateTime.TryParse((string)cellAr.Value, out dtAr);

                            ar.Add(sta, dtAr);
                        }
                    }

                    if (topDataGridView.Columns.Contains(sta.Name + "dp"))
                    {
                        DataGridViewCell cellDp = row.Cells[sta.Name + "dp"];

                        if ((string)cellDp.Value != "" && cellDp.Value != null)
                        {
                            DateTime dtDp;
                            DateTime.TryParse((string)cellDp.Value, out dtDp);

                            dp.Add(sta, dtDp);
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

                foreach (var sta in tt.Stations)
                {
                    if (bottomDataGridView.Columns.Contains(sta.Name + "ar"))
                    {
                        DataGridViewCell cellAr = row.Cells[sta.Name + "ar"];

                        if ((string)cellAr.Value != "" && cellAr.Value != null)
                        {
                            DateTime dtAr;
                            DateTime.TryParse((string)cellAr.Value, out dtAr);

                            ar.Add(sta, dtAr);
                        }
                    }

                    if (bottomDataGridView.Columns.Contains(sta.Name + "dp"))
                    {
                        DataGridViewCell cellDp = row.Cells[sta.Name + "dp"];

                        if ((string)cellDp.Value != null && cellDp.Value != null)
                        {
                            DateTime dtDp;
                            DateTime.TryParse((string)cellDp.Value, out dtDp);

                            dp.Add(sta, dtDp);
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

            foreach (var t in tt.Trains)
                UpdateTrainData(t);

            topDataGridView.Rows.Clear();
            topDataGridView.Columns.Clear();

            bottomDataGridView.Rows.Clear();
            bottomDataGridView.Columns.Clear();

            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            tt = tt_undo;

            Close();
        }

        private void timetableEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
