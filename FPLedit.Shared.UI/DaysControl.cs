using System;
using System.Linq;
using Eto.Forms;

namespace FPLedit.Shared.UI
{
    public abstract class DaysControl : Panel
    {
        private CheckBox[]? daysBoxes;
        private ToggleButton[]? shortcutsToggle;
        
        private bool applyingShortcut;
        
        private readonly Days wShortcut = Days.Parse("1111110");
        private readonly Days wExclSaShortcut = Days.Parse("1111100");
        private readonly Days sShortcut = Days.Parse("0000001");
        private readonly Days aShortcut = Days.Parse("1111111");
        private readonly Days zShortcut = Days.Parse("0000000");

        public event EventHandler? SelectedDaysChanged;

        protected void InitializeWithControls()
        {
            daysBoxes = new[] { 
                FindChild<CheckBox>("mondayCheckBox"), 
                FindChild<CheckBox>("tuesdayCheckBox"), 
                FindChild<CheckBox>("wednesdayCheckBox"),
                FindChild<CheckBox>("thursdayCheckBox"), 
                FindChild<CheckBox>("fridayCheckBox"), 
                FindChild<CheckBox>("saturdayCheckBox"), 
                FindChild<CheckBox>("sundayCheckBox") 
            };
            foreach (var dayBox in daysBoxes)
                dayBox.CheckedChanged += CheckBoxStateChanged;
            
            var shortcuts = new[] { wShortcut, wExclSaShortcut, sShortcut, aShortcut, zShortcut };
            shortcutsToggle = new[] { 
                FindChild<ToggleButton>("wShort"), 
                FindChild<ToggleButton>("wSaShort"), 
                FindChild<ToggleButton>("sShort"), 
                FindChild<ToggleButton>("aShort"), 
                FindChild<ToggleButton>("zShort") 
            };
            for (int i = 0; i < shortcutsToggle.Length; i++)
            {
                shortcutsToggle[i].Tag = shortcuts[i];
                shortcutsToggle[i].Click += ApplyShortcutBtn;
            }
        }

        public Days SelectedDays
        {
            get => new Days(daysBoxes.Select(b => b.Checked!.Value).ToArray());
            set => SetDays(value);
        } 
        
        private void SetDays(Days days)
        {
            applyingShortcut = true;
            for (int i = 0; i < days.Length; i++)
                daysBoxes![i].Checked = days[i];
            applyingShortcut = false;
            
            UpdateShortcutState();
        }

        public void HandleKeypress(KeyEventArgs e)
        {
            if (!e.Control)
                return;

            var handled = true;
            if (new[] { Keys.D0, Keys.Keypad0 }.Contains(e.Key))
                SetDays(zShortcut);
            else if (e.Key == Keys.A)
                SetDays(aShortcut);
            else if (e.Key == Keys.W && e.Shift)
                SetDays(wExclSaShortcut);
            else if (e.Key == Keys.W)
                SetDays(wShortcut);
            else if (e.Key == Keys.S)
                SetDays(sShortcut);
            else
                handled = false;

            e.Handled = handled;
        }

        private void ApplyShortcutBtn(object? sender, EventArgs e)
        {
            var btn = (ToggleButton)sender!;
            foreach (var toggle in shortcutsToggle!)
                toggle.Checked = false;
            if (btn.Tag is Days days)
                SetDays(days);
            UpdateShortcutState();
        }

        private void CheckBoxStateChanged(object? sender, EventArgs e)
        {
            if (applyingShortcut) // Don't update if we are setting days.
                return;
            UpdateShortcutState();
        }

        private void UpdateShortcutState()
        {
            var cur = SelectedDays;
            foreach (var item in shortcutsToggle!)
                item.Checked = cur.Equals((Days)item.Tag);
            
            SelectedDaysChanged?.Invoke(this, new EventArgs());
        }
    }

    public sealed class DaysControlNarrow : DaysControl
    {
        public DaysControlNarrow()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            InitializeWithControls();
        }
    }
    
    public sealed class DaysControlWide : DaysControl
    {
        public DaysControlWide()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            InitializeWithControls();
        }
    }
}