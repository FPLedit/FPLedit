using Eto.Forms;
using FPLedit.BildfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Drawing;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.BildfahrplanExport.Forms
{
    public partial class TrainColorEditForm : Dialog<DialogResult>
    {
        public Train Train { get; set; }

        private TrainStyle style;
        private ColorCollection cc;
        private DashStyleHelper ds;

#pragma warning disable CS0649
        private DropDown colorComboBox, widthComboBox, dashComboBox;
        private CheckBox drawCheckBox;
#pragma warning restore CS0649

        private TrainColorEditForm(ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            cc = new ColorCollection(settings);
            ds = new DashStyleHelper();

            colorComboBox.DataStore = cc.ColorHexStrings;
            colorComboBox.ItemTextBinding = cc.ColorBinding;

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

        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;

            style.TrainColor = ColorFormatter.FromHexString((string)colorComboBox.SelectedValue);
            style.TrainWidth = (int)widthComboBox.SelectedValue;
            style.LineStyle = (int)dashComboBox.SelectedValue;
            style.Show = drawCheckBox.Checked.Value;
            Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Cancel;
            Close();
        }
    }
}
