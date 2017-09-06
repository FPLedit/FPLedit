using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    public partial class FilterForm : Form
    {
        private List<FilterRule> curTrainRules, curStationRules;
        private FilterableContainer[] fcontainers;
        private IInfo info;

        public FilterForm()
        {
            InitializeComponent();
        }

        public FilterForm(IInfo info): this()
        {
            this.info = info;
            var tt = info.Timetable;

            var filterables = info.GetRegistered<IFilterableUi>();
            fcontainers = filterables.Select(f => new FilterableContainer()
            {
                Filterable = f,
                StationRules = f.LoadStationRules(tt),
                TrainRules = f.LoadTrainRules(tt),
            }).ToArray();

            typeListBox.Items.AddRange(filterables.Select(f => f.DisplayName).ToArray());

            trainPattListView.Columns.Add("Typ");
            trainPattListView.Columns.Add("Suchwert");

            stationPattListView.Columns.Add("Typ");
            stationPattListView.Columns.Add("Suchwert");

            if (fcontainers.Length == 0)
                return;
            SwitchType(0);
            typeListBox.SelectedIndex = 0;
        }

        private void SwitchType(int idx)
        {
            var f = fcontainers[idx];
            curTrainRules = f.TrainRules;
            curStationRules = f.StationRules;
            Text = "Filterregeln für " + f.Filterable.DisplayName;

            UpdateListView(stationPattListView, curStationRules);
            UpdateListView(trainPattListView, curTrainRules);
        }

        private void UpdateListView(ListView view, List<FilterRule> rules)
        {
            view.Items.Clear();
            var patterns = rules.Select(r => r.Pattern);
            foreach (var pattern in patterns)
            {
                var type = TypeDescription(pattern[0]);
                var rest = pattern.Substring(1);
                view.Items.Add(new ListViewItem(new[] { type, rest }));
            }
        }

        private void DeleteEntry(ListView view, List<FilterRule> patterns, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                var itm = view.SelectedItems[0];
                patterns.RemoveAt(itm.Index);
                view.Items.Remove(itm);
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Regel ausgewählt werden!", "Regel löschen");
        }

        private void EditEntry(ListView view, List<FilterRule> patterns, string property, bool message=true)
        {
            if (view.SelectedItems.Count > 0)
            {
                var itm = view.SelectedItems[0];

                var epf = new EditPatternForm(patterns[itm.Index].Pattern, property);
                if (epf.ShowDialog() == DialogResult.OK)
                {
                    patterns[itm.Index] = new FilterRule(epf.Pattern);

                    itm.SubItems[0].Text = TypeDescription(epf.Pattern[0]);
                    itm.SubItems[1].Text = epf.Pattern.Substring(1);
                }
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Regel ausgewählt werden!", "Regel bearbeiten");
        }

        private void AddEntry(ListView view, List<FilterRule> patterns, string property)
        {
            var epf = new EditPatternForm(property);
            if (epf.ShowDialog() == DialogResult.OK)
            {
                patterns.Add(new FilterRule(epf.Pattern));

                var type = TypeDescription(epf.Pattern[0]);
                var rest = epf.Pattern.Substring(1);
                view.Items.Add(new ListViewItem(new[] { type, rest }));
            }
        }

        private string TypeDescription(char type)
        {
            switch (type)
            {
                case '^':
                    return "beginnt mit";
                case '$':
                    return "endet mit";
                case ' ':
                    return "enthält";
                case '=':
                    return "ist";
                default:
                    return "";
            }
        }

        #region Events

        private void addTrainPattButton_Click(object sender, EventArgs e)
            => AddEntry(trainPattListView, curTrainRules, "Zugname");

        private void editTrainPattButton_Click(object sender, EventArgs e)
            => EditEntry(trainPattListView, curTrainRules, "Zugname");

        private void deleteTrainPattButton_Click(object sender, EventArgs e)
            => DeleteEntry(trainPattListView, curTrainRules);

        private void addStationPattButton_Click(object sender, EventArgs e)
            => AddEntry(stationPattListView, curStationRules, "Stationsname");

        private void editStationPattButton_Click(object sender, EventArgs e)
            => EditEntry(stationPattListView, curStationRules, "Stationsname");

        private void deleteStationPattButton_Click(object sender, EventArgs e)
            => DeleteEntry(stationPattListView, curStationRules);

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
            => SwitchType(typeListBox.SelectedIndex);

        private void closeButton_Click(object sender, EventArgs e)
        {
            foreach (var fcon in fcontainers)
            {
                fcon.Filterable.SaveFilter(info.Timetable, fcon.StationRules, fcon.TrainRules);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        #endregion

        private class FilterableContainer
        {
            public IFilterableUi Filterable;
            public List<FilterRule> TrainRules, StationRules;
        }
    }
}
