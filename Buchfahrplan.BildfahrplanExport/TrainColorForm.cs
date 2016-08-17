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

namespace Buchfahrplan.BildfahrplanExport
{
    public partial class TrainColorForm : Form
    {
        private Timetable tt;

        public TrainColorForm()
        {
            InitializeComponent();
        }

        public void Init(Timetable tt)
        {
            this.tt = tt;
        }
    }
}
