using Buchfahrplan.FileModel;
using Buchfahrplan.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Buchfahrplan
{
    public partial class NewEditForm : Form
    {
        public List<Train> trains = new List<Train>();

        public NewEditForm()
        {
            InitializeComponent();

            this.Icon = Resources.programm;
        }

        public void Init(List<Train> trains)
        {
            this.trains = trains;
            UpdateTrains();
        }

        public void UpdateTrains()
        {
            listView1.Items.Clear();

            foreach (var train in trains)
            {
                listView1.Items.Add(new ListViewItem(new[] { 
                    train.Name, 
                    train.Line,
                    train.Locomotive,
                    train.Negative.ToString() }) 
                    { Tag = train });
            }
        }

        private void NewEditForm_Load(object sender, EventArgs e)
        {
            listView1.Columns.Add("Zugnummer");
            listView1.Columns.Add("Strecke");
            listView1.Columns.Add("Tfz");
            listView1.Columns.Add("Umgekehrt");            
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListViewItem item = null;

            if (listView1.SelectedItems.Count > 0)
            {
                item = (ListViewItem)listView1.Items[listView1.SelectedIndices[0]];
            }

            //TODO: Bug fixen, funktioniert nur bei manuellem deselektieren und neu selektieren!
            if (item != null)
            {
                Train train = (Train)item.Tag;
                negativeCheckBox.Checked = train.Negative;
                lineTextBox.Text = train.Line;
            }
        }

        private void saveTrainButton_Click(object sender, EventArgs e)
        {
            //TODO: Lokomotive
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = (ListViewItem)listView1.Items[listView1.SelectedIndices[0]];

                trains[trains.IndexOf((Train)item.Tag)].Line = lineTextBox.Text;
                trains[trains.IndexOf((Train)item.Tag)].Locomotive = locomotiveTextBox.Text;
                trains[trains.IndexOf((Train)item.Tag)].Negative = negativeCheckBox.Checked;

                UpdateTrains();
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void NewEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }
    }
}
