using Eto.Drawing;
using Eto.Forms;
using FPLedit.Editor.Network;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    internal abstract class TimetableEditorBase : Panel
    {
        private TimeNormalizer normalizer = new TimeNormalizer();

        private ToggleButton trapeztafelToggle;

        protected bool mpmode = false;

        protected void Init(ToggleButton trapeztafelToggle)
        {
            this.trapeztafelToggle = trapeztafelToggle;
            mpmode = !Eto.Platform.Instance.SupportedFeatures.HasFlag(Eto.PlatformFeatures.CustomCellSupportsControlView);
        }

        protected void FormatCell(TimetableDataElement data, Station sta, bool arrival, TextBox tb)
        {
            string val = tb.Text;
            if (val == null || val == "")
            {
                data.SetError(sta, arrival, null);
                data.SetTime(sta, arrival, "0");
                return;
            }

            val = normalizer.Normalize(val);
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

            var data = (TimetableDataElement)view.SelectedItem;

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

            var data = (TimetableDataElement)view.SelectedItem;
            var sta = data.GetStation();

            // Zuglaufmeldungen dürfen auch bei Abfahrt am ersten Bahnhof sein
            if (!data.IsFirst(sta) && !data.IsSelectedArrival)
                return;

            var zlmDialog = new ZlmEditForm(data.ArrDeps[sta].Zuglaufmeldung ?? "");
            if (zlmDialog.ShowModal(this) != DialogResult.Ok)
                return;

            data.SetZlm(sta, zlmDialog.Zlm);

            view.ReloadData(view.SelectedRow);
        }

        protected void HandleKeystroke(KeyEventArgs e, GridView view)
        {
            if (view == null)
                return;

            if (e.Key == Keys.Enter)
            {
                if (!mpmode)
                    return;

                e.Handled = true;

                var data = (TimetableDataElement)view.SelectedItem;
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
                var data = (TimetableDataElement)view.SelectedItem;
                if (data == null || data.GetStation() == null || data.SelectedTextBox == null)
                    return;
                FormatCell(data, data.GetStation(), data.IsSelectedArrival, data.SelectedTextBox);

                var target = GetNextEditingPosition(data, view, e);
                Console.WriteLine("Next pos: " + target.ToString());

                view.ReloadData(view.SelectedRow); // Commit current data
                view.BeginEdit(target.X, target.Y);
            }
            else
            {
                if (!mpmode)
                    return;

                var data = (TimetableDataElement)view.SelectedItem;
                if (data == null || data.SelectedTextBox == null)
                    return;
                var tb = data.SelectedTextBox;
                if (tb.HasFocus) // Wir können "echt" editieren
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

        protected abstract void CellSelected(TimetableDataElement data, Station sta, bool arrival);

        protected abstract Point GetNextEditingPosition(TimetableDataElement data, GridView view, KeyEventArgs e);

        protected abstract int FirstEditingColumn { get; }
    }
}
