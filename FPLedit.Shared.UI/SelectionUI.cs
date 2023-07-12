using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace FPLedit.Shared.UI;

public sealed class SelectionUI<T> : IDisposable where T : Enum
{
    private readonly Action<T>? selectedEvent;

    private readonly ActionInfo[] actions;

    private ActionInfo? selectedAction;

    public T? SelectedState => selectedAction != null ? selectedAction.Value : default;
        
    public bool EnabledOptionSelected => selectedAction?.Enabled ?? false;

    public SelectionUI(Action<T>? selectedEvent, StackLayout st)
    {
        var assembly = Assembly.GetCallingAssembly();
        this.selectedEvent = selectedEvent;

        actions = GetActions(typeof(T)).ToArray();

        if (actions.Length == 0)
            return;

        RadioButton? master = null;

        for (int i = 0; i < actions.Length; i++)
        {
            var ac = actions[i];
            var rb = new RadioButton(master)
            {
                Text = FPLedit.Shared.T._a(assembly, ac.Name),
                Checked = i == 0
            };
            master ??= rb;

            rb.CheckedChanged += (_, _) =>
            {
                if (rb.Checked)
                    InternalSelect(ac.Value);
            };

            st.Items.Add(rb);
            ac.RadioButton = rb;
        }

        selectedAction = actions[0];
        InternalSelect(actions[0].Value);
    }

    private IEnumerable<ActionInfo> GetActions(Type type)
    {
        var values = type.GetEnumValues().OfType<T>();

        foreach (var val in values)
        {
            var memInfo = type.GetMember(val.ToString());
            var mem = memInfo.First(m => m.DeclaringType == type);
            var name = mem.GetCustomAttribute<SelectionNameAttribute>(false)?.Name;
            if (name != null)
                yield return new ActionInfo(val, name);
        }
    }

    public void DisableOption(T option)
    {
        var state = GetState(option);
        state.RadioButton.Enabled = false;
        state.Enabled = false;
    }

    public void ChangeSelection(T option)
    {
        var state = GetState(option);
        state.RadioButton.Checked = true;
    }

    private void InternalSelect(T option)
    {
        selectedAction = GetState(option);
        selectedEvent?.Invoke(option);
    }

    public void Dispose()
    {
        foreach (var rb in actions)
            if (rb.RadioButton != null && !rb.RadioButton.IsDisposed)
                rb.RadioButton.Dispose();
    }

    private ActionInfo GetState(T value) => actions.First(v => v.Value.Equals(value));

    private class ActionInfo
    {
        private RadioButton? radioButton;
            
        public readonly string Name;
        public readonly T Value;
        public bool Enabled;

        [NotNull]
        public RadioButton? RadioButton
        {
            get => radioButton ?? throw new Exception("");
            set => radioButton = value;
        }

        public ActionInfo(T value, string name)
        {
            Name = name;
            Value = value;
            Enabled = true;
        }
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public class SelectionNameAttribute : Attribute
{
    public string Name { get; }

    public SelectionNameAttribute(string name)
    {
        Name = name;
    }
}