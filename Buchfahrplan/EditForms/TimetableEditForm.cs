using Buchfahrplan.FileModel;
using Buchfahrplan.Properties;
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

            UpdateGrid();
        }

        private void UpdateGrid()
        {
            // Obere DataGridView
            foreach (var sta in tt.Stations.OrderBy(s => s.Kilometre))
            {
                topDataGridView.Columns.Add(sta.Name + "ar", sta.Name + "an");
                topDataGridView.Columns.Add(sta.Name + "dp", sta.Name + "ab");
            }

            foreach (var tra in tt.Trains.Where(t => t.Negative))
            {
                DataGridViewRow trainRow = topDataGridView.Rows[topDataGridView.Rows.Add()];

                foreach (var sta in tra.Arrivals.Keys.OrderBy(s => s.Kilometre))
                {
                    try { trainRow.Cells[sta.Name + "ar"].Value = tra.Arrivals[sta].ToShortTimeString(); }
                    catch { trainRow.Cells[sta.Name + "ar"].Value = ""; }
                }

                foreach (var sta in tra.Departures.Keys.OrderBy(s => s.Kilometre))
                {
                    try { trainRow.Cells[sta.Name + "dp"].Value = tra.Departures[sta].ToShortTimeString(); }
                    catch { trainRow.Cells[sta.Name + "dp"].Value = ""; }
                }

                trainRow.Tag = tra;
                trainRow.HeaderCell = new DataGridViewRowHeaderCell() { Value = tra.Name };
            }

            foreach (DataGridViewColumn column in topDataGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            // Untere DataGridView

            foreach (var sta in tt.Stations.OrderByDescending(s => s.Kilometre))
            {
                bottomDataGridView.Columns.Add(sta.Name + "ar", sta.Name + "an");
                bottomDataGridView.Columns.Add(sta.Name + "dp", sta.Name + "ab");
            }

            foreach (var tra in tt.Trains.Where(t => !t.Negative))
            {
                DataGridViewRow trainRow = bottomDataGridView.Rows[bottomDataGridView.Rows.Add()];

                foreach (var sta in tra.Arrivals.Keys.OrderByDescending(s => s.Kilometre))
                {
                    try { trainRow.Cells[sta.Name + "ar"].Value = tra.Arrivals[sta].ToShortTimeString(); }
                    catch { trainRow.Cells[sta.Name + "ar"].Value = ""; }
                }

                foreach (var sta in tra.Departures.Keys.OrderByDescending(s => s.Kilometre))
                {
                    try { trainRow.Cells[sta.Name + "dp"].Value = tra.Departures[sta].ToShortTimeString(); }
                    catch { trainRow.Cells[sta.Name + "dp"].Value = ""; }
                }

                trainRow.Tag = tra;
                trainRow.HeaderCell = new DataGridViewRowHeaderCell() { Value = tra.Name };
            }

            foreach (DataGridViewColumn column in bottomDataGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private bool GetTrainData(string trainName, out Dictionary<Station, DateTime> ar,
            out Dictionary<Station, DateTime> dp)
        {
            ar = new Dictionary<Station, DateTime>();
            dp = new Dictionary<Station, DateTime>();

            bool found = false;

            // Obere DataGridView
            foreach (DataGridViewRow row in topDataGridView.Rows)
            {
                Train t = (Train)row.Tag;

                if (t.Name != trainName)
                    continue;
                if (found)
                    break;

                foreach (var sta in tt.Stations)
                {
                    DataGridViewCell cellAr = row.Cells[sta.Name + "ar"];

                    DateTime dtAr;
                    DateTime.TryParse((string)cellAr.Value, out dtAr);

                    DataGridViewCell cellDp = row.Cells[sta.Name + "dp"];

                    DateTime dtDp;
                    DateTime.TryParse((string)cellDp.Value, out dtDp);

                    ar.Add(sta, dtAr);
                    dp.Add(sta, dtDp);
                }

                found = true;
            }

            // Untere DataGridView
            foreach (DataGridViewRow row in bottomDataGridView.Rows)
            {
                Train t = (Train)row.Tag;

                if (t.Name != trainName)
                    continue;
                if (found)
                    break;

                foreach (var sta in tt.Stations)
                {
                    DataGridViewCell cellAr = row.Cells[sta.Name + "ar"];

                    DateTime dtAr;
                    DateTime.TryParse((string)cellAr.Value, out dtAr);

                    DataGridViewCell cellDp = row.Cells[sta.Name + "dp"];

                    DateTime dtDp;
                    DateTime.TryParse((string)cellDp.Value, out dtDp);

                    ar.Add(sta, dtAr);
                    dp.Add(sta, dtDp);
                }

                found = true;
            }
 
            return found;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            foreach (var t in tt.Trains)
            {
                Dictionary<Station, DateTime> ar;
                Dictionary<Station, DateTime> dp;

                if (!GetTrainData(t.Name, out ar, out dp))
                    throw new Exception("In der Anwendung ist ein interner Fehler aufgetreten!");

                t.Arrivals = ar;
                t.Departures = dp;
            }

            topDataGridView.Rows.Clear();
            topDataGridView.Columns.Clear();

            bottomDataGridView.Rows.Clear();
            bottomDataGridView.Columns.Clear();

            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            tt = tt_undo;

            this.Close();
        }

        private void lineEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
