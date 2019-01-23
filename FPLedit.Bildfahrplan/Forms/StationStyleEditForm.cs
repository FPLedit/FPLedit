using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Linq;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;

namespace FPLedit.Bildfahrplan.Forms
{
    public partial class StationStyleEditForm : FDialog<DialogResult>
    {
        public Station Station { get; set; }

        private StationStyle style;
        private ColorCollection cc;
        private DashStyleHelper ds;

#pragma warning disable CS0649
        private DropDown colorComboBox, widthComboBox, dashComboBox;
        private CheckBox drawCheckBox;
#pragma warning restore CS0649

        private StationStyleEditForm(ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            cc = new ColorCollection(settings);
            ds = new DashStyleHelper();

            colorComboBox.DataStore = cc.ColorHexStrings;
            colorComboBox.ItemTextBinding = Shared.UI.ExtBind.ColorBinding(cc);

            widthComboBox.DataStore = Enumerable.Range(1, 5).Cast<object>();
            widthComboBox.ItemTextBinding = Binding.Property<int, string>(c => c.ToString());

            dashComboBox.DataStore = ds.Indices.Cast<object>();
            dashComboBox.ItemTextBinding = Binding.Property<int, string>(i => ds.GetDescription(i));
        }

        public StationStyleEditForm(Station station, ISettings settings) : this(settings)
        {
            Station = station;
            style = new StationStyle(station);
            var attrs = new TimetableStyle(station._parent);

            colorComboBox.SelectedValue = ColorFormatter.ToString(style.StationColor ?? attrs.StationColor);
            widthComboBox.SelectedValue = style.StationWidth ?? attrs.StationWidth;
            dashComboBox.SelectedValue = style.LineStyle;
            drawCheckBox.Checked = style.Show;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            style.StationColor = ColorFormatter.FromHexString((string)colorComboBox.SelectedValue);
            style.StationWidth = (int)widthComboBox.SelectedValue;
            style.LineStyle = (int)dashComboBox.SelectedValue;
            style.Show = drawCheckBox.Checked.Value;
            Close(DialogResult.Ok);
        }

        private void cancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
