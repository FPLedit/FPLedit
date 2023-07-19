using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;

namespace FPLedit.Editor;

internal sealed class RenderSettingsForm : FDialog<DialogResult>
{
#pragma warning disable CS0649,CA2213
    private readonly TabControl tabControl = default!;
    private readonly CheckBox expertCheckBox = default!;
#pragma warning restore CS0649,CA2213

    private readonly IPluginInterface pluginInterface;
    private readonly List<IAppearanceHandler> handlers;

    public RenderSettingsForm(IPluginInterface pluginInterface)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);
        this.AddSizeStateHandler();
            
        this.pluginInterface = pluginInterface;

        var designables = pluginInterface.GetRegistered<IAppearanceControl>();

        tabControl.SuspendLayout();
        tabControl.Pages.Clear();

        handlers = new List<IAppearanceHandler>();
        foreach (var d in designables)
        {
            var c = d.GetControl(pluginInterface);
            var tp = new TabPage(c)
            {
                Text = d.DisplayName
            };
            c.BackgroundColor = tp.BackgroundColor;
            tabControl.Pages.Add(tp);

            if (c is IAppearanceHandler sh)
                handlers.Add(sh);
        }

        tabControl.ResumeLayout();

        expertCheckBox.Checked = pluginInterface.Settings.Get<bool>("std.expert");
        expertCheckBox.CheckedChanged += ExpertCheckBox_CheckedChanged;
        ExpertCheckBox_CheckedChanged(this, EventArgs.Empty);
    }

    private void CloseButton_Click(object sender, EventArgs e)
    {
        pluginInterface.Settings.Set("std.expert", expertCheckBox.Checked!.Value);
        handlers.ForEach(sh => sh.Save());
        Close(DialogResult.Ok);
    }

    private void CancelButton_Click(object sender, EventArgs e)
        => Close(DialogResult.Cancel);

    private void ExpertCheckBox_CheckedChanged(object? sender, EventArgs e)
        => handlers.ForEach(eh => eh.SetExpertMode(expertCheckBox.Checked!.Value));
        
    private static class L
    {
        public static readonly string Cancel = T._("Abbrechen");
        public static readonly string Close = T._("Schließen");
        public static readonly string Title = T._("Fahrplandarstellung");
        public static readonly string Expert = T._("Expertenmodus (CSS-Bearbeitung) aktivieren");
    }
}