using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Buchfahrplan.FileModel;
using Buchfahrplan.Properties;

namespace Buchfahrplan.EditForms
{
    public partial class DaysEditForm : Form
    {
        private Train train;
        private Train train_undo;

        public DaysEditForm()
        {
            InitializeComponent();

            this.Icon = Resources.programm;
        }

        public void Init(Train train)
        {
            this.train = train;
            this.train_undo = train;

            UpdateDays();
        }

        private void UpdateDays()
        {
            /*MondayCheckBox.Checked = train.Monday;
            TuesdayCheckBox.Checked = train.Tuesday;
            WednesdayCheckBox.Checked = train.Wednesday;
            ThursdayCheckBox.Checked = train.Thursday;
            FridayCheckBox.Checked = train.Friday;
            SaturdayCheckBox.Checked = train.Saturday;
            SundayCheckBox.Checked = train.Sunday;*/
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            train = train_undo;           

            this.Close();
        }

        private void dayCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chk = (CheckBox)sender;
            switch(chk.Name)
            {
                case "MondayCheckBox":
                    //train.Monday = MondayCheckBox.Checked;
                    break;
                case "TuesdayCheckBox":
                    //train.Tuesday = TuesdayCheckBox.Checked;
                    break;
                case "WednesdayCheckBox":
                    //train.Wednesday = WednesdayCheckBox.Checked;
                    break;
                case "ThursdayCheckBox":
                    //train.Thursday = ThursdayCheckBox.Checked;
                    break;
                case "FridayCheckBox":
                    //train.Friday = FridayCheckBox.Checked;
                    break;
                case "SaturdayCheckBox":
                    //train.Saturday = SaturdayCheckBox.Checked;
                    break;
                case "SundayCheckBox":
                    //train.Sunday = SundayCheckBox.Checked;
                    break;
            }
        }
    }
}
