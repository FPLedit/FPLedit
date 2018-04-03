using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor.Network
{
    public class ToggleButton
    {
        private Button btn;
        private Color origColor;

        private bool _checked;
        public bool Checked
        {
            get => _checked;
            set
            {
                if (value != _checked)
                {
                    _checked = value;
                    OnCheckedChanged();
                }
            }
        }

        public event EventHandler CheckedChanged;
        public event EventHandler ToggleClick;

        public ToggleButton(Button btn)
        {
            this.btn = btn;
            origColor = btn.BackgroundColor;
            btn.Click += Click;
        }

        protected void OnCheckedChanged()
        {
            btn.BackgroundColor = Checked ? Colors.LightBlue : origColor;
            CheckedChanged?.Invoke(this, new EventArgs());
        }

        protected void Click(object sender, EventArgs e)
        {
            Checked = !Checked;
            ToggleClick?.Invoke(sender, e);
        }
    }
}
