using Eto.Forms;
using FPLedit.Shared;
using System;
using FPLedit.Shared.UI;

namespace FPLedit.DebugDump.Forms;

internal sealed class SettingsFormHandler : ISettingsControl
{
    public string DisplayName => "Debug Dump";

    public Control GetControl(IPluginInterface pluginInterface) => new SettingsForm(pluginInterface.Settings);
}

internal sealed class SettingsForm : Panel
{
#pragma warning disable CS0649,CA2213
    private readonly TextBox pathTextBox = default!;
    private readonly CheckBox recordCheckBox = default!;
    private readonly Label privacyLabel = default!;
#pragma warning restore CS0649,CA2213

    public SettingsForm(ISettings settings)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);

        pathTextBox.TextBinding.Bind(() => settings.Get("dump.path", ""), s => settings.Set("dump.path", s));
        recordCheckBox.CheckedBinding.Bind(() => settings.Get("dump.record", false), b => settings.Set("dump.record", b ?? false));

        if (Platform.IsWpf)
            privacyLabel.WordWrap(450);
    }

    private void SelectTargetDir_Click(object sender, EventArgs e)
    {
        using var sfd = new SelectFolderDialog();
        sfd.Directory = pathTextBox.Text;
        if (sfd.ShowDialog(this) != DialogResult.Ok) return;

        pathTextBox.Text = sfd.Directory;
    }

    private void ViewDump_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog();
        ofd.Title = T._("Dump auswählen");
        ofd.AddLegacyFilter("*.fpldmp|*.fpldmp");
        if (ofd.ShowDialog(this) != DialogResult.Ok) return;

        try
        {
            var reader = new DumpReader(ofd.FileName);
            var events = reader.Events;
            using var isf = new InspectForm(events);
            isf.ShowModal();
        }
        catch
        {
            MessageBox.Show(T._("Fehler beim Öffnen der Datei."));
        }
    }
}