using FPLedit.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FPLedit.BildfahrplanExport
{
    public partial class TrainColorEditForm : Form
    {
        public Train Train { get; set; }

        private TrainStyle style;

        public TrainColorEditForm()
        {
            InitializeComponent();

            colorComboBox.Items.AddRange(ColorHelper.ColorNames);
            for (int i = 1; i <= 5; i++)
                widthComboBox.Items.Add(i);
        }

        public TrainColorEditForm(Train train) : this()
        {
            Train = train;
            style = new TrainStyle(train);
            var attrs = new TimetableStyle(train._parent);

            colorComboBox.SelectedItem = ColorHelper.NameFromColor(style.TrainColor ?? attrs.TrainColor);
            widthComboBox.SelectedItem = style.TrainWidth ?? attrs.TrainWidth;
            drawCheckBox.Checked = style.Show;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            style.TrainColor = ColorHelper.ColorFromName((string)colorComboBox.SelectedItem);
            style.TrainWidth = (int)widthComboBox.SelectedItem;
            style.Show = drawCheckBox.Checked;
            Close();
        }
    }
}
