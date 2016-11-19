using FPLedit.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FPLedit.BildfahrplanExport
{
    public partial class TrainColorEditForm : Form
    {
        public Train Train { get; set; }

        public TrainColorEditForm()
        {
            InitializeComponent();

            colorComboBox.Items.AddRange(ColorHelper.ColorNames);
            colorComboBox.SelectedItem = "Black";
            for (int i = 1; i <= 5; i++) widthComboBox.Items.Add(i.ToString());
            widthComboBox.SelectedItem = "1";
            drawCheckBox.Checked = true;
        }

        public TrainColorEditForm(Train train) : this()
        {
            Train = train;

            colorComboBox.SelectedItem = train.GetMeta("Color", (string)colorComboBox.SelectedItem);
            widthComboBox.SelectedItem = train.GetMeta("Width", (string)widthComboBox.SelectedItem);
            drawCheckBox.Checked = train.GetMetaBool("Draw", drawCheckBox.Checked);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;

            Train.Metadata["Color"] = (string)colorComboBox.SelectedItem;
            Train.Metadata["Width"] = (string)widthComboBox.SelectedItem;
            Train.Metadata["Draw"] = drawCheckBox.Checked.ToString();        
            Close();
        }
    }
}
