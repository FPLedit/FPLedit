using Eto.Forms;
using FPLedit.BildfahrplanExport.Helpers;
using FPLedit.BildfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Drawing;
using System.Linq;

namespace FPLedit.BildfahrplanExport
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

        public TrainColorEditForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            cc = new ColorCollection();
            ds = new DashStyleHelper();

            colorComboBox.DataStore = cc.ColorHexStrings;
            colorComboBox.ItemTextBinding = cc.ColorBinding;

            widthComboBox.DataStore = Enumerable.Range(1, 5).Cast<object>();
            widthComboBox.ItemTextBinding = Binding.Property<int, string>(c => c.ToString());

            dashComboBox.DataStore = ds.Indices.Cast<object>();
            dashComboBox.ItemTextBinding = Binding.Property<int, string>(i => ds.GetDescription(i));
        }

        public TrainColorEditForm(Train train) : this()
        {
            Train = train;
            style = new TrainStyle(train);
            var attrs = new TimetableStyle(train._parent);

            colorComboBox.SelectedValue = ColorHelper.ToHexString(style.TrainColor ?? attrs.TrainColor);
            widthComboBox.SelectedValue = style.TrainWidth ?? attrs.TrainWidth;
            dashComboBox.SelectedValue = style.LineStyle;
            drawCheckBox.Checked = style.Show;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Result = DialogResult.Ok;

            style.TrainColor = ColorHelper.FromHexString((string)colorComboBox.SelectedValue);
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
