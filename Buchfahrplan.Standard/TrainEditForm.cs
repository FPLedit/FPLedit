using Buchfahrplan.Shared;
using System;
using System.Windows.Forms;

namespace Buchfahrplan.Standard
{
    public partial class TrainEditForm : Form
    {
        public Train Train { get; set; }

        private bool direction;

        public TrainEditForm()
        {
            InitializeComponent();
        }

        public TrainEditForm(Train train) : this()
        {
            Train = train;
            nameTextBox.Text = train.Name;
            lineTextBox.Text = train.Line;
            direction = train.Direction;
            locomotiveTextBox.Text = train.Locomotive;

            mondayCheckBox.Checked = Train.Days[0];
            tuesdayCheckBox.Checked = Train.Days[1];
            wednesdayCheckBox.Checked = Train.Days[2];
            thursdayCheckBox.Checked = Train.Days[3];
            fridayCheckBox.Checked = Train.Days[4];
            saturdayCheckBox.Checked = Train.Days[5];
            sundayCheckBox.Checked = Train.Days[6];
            Text = "Zug bearbeiten";
        }

        public TrainEditForm(Timetable tt, bool direction) : this()
        {
            this.direction = direction;
            lineTextBox.Text = tt.GetLineName(direction);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (Train == null)
                Train = new Train();

            Train.Name = nameTextBox.Text;
            Train.Line = lineTextBox.Text;
            Train.Locomotive = locomotiveTextBox.Text;
            Train.Direction = direction;
            Train.Days[0] = mondayCheckBox.Checked;
            Train.Days[1] = tuesdayCheckBox.Checked;
            Train.Days[2] = wednesdayCheckBox.Checked;
            Train.Days[3] = thursdayCheckBox.Checked;
            Train.Days[4] = fridayCheckBox.Checked;
            Train.Days[5] = saturdayCheckBox.Checked;
            Train.Days[6] = sundayCheckBox.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
