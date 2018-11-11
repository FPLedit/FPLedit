using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public class SelectionUI
    {
        private Action<int> selectedEvent;
        private DropDown dropDown;
        private RadioButton[] radioButtons;
        private int optionsCount;

        public int SelectedState { get; private set; }

        public SelectionUI(Action<int> selectedEvent, StackLayout st, params string[] actions)
        {
            this.selectedEvent = selectedEvent;
            optionsCount = actions.Length;

            if (actions.Length == 0)
                return;

            if (EnableRadioButtons)
            {
                radioButtons = new RadioButton[actions.Length];
                RadioButton rbf = null;

                for (int i = 0; i < actions.Length; i++)
                {
                    var ac = actions[i];
                    var rb = new RadioButton(rbf);
                    rb.Text = ac;

                    rb.Checked = (rbf == null);
                    if (rbf == null)
                        rbf = rb;

                    var tmp = i;
                    rb.CheckedChanged += (s, e) =>
                    {
                        if (rb.Checked)
                            InternalSelect(tmp);
                    };

                    st.Items.Add(rb);
                    radioButtons[i] = rb;
                }
            }
            else
            {
                dropDown = new DropDown();
                dropDown.DataStore = actions;
                dropDown.ItemTextBinding = Binding.Property<RadioButton, string>(rb => rb.Text);
                dropDown.SelectedIndexChanged += (s, e) => InternalSelect(dropDown.SelectedIndex);
                dropDown.SelectedIndex = 0;
                st.Items.Add(dropDown);
            }
        }

        public void ChangeSelection(int idx)
        {
            if (idx >= optionsCount)
                throw new IndexOutOfRangeException("Der Index lag außerhalb des erlaubten Bereich dieser SelectionUI!");

            if (EnableRadioButtons)
                radioButtons[idx].Checked = true;
            else
                dropDown.SelectedIndex = idx;
        }

        private void InternalSelect(int idx)
        {
            SelectedState = idx;
            selectedEvent?.Invoke(idx);
        }

        private bool EnableRadioButtons => Eto.Platform.Instance.IsWpf;
    }
}
