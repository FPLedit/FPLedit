#nullable enable
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;

namespace FPLedit.SettingsUi
{
    internal sealed class SettingsForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649,CA2213
        private readonly ListBox typeListBox = default!;
        private readonly Panel contentStack = default!;
 #pragma warning restore CS0649,CA2213

        private readonly IPluginInterface pluginInterface;

        public SettingsForm(IPluginInterface pluginInterface)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);
            this.AddSizeStateHandler();

            this.pluginInterface = pluginInterface;

            var settingsSections = pluginInterface.GetRegistered<ISettingsControl>();

            typeListBox.SelectedValueChanged += SelectedSectionChanged;

            typeListBox.ItemTextBinding = Binding.Delegate<ISettingsControl, string>(c => c.DisplayName);
            typeListBox.DataStore = settingsSections;

            if (settingsSections.Length > 0)
                typeListBox.SelectedIndex = 0;
        }

        private void SelectedSectionChanged(object? sender, EventArgs e)
        {
            if (typeListBox.SelectedValue != null)
            {
                var c = (ISettingsControl) typeListBox.SelectedValue;
                contentStack.Content = c.GetControl(pluginInterface);
            }
            else
            {
                contentStack.Content = null;
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Ok);

        private void CancelButton_Click(object sender, EventArgs e)
            => Close(DialogResult.Cancel);

        private static class L
        {
            public static readonly string Close = T._("Schließen");
            public static readonly string Title = T._("Einstellungen");
        }
    }
}
