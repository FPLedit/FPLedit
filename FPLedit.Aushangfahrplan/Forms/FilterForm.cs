using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Aushangfahrplan.Forms
{
    public partial class FilterForm : Form
    {
        private AfplAttrs attrs;

        private List<string> trainPatterns, stationPatterns;

        public FilterForm()
        {
            InitializeComponent();
        }

        public FilterForm(Timetable tt): this()
        {
            trainPattListView.Columns.Add("Typ");
            trainPattListView.Columns.Add("Suchwert");

            stationPattListView.Columns.Add("Typ");
            stationPattListView.Columns.Add("Suchwert");

            attrs = AfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                trainPatterns = attrs.TrainPatterns.Split('|').Where(p => p != "").ToList();
                stationPatterns = attrs.StationPatterns.Split('|').Where(p => p != "").ToList();

                UpdateListView(trainPattListView, trainPatterns);
                UpdateListView(stationPattListView, stationPatterns);
            }
            else
            {
                attrs = new AfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);

                trainPatterns = new List<string>();
                stationPatterns = new List<string>();
            }
        }

        private void UpdateListView(ListView view, List<string> patterns)
        {
            view.Items.Clear();
            foreach (var pattern in patterns)
            {
                var type = TypeDescription(pattern[0]);
                var rest = pattern.Substring(1);
                view.Items.Add(new ListViewItem(new[] { type, rest }));
            }
        }

        private void DeleteEntry(ListView view, List<string> patterns, bool message = true)
        {
            if (view.SelectedItems.Count > 0)
            {
                var itm = view.SelectedItems[0];
                patterns.RemoveAt(itm.Index);
                view.Items.Remove(itm);
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Regel ausgewählt werden!", "Reegel löschen");
        }

        private void EditEntry(ListView view, List<string> patterns, string property, bool message=true)
        {
            if (view.SelectedItems.Count > 0)
            {
                var itm = view.SelectedItems[0];

                var epf = new EditPatternForm(patterns[itm.Index], property);
                if (epf.ShowDialog() == DialogResult.OK)
                {
                    patterns[itm.Index] = epf.Pattern;

                    itm.SubItems[0].Text = TypeDescription(epf.Pattern[0]);
                    itm.SubItems[1].Text = epf.Pattern.Substring(1);
                }
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Regel ausgewählt werden!", "Regel bearbeiten");
        }

        private void AddEntry(ListView view, List<string> patterns, string property)
        {
            var epf = new EditPatternForm(property);
            if (epf.ShowDialog() == DialogResult.OK)
            {
                patterns.Add(epf.Pattern);

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
            => AddEntry(trainPattListView, trainPatterns, "Zugname");

        private void editTrainPattButton_Click(object sender, EventArgs e)
            => EditEntry(trainPattListView, trainPatterns, "Zugname");

        private void deleteTrainPattButton_Click(object sender, EventArgs e)
            => DeleteEntry(trainPattListView, trainPatterns);

        private void addStationPattButton_Click(object sender, EventArgs e)
            => AddEntry(stationPattListView, stationPatterns, "Stationsname");

        private void editStationPattButton_Click(object sender, EventArgs e)
            => EditEntry(stationPattListView, stationPatterns, "Stationsname");

        private void closeButton_Click(object sender, EventArgs e)
        {
            attrs.TrainPatterns = string.Join("|", trainPatterns.ToArray());
            attrs.StationPatterns = string.Join("|", stationPatterns.ToArray());

            DialogResult = DialogResult.OK;
            Close();
        }

        private void deleteStationPattButton_Click(object sender, EventArgs e)
            => DeleteEntry(stationPattListView, stationPatterns);

        #endregion
    }
}
