using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor.Filters
{
    internal class FilterForm : FDialog<DialogResult>
    {
        private List<FilterRule> curTrainRules, curStationRules;
        private FilterableContainer[] fcontainers;
        private IInfo info;

#pragma warning disable CS0649
        private GridView trainPattListView, stationPattListView;
        private ListBox typeListBox;
#pragma warning restore CS0649

        public FilterForm(IInfo info)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.info = info;
            var tt = info.Timetable;

            var filterables = info.GetRegistered<IFilterableUi>();
            fcontainers = filterables.Select(f => new FilterableContainer()
            {
                Filterable = f,
                StationRules = f.LoadStationRules(tt),
                TrainRules = f.LoadTrainRules(tt),
            }).ToArray();

            typeListBox.Items.AddRange(filterables.Select(f => new ListItem() { Text = f.DisplayName }));
            typeListBox.SelectedIndexChanged += listBox1_SelectedIndexChanged;

            InitView(trainPattListView);
            InitView(stationPattListView);

            if (fcontainers.Length == 0)
                return;
            SwitchType(0);
            typeListBox.SelectedIndex = 0;

            this.AddSizeStateHandler();
        }

        private void InitView(GridView view)
        {
            view.AddColumn<FilterRule>(r => TypeDescription(r.FilterType, r.Negate), "Typ");
            view.AddColumn<FilterRule>(r => r.SearchString, "Suchwert");
        }

        private void SwitchType(int idx)
        {
            var f = fcontainers[idx];
            curTrainRules = f.TrainRules;
            curStationRules = f.StationRules;
            Title = "Filterregeln für " + f.Filterable.DisplayName;

            UpdateListView(stationPattListView, curStationRules);
            UpdateListView(trainPattListView, curTrainRules);
        }

        private void UpdateListView(GridView view, List<FilterRule> rules)
        {
            view.DataStore = rules;
        }

        private void DeleteEntry(GridView view, List<FilterRule> patterns, bool message = true)
        {
            if (view.SelectedItem != null)
            {
                var itm = (FilterRule)view.SelectedItem;
                patterns.Remove(itm);
                UpdateListView(view, patterns);
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Regel ausgewählt werden!", "Regel löschen");
        }

        private void EditEntry(GridView view, List<FilterRule> patterns, string property, bool message=true)
        {
            if (view.SelectedItem != null)
            {
                var frule = (FilterRule)view.SelectedItem;

                var epf = new EditPatternForm(frule, property);
                if (epf.ShowModal(this) == DialogResult.Ok)
                {
                    patterns[view.SelectedRow] = epf.Pattern;

                    UpdateListView(view, patterns);
                }
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Regel ausgewählt werden!", "Regel bearbeiten");
        }

        private void AddEntry(GridView view, List<FilterRule> patterns, string property)
        {
            var epf = new EditPatternForm(property);
            if (epf.ShowModal(this) == DialogResult.Ok)
            {
                patterns.Add(epf.Pattern);

                UpdateListView(view, patterns);
            }
        }

        private string TypeDescription(FilterType type, bool negate)
        {
            switch (type)
            {
                case FilterType.StartsWith:
                    return negate ? "beginnt nicht mit" : "beginnt mit";
                case FilterType.EndsWidth:
                    return negate ? "endet nicht mit" : "endet mit";
                case FilterType.Contains:
                    return negate ? "enthält nicht" : "enthält";
                case FilterType.Equals:
                    return negate ? "ist nicht" : "ist";
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
                fcon.Filterable.SaveFilter(info.Timetable, fcon.StationRules, fcon.TrainRules);

            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        #endregion

        private class FilterableContainer
        {
            public IFilterableUi Filterable;
            public List<FilterRule> TrainRules, StationRules;
        }
    }
}
