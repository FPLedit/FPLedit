using Buchfahrplan.Shared;
using System;
using System.Windows.Forms;

namespace Buchfahrplan.Standard
{
    public partial class TrainEditForm : Form
    {
        public Train NewTrain { get; set; }

        private bool direction;

        public TrainEditForm()
        {
            InitializeComponent();
        }

        public void Initialize(Train train)
        {
            NewTrain = train;
            nameTextBox.Text = train.Name;
            lineTextBox.Text = train.Line;
            direction = train.Direction;
            locomotiveTextBox.Text = train.Locomotive;

            mondayCheckBox.Checked = NewTrain.Days[0];
            tuesdayCheckBox.Checked = NewTrain.Days[1];
            wednesdayCheckBox.Checked = NewTrain.Days[2];
            thursdayCheckBox.Checked = NewTrain.Days[3];
            fridayCheckBox.Checked = NewTrain.Days[4];
            saturdayCheckBox.Checked = NewTrain.Days[5];
            sundayCheckBox.Checked = NewTrain.Days[6];
            Text = "Zug bearbeiten";
        }

        public void Initialize(bool direction)
        {
            this.direction = direction;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (NewTrain == null)
                NewTrain = new Train();

            NewTrain.Name = nameTextBox.Text;
            NewTrain.Line = lineTextBox.Text;
            NewTrain.Locomotive = locomotiveTextBox.Text;
            NewTrain.Direction = direction;
            NewTrain.Days[0] = mondayCheckBox.Checked;
            NewTrain.Days[1] = tuesdayCheckBox.Checked;
            NewTrain.Days[2] = wednesdayCheckBox.Checked;
            NewTrain.Days[3] = thursdayCheckBox.Checked;
            NewTrain.Days[4] = fridayCheckBox.Checked;
            NewTrain.Days[5] = saturdayCheckBox.Checked;
            NewTrain.Days[6] = sundayCheckBox.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
