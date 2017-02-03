using FPLedit.Shared;
using System;
using System.Windows.Forms;

namespace FPLedit.Standard
{
    public partial class TrainEditForm : Form
    {
        public Train Train { get; set; }

        private CheckBox[] daysBoxes;

        public TrainEditForm()
        {
            InitializeComponent();
            daysBoxes = new[] { mondayCheckBox, tuesdayCheckBox, wednesdayCheckBox, thursdayCheckBox, fridayCheckBox, saturdayCheckBox, sundayCheckBox };
        }

        public TrainEditForm(Train train) : this()
        {
            Train = train;           
            nameTextBox.Text = train.TName;
            locomotiveComboBox.Text = train.Locomotive;
            locomotiveComboBox.Items.AddRange(train._parent.GetAllTfzs());
            commentTextBox.Text = train.Comment;

            for (int i = 0; i < Train.Days.Length; i++)
                daysBoxes[i].Checked = Train.Days[i];

            Text = "Zug bearbeiten";
        }

        public TrainEditForm(Timetable tt, TrainDirection direction) : this()
        {
            Train = new Train(direction, tt);
            locomotiveComboBox.Items.AddRange(tt.GetAllTfzs());
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            if (!nameValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            Train.TName = nameTextBox.Text;
            Train.Locomotive = locomotiveComboBox.Text;
            Train.Comment = commentTextBox.Text;

            for (int i = 0; i < daysBoxes.Length; i++)
                Train.Days[i] = daysBoxes[i].Checked;

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
