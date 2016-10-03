using Buchfahrplan.Shared;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Buchfahrplan.BildfahrplanExport
{
    public partial class TrainColorEditForm : Form
    {
        public Train Train { get; set; }

        public TrainColorEditForm()
        {
            InitializeComponent();

            colorComboBox.Items.AddRange(colors);
            colorComboBox.SelectedItem = "Black";
            for (int i = 1; i <= 5; i++) widthComboBox.Items.Add(i.ToString());
            widthComboBox.SelectedItem = "1";
            drawCheckBox.Checked = true;
        }

        public void Initialize(Train train)
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

        private string[] colors = new[]
        {
            "Black",
            "Gray",
            "White",
            "Red",
            "Orange",
            "Yellow",
            "Blue",
            "LightBlue",
            "Green",
            "DrakGreen",
            "Brown",
            "Magenta"
        };
    }
}
