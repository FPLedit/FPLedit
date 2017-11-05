using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.NewEditor
{
    public partial class NewLineStationForm : Form
    {
        Timetable _parent;

        public Station Station { get; private set; }

        public float Position { get; private set; }

        private NewLineStationForm()
        {
            InitializeComponent();
        }

        public NewLineStationForm(Timetable tt) : this()
        {
            _parent = tt;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            string name = nameTextBox.Text;

            if (!positionValidator.Valid || !nameValidator.Valid)
            {
                MessageBox.Show("Bitte erst alle Fehler beheben!");
                return;
            }

            Station = new Station(_parent);
            Station.SName = name;
            Position = float.Parse(positionTextBox.Text);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
