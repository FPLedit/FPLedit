using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Editor.Filters
{
    internal sealed class FilterForm : FDialog<DialogResult>
    {
        private readonly FilterableContainer[] fcontainers;
        private readonly IPluginInterface pluginInterface;

        private List<FilterRule> curTrainRules, curStationRules;

#pragma warning disable CS0649
        private readonly GridView trainPattListView, stationPattListView;
        private readonly ListBox typeListBox;
#pragma warning restore CS0649

        public FilterForm(IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            this.pluginInterface = pluginInterface;
            var tt = pluginInterface.Timetable;

            var filterables = pluginInterface.GetRegistered<IFilterRuleContainer>();
            fcontainers = filterables.Select(f => new FilterableContainer()
            {
                Filterable = f,
                StationRules = f.LoadStationRules(tt).ToList(),
                TrainRules = f.LoadTrainRules(tt).ToList(),
            }).ToArray();

            typeListBox.Items.AddRange(filterables.Select(f => new ListItem() { Text = f.DisplayName }));
            typeListBox.SelectedIndexChanged += TypeListBox_SelectedIndexChanged;

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
            view.AddColumn<FilterRule>(r => TypeDescription(r.FilterType, r.Negate), T._("Typ"));
            view.AddColumn<FilterRule>(r => r.SearchString, T._("Suchwert"));
        }

        private void SwitchType(int idx)
        {
            var f = fcontainers[idx];
            curTrainRules = f.TrainRules;
            curStationRules = f.StationRules;
            Title = T._("Filterregeln für {0}", f.Filterable.DisplayName);
            
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
                MessageBox.Show(T._("Zuerst muss eine Regel ausgewählt werden!"), T._("Regel löschen"));
        }

        private void EditEntry(GridView view, List<FilterRule> patterns, string property, FilterTarget target, bool message=true)
        {
            if (view.SelectedItem != null)
            {
                var frule = (FilterRule)view.SelectedItem;

                using (var epf = new EditPatternForm(frule, property, target))
                {
                    if (epf.ShowModal(this) == DialogResult.Ok)
                    {
                        patterns[view.SelectedRow] = epf.Pattern;
                        UpdateListView(view, patterns);
                    }
                }
            }
            else if (message)
                MessageBox.Show(T._("Zuerst muss eine Regel ausgewählt werden!"), T._("Regel bearbeiten"));
        }

        private void AddEntry(GridView view, List<FilterRule> patterns, string property, FilterTarget target)
        {
            using (var epf = new EditPatternForm(property, target))
            {
                if (epf.ShowModal(this) == DialogResult.Ok)
                {
                    patterns.Add(epf.Pattern);
                    UpdateListView(view, patterns);
                }
            }
        }

        private string TypeDescription(FilterType type, bool negate)
        {
            switch (type)
            {
                case FilterType.StartsWith: return negate ? T._("beginnt nicht mit") : T._("beginnt mit");
                case FilterType.EndsWidth: return negate ? T._("endet nicht mit") : T._("endet mit");
                case FilterType.Contains: return negate ? T._("enthält nicht") : T._("enthält");
                case FilterType.Equals: return negate ? T._("ist nicht") : T._("ist");
                case FilterType.StationType: return negate ? T._("Betriebsst.-Typ ist nicht") : T._("Betriebsst.-Typ ist");
                default: return "";
            }
        }

        #region Events

        private void AddTrainPattButton_Click(object sender, EventArgs e)
            => AddEntry(trainPattListView, curTrainRules, "Zugname", FilterTarget.Train);

        private void EditTrainPattButton_Click(object sender, EventArgs e)
            => EditEntry(trainPattListView, curTrainRules, "Zugname", FilterTarget.Train);

        private void DeleteTrainPattButton_Click(object sender, EventArgs e)
            => DeleteEntry(trainPattListView, curTrainRules);

        private void AddStationPattButton_Click(object sender, EventArgs e)
            => AddEntry(stationPattListView, curStationRules, "Stationsname", FilterTarget.Station);

        private void EditStationPattButton_Click(object sender, EventArgs e)
            => EditEntry(stationPattListView, curStationRules, "Stationsname", FilterTarget.Station);

        private void DeleteStationPattButton_Click(object sender, EventArgs e)
            => DeleteEntry(stationPattListView, curStationRules);

        private void TypeListBox_SelectedIndexChanged(object sender, EventArgs e)
            => SwitchType(typeListBox.SelectedIndex);

        private void CloseButton_Click(object sender, EventArgs e)
        {
            foreach (var fcon in fcontainers)
                fcon.Filterable.SaveFilter(pluginInterface.Timetable, fcon.StationRules, fcon.TrainRules);

            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        #endregion

        private class FilterableContainer
        {
            public IFilterRuleContainer Filterable;
            public List<FilterRule> TrainRules, StationRules;
        }
        
        private static class L
        {
            public static readonly string Cancel = T._("Abbrechen");
            public static readonly string Close = T._("Schließen");
            public static readonly string Title = T._("Filterregeln");
            public static readonly string New = T._("&Hinzufügen");
            public static readonly string Edit = T._("&Bearbeiten");
            public static readonly string Delete = T._("&Löschen");
            public static readonly string New2 = T._("H&inzufügen");
            public static readonly string Edit2 = T._("B&earbeiten");
            public static readonly string Delete2 = T._("Lö&schen");
            public static readonly string FilterFor = T._("Filter für");
            public static readonly string Trains = T._("Züge ausblenden");
            public static readonly string Stations = T._("Bahnhöfe ausblenden");
        }
    }
}
