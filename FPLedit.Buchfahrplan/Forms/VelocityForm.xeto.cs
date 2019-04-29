using Eto.Forms;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Buchfahrplan.Forms
{
    internal class VelocityForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly GridView gridView;
        private readonly Button deleteButton;
#pragma warning restore CS0649

        private readonly IInfo info;
        private readonly Route route;
        private readonly Timetable tt;
        private readonly BfplAttrs attrs;

        private VelocityForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            gridView.AddColumn<IStation>(s => s.Positions.GetPosition(route.Index).ToString(), "km");
            gridView.AddColumn<IStation>(s => s.SName, "Name");
            gridView.AddColumn<IStation>(s => s.Vmax.GetValue(route.Index), "Vmax");
            gridView.AddColumn<IStation>(s => s.Wellenlinien.GetValue(route.Index).ToString(), "Wellenlinien");

            gridView.MouseDoubleClick += (s, e) => EditPoint(false);

            gridView.SelectedItemsChanged += (s, e) => SelectPoint();

            this.AddCloseHandler();
            this.AddSizeStateHandler();
        }

        public VelocityForm(IInfo info, Route route) : this()
        {
            this.info = info;
            tt = info.Timetable;
            this.route = route;

            attrs = BfplAttrs.GetAttrs(tt);
            if (attrs == null)
            {
                attrs = new BfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);
            }

            info.BackupTimetable();
            UpdateListView();
        }

        private void UpdateListView()
        {
            List<IStation> points = new List<IStation>();
            points.AddRange(route.Stations);
            if (attrs != null)
                points.AddRange(attrs.GetPoints(route.Index));

            gridView.DataStore = points.OrderBy(o => o.Positions.GetPosition(route.Index));
        }

        private void AddPoint()
        {
            VelocityEditForm vef = new VelocityEditForm(tt, route.Index);
            if (vef.ShowModal(this) == DialogResult.Ok)
            {
                var p = (BfplPoint)vef.Station;

                var pos = p.Positions.GetPosition(route.Index);
                if (pos < route.MinPosition || pos > route.MaxPosition)
                {
                    MessageBox.Show($"Die Position muss im Streckenbereich liegen, also zwischen {route.MinPosition} und {route.MaxPosition}!", "FPLedit");
                    return;
                }
                if (attrs != null)
                    attrs.AddPoint(p);
                UpdateListView();
            }
        }

        private void EditPoint(bool message = true)
        {
            if (gridView.SelectedItem != null)
            {
                var sta = (IStation)gridView.SelectedItem;

                VelocityEditForm vef = new VelocityEditForm(sta, route.Index);
                if (vef.ShowModal(this) == DialogResult.Ok)
                    UpdateListView();
            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Zeile ausgewählt werden!", "Höchstgeschwindigkeit ändern");
        }

        private void RemovePoint(bool message = true)
        {
            if (gridView.SelectedItem != null)
            {
                var sta = gridView.SelectedItem;

                if (sta is Station)
                    throw new NotSupportedException("Bahnhöfe können nicht gelöscht werden!");
                else if (sta is BfplPoint point)
                {
                    if (attrs != null)
                        attrs.RemovePoint(point);

                    UpdateListView();
                }

            }
            else if (message)
                MessageBox.Show("Zuerst muss eine Zeile ausgewählt werden!", "Löschen");
        }

        private void SelectPoint()
        {
            deleteButton.Enabled = (gridView.SelectedItem is BfplPoint);
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

        #region Events
        private void editButton_Click(object sender, EventArgs e)
            => EditPoint();

        private void addButton_Click(object sender, EventArgs e)
            => AddPoint();

        private void deleteButton_Click(object sender, EventArgs e)
            => RemovePoint();
        #endregion
    }
}
