using FPLedit.Shared;
using System;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    public partial class TrainEditForm : Form
    {
        public Train Train { get; set; }

        private TrainDirection direction;

        private CheckBox[] daysBoxes;

        public TrainEditForm()
        {
            InitializeComponent();
            daysBoxes = new[] { mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox };
        }

        public TrainEditForm(Train train) : this()
        {
            Train = train;
            nameTextBox.Text = train.Name;
            lineTextBox.Text = train.Line;
            direction = train.Direction;
            locomotiveTextBox.Text = train.Locomotive;

            for (int i = 0; i < Train.Days.Length; i++)
                daysBoxes[i].Checked = Train.Days[i];

            Text = "Zug bearbeiten";
        }

        public TrainEditForm(Timetable tt, TrainDirection direction) : this()
        {
            this.direction = direction;
            lineTextBox.Text = tt.GetLineName(direction);
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (Train == null)
                Train = new Train();

            if (!nameValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            Train.Name = nameTextBox.Text;
            Train.Line = lineTextBox.Text;
            Train.Locomotive = locomotiveTextBox.Text;
            Train.Direction = direction;

            for (int i = 0; i < daysBoxes.Length; i++)
                Train.Days[i] = daysBoxes[i].Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
