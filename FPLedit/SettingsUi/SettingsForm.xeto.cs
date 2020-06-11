using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;

namespace FPLedit.SettingsUi
{
    internal sealed class SettingsForm : FDialog<DialogResult>
    {
#pragma warning disable CS0649
        private readonly ListBox typeListBox;
        private readonly Panel contentStack;
 #pragma warning restore CS0649

        private readonly IPluginInterface pluginInterface;
        private readonly List<IAppearanceHandler> handlers;

        private SettingsForm()
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            handlers = new List<IAppearanceHandler>();

            this.AddSizeStateHandler();
        }

        public SettingsForm(IPluginInterface pluginInterface) : this()
        {
            this.pluginInterface = pluginInterface;

            var settingsSections = pluginInterface.GetRegistered<ISettingsControl>();
            
            typeListBox.SelectedValueChanged += SelectedSectionChanged;

            typeListBox.ItemTextBinding = Binding.Property<ISettingsControl, string>(c => c.DisplayName);
            typeListBox.DataStore = settingsSections;

            if (settingsSections.Length > 0)
                typeListBox.SelectedIndex = 0;
        }

        private void SelectedSectionChanged(object sender, EventArgs e)
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
    }
}
