using Eto.Forms;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Linq;
using FPLedit.Shared.Rendering;
using FPLedit.Shared.UI;

namespace FPLedit.Bildfahrplan.Forms
{
    public partial class TrainColorEditForm : FDialog<DialogResult>
    {
        public Train Train { get; set; }

        private readonly TrainStyle style;
        private readonly ColorCollection cc;
        private readonly DashStyleHelper ds;

#pragma warning disable CS0649
        private readonly DropDown colorComboBox, widthComboBox, dashComboBox;
        private readonly CheckBox drawCheckBox;
#pragma warning restore CS0649

        private TrainColorEditForm(ISettings settings)
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

        public TrainColorEditForm(Train train, ISettings settings) : this(settings)
        {
            Train = train;
            style = new TrainStyle(train);
            var attrs = new TimetableStyle(train._parent);

            colorComboBox.SelectedValue = ColorFormatter.ToString(style.TrainColor ?? attrs.TrainColor);
            widthComboBox.SelectedValue = style.TrainWidth ?? attrs.TrainWidth;
            dashComboBox.SelectedValue = style.LineStyle;
            drawCheckBox.Checked = style.Show;
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            style.TrainColor = ColorFormatter.FromHexString((string)colorComboBox.SelectedValue);
            style.TrainWidth = (int)widthComboBox.SelectedValue;
            style.LineStyle = (int)dashComboBox.SelectedValue;
            style.Show = drawCheckBox.Checked.Value;
            Close(DialogResult.Ok);
        }

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);
    }
}
