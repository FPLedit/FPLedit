using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace FPLedit.Editor.TimetableEditor
{
    internal abstract class BaseTimetableEditControl : Panel
    {
        private ToggleButton trapeztafelToggle;

        protected bool mpmode = false;

        public bool Initialized { get; protected set; }

        // Actions
        private TableLayout internalActionsLayout;

        protected readonly ObservableCollection<Control> actionButtons;
        private readonly TimeNormalizer timeNormalizer = new TimeNormalizer();
        
        public IList<Control> ActionButtons => actionButtons;

        protected BaseTimetableEditControl()
        {
            actionButtons = new ObservableCollection<Control>();
            actionButtons.CollectionChanged += (_, e) =>
            {
                var row = internalActionsLayout.Rows[0];
                if (e.Action == NotifyCollectionChangedAction.Add)
                    foreach (Control btn in e.NewItems)
                        row.Cells.Add(btn);
                if (e.Action == NotifyCollectionChangedAction.Remove)
                    foreach (Control btn in e.OldItems)
                        row.Cells.Remove(btn);
            };
        }

        protected void Init(ToggleButton trapeztafelToggle, TableLayout actionsLayout)
        {
            this.trapeztafelToggle = trapeztafelToggle;
            internalActionsLayout = actionsLayout;
            mpmode = !Eto.Platform.Instance.SupportedFeatures.HasFlag(Eto.PlatformFeatures.CustomCellSupportsControlView);
        }

        protected void FormatCell(BaseTimetableDataElement data, Station sta, bool arrival, TextBox tb)
        {
            string val = tb.Text;
            if (string.IsNullOrEmpty(val))
            {
                data.SetError(sta, arrival, null);
                data.SetTime(sta, arrival, "0");
                return;
            }

            val = timeNormalizer.Normalize(val);
            bool error = true;
            if (val != null)
            {
                tb.Text = val;
                data.SetTime(sta, arrival, val);
                error = false;
            }
            data.SetError(sta, arrival, error ? tb.Text : null);
        }

        protected void Trapez(GridView view)
        {
            if (view.SelectedRow == -1)
                return;

            var data = (BaseTimetableDataElement)view.SelectedItem;

            // Trapeztafelhalt darf nur bei Ankünften sein
            if (!data.IsSelectedArrival)
                return;

            var sta = data.GetStation();
            var trapez = !data.ArrDeps[sta].TrapeztafelHalt;
            data.SetTrapez(sta, trapez);

            view.ReloadData(view.SelectedRow);
            trapeztafelToggle.Checked = trapez;
            CellSelected(data, sta, data.IsSelectedArrival);
        }

        protected void Zuglaufmeldung(GridView view)
        {
            if (view.SelectedRow == -1)
                return;

            var data = (BaseTimetableDataElement)view.SelectedItem;
            var sta = data.GetStation();

            // Zuglaufmeldungen dürfen auch bei Abfahrt am ersten Bahnhof sein
            if (!data.IsFirst(sta) && !data.IsSelectedArrival)
                return;

            using (var zlmDialog = new ZlmEditForm(data.ArrDeps[sta].Zuglaufmeldung))
            {
                if (zlmDialog.ShowModal(this) != DialogResult.Ok)
                    return;

                data.SetZlm(sta, zlmDialog.Zlm);
            }

            view.ReloadData(view.SelectedRow);
        }

        protected virtual void HandleKeystroke(KeyEventArgs e, GridView view)
        {
            if (view == null)
                return;

            if (e.Handled)
                return;

            if (e.Key == Keys.Enter)
            {
                if (!mpmode)
                    return;

                e.Handled = true;

                var data = (BaseTimetableDataElement)view.SelectedItem;
                if (data == null || data.GetStation() == null || data.SelectedTextBox == null)
                    return;
                FormatCell(data, data.GetStation(), data.IsSelectedArrival, data.SelectedTextBox);

                view.ReloadData(view.SelectedRow);
            }
            else if (e.Key == Keys.T)
            {
                e.Handled = true;
                Trapez(view);
            }
            else if (e.Key == Keys.Z)
            {
                e.Handled = true;
                Zuglaufmeldung(view);
            }
            else if (e.Key == Keys.Tab)
            {
                if (!mpmode)
                    return;

                e.Handled = true;

                var data = (BaseTimetableDataElement)view.SelectedItem;
                if (data == null || data.GetStation() == null)
                    return;

                if (data.SelectedTextBox !=  null)
                    FormatCell(data, data.GetStation(), data.IsSelectedArrival, data.SelectedTextBox);

                var target = GetNextEditingPosition(data, view, e);
                //Console.WriteLine("Next pos: " + target.ToString());

                view.ReloadData(view.SelectedRow); // Commit current data
                view.BeginEdit(target.X, target.Y);
            }
            else if (e.Key == Keys.Escape)
            {
                e.Handled = true;
                view.CancelEdit();
            }
            if (e.Key == Keys.Down)
            {
                var data = (BaseTimetableDataElement)view.SelectedItem;
                if (data == null || data.GetStation() == null || data.SelectedDropDown == null)
                    return;
                
                e.Handled = true;
                var idx = data.SelectedDropDown.SelectedIndex + 1;
                if (idx < data.SelectedDropDown.DataStore.Count())
                    data.SelectedDropDown.SelectedIndex = idx;
                
            }
            if (e.Key == Keys.Up)
            {
                var data = (BaseTimetableDataElement)view.SelectedItem;
                if (data == null || data.GetStation() == null || data.SelectedDropDown == null)
                    return;
                
                e.Handled = true;
                var idx = data.SelectedDropDown.SelectedIndex - 1;
                if (idx >= 0)
                    data.SelectedDropDown.SelectedIndex = idx;
            }
            else
            {
                if (!mpmode)
                    return;

                var data = (BaseTimetableDataElement)view.SelectedItem;
                if (data == null || data.SelectedTextBox == null)
                    return;
                var tb = data.SelectedTextBox;
                if (tb.HasFocus || tb.ReadOnly) // Wir können "echt" editieren / sind read-Only
                    return;
                if (char.IsLetterOrDigit(e.KeyChar) || char.IsPunctuation(e.KeyChar))
                {
                    tb.Text += e.KeyChar;
                    e.Handled = true;
                }
                if (e.Key == Keys.Backspace && tb.Text.Length > 0)
                    tb.Text = tb.Text.Substring(0, tb.Text.Length - 1);
            }
        }

        protected void HandleViewKeystroke(KeyEventArgs e, GridView view)
        {
            if (e.Key == Keys.Home) // Pos1
            {
                if (!mpmode)
                    return;

                e.Handled = true;
                if (view.IsEditing)
                    view.ReloadData(view.SelectedRow);
                view.BeginEdit(0, FirstEditingColumn); // erstes Abfahrtsfeld
            }
            else
                HandleKeystroke(e, view);
        }

        protected abstract void CellSelected(BaseTimetableDataElement data, Station sta, bool arrival);

        protected abstract Point GetNextEditingPosition(BaseTimetableDataElement data, GridView view, KeyEventArgs e);

        protected abstract int FirstEditingColumn { get; }
    }
}
