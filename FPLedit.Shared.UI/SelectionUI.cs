using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public sealed class SelectionUI<T> : IDisposable where T : Enum
    {
        private readonly Action<T> selectedEvent;
        private readonly DropDown dropDown;
        private readonly Color origBackground;
        private readonly ActionInfo[] actions;

        private ActionInfo selectedState;

        public T SelectedState => selectedState.Value;

        public Color ErrorColor { get; set; } = new Color(Colors.Red, 0.4f);

        public bool EnableErrorColoring { get; set; } = true;

        public bool EnabledOptionSelected => selectedState.Enabled;

        public SelectionUI(Action<T> selectedEvent, StackLayout st)
        {
            this.selectedEvent = selectedEvent;

            actions = GetActions(typeof(T)).ToArray();

            if (actions.Length == 0)
                return;

            if (EnableRadioButtons)
            {
                RadioButton rbf = null;

                for (int i = 0; i < actions.Length; i++)
                {
                    var ac = actions[i];
                    var rb = new RadioButton(rbf)
                    {
                        Text = ac.Name,
                        Checked = rbf == null
                    };
                    if (rbf == null)
                        rbf = rb;

                    var tmp = i;
                    rb.CheckedChanged += (s, e) =>
                    {
                        if (rb.Checked)
                            InternalSelect(ac.Value);
                    };

                    st.Items.Add(rb);
                    ac.RadioButton = rb;
                }
            }
            else
            {
                dropDown = new DropDown
                {
                    DataStore = actions.Cast<object>(),
                    ItemTextBinding = Binding.Property<string, string>(s => s)
                };
                dropDown.SelectedIndexChanged += (s, e) => InternalSelect(((ActionInfo)dropDown.SelectedValue).Value);
                dropDown.SelectedIndex = 0;
                origBackground = dropDown.BackgroundColor;
                st.Items.Add(dropDown);
            }
        }

        private IEnumerable<ActionInfo> GetActions(Type type)
        {
            var values = type.GetEnumValues();

            foreach (T val in values)
            {
                var memInfo = type.GetMember(val.ToString());
                var mem = memInfo.FirstOrDefault(m => m.DeclaringType == type);
                var attributes = mem.GetCustomAttributes(typeof(SelectionNameAttribute), false);
                var name = ((SelectionNameAttribute)attributes.FirstOrDefault())?.Name;
                yield return new ActionInfo(val, name);
            }
        }

        public void DisableOption(T option)
        {
            var state = GetState(option);
            if (EnableRadioButtons)
                state.RadioButton.Enabled = false;
            state.Enabled = false;
        }

        public void ChangeSelection(T option)
        {
            var state = GetState(option);
            if (EnableRadioButtons)
                state.RadioButton.Checked = true;
            else
                dropDown.SelectedValue = state;
        }

        private void InternalSelect(T option)
        {
            selectedState = GetState(option);
            selectedEvent?.Invoke(option);

            if (!EnableRadioButtons && EnableErrorColoring)
                dropDown.BackgroundColor = !selectedState.Enabled ? ErrorColor : origBackground;
        }

        public void Dispose()
        {
            dropDown?.Dispose();
            if (EnableRadioButtons)
                foreach (var rb in actions)
                    rb.RadioButton?.Dispose();
        }

        private bool EnableRadioButtons => Eto.Platform.Instance.IsWpf;

        private ActionInfo GetState(T value) => actions.First(v => v.Value.Equals(value));

        private class ActionInfo
        {
            public string Name;
            public T Value;
            public bool Enabled;
            public RadioButton RadioButton;

            public ActionInfo(T value, string name)
            {
                Name = name;
                Value = value;
                Enabled = true;
                RadioButton = null;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class SelectionNameAttribute : Attribute
    {
        private readonly string name;

        public string Name => name;

        public SelectionNameAttribute(string name)
        {
            this.name = name;
        }
    }
}
