using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Buchfahrplan
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

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
