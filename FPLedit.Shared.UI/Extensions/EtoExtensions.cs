using Eto.Forms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Eto;

namespace FPLedit.Shared.UI;

public static class EtoExtensions
{
    private static SizeManager? sizeManager;
    public static void Initialize(IPluginInterface pluginInterface)
    {
        sizeManager = new SizeManager(pluginInterface.Settings);
    }

    public static Stream GetResource(this Control dialog, string dotFilePath)
    {
        var assembly = Assembly.GetCallingAssembly();
        return assembly.GetManifestResourceStream("FPLedit." + dotFilePath) 
               ?? throw new Exception("Requested resource " + "FPLedit." + dotFilePath + " not found!"); 
    }

    public static void AddSizeStateHandler(this Window w) => sizeManager!.Apply(w);

    public static void AddLegacyFilter(this FileDialog dialog, params string[] filters)
    {
        foreach (var filter in filters)
            dialog.AddLegacyFilter(filter);
    }

    public static void AddLegacyFilter(this FileDialog dialog, string filter)
    {
        var parts = filter.Split('|');
        for (int i = 0; i < parts.Length; i += 2)
        {
            var f = new FileFilter(parts[i], parts[i + 1]);
            dialog.Filters.Add(f);
        }
    }
    
    public static ButtonMenuItem CreateItem(string text, bool enabled = true, EventHandler<EventArgs>? clickHandler = null, Keys shortcut = default)
    {
        var itm = new ButtonMenuItem
        {
            Text = text,
            Enabled = enabled,
            Shortcut = shortcut
        };
        if (clickHandler != null)
            itm.Click += clickHandler;
        return itm;
    }

    public static ButtonMenuItem CreateItem(this ISubmenu parent, string text, bool enabled = true, EventHandler<EventArgs>? clickHandler = null, Keys shortcut = default)
    {
        var itm = new ButtonMenuItem
        {
            Text = text,
            Enabled = enabled,
            Shortcut = shortcut
        };
        if (clickHandler != null)
            itm.Click += clickHandler;
        parent.Items.Add(itm);
        return itm;
    }

    public static MenuItem? GetItem(this ISubmenu parent, string text)
    {
        if (Platform.Instance.IsGtk)
            return parent.Items.FirstOrDefault(i => i.Text == text.Replace("&", ""));
        return parent.Items.FirstOrDefault(i => i.Text == text);
    }
        
    public static void DisposeMenu(this MenuItem mi)
    {
        if (!mi.IsDisposed)
        {
            if (mi is ButtonMenuItem bmi && !bmi.IsDisposed)
                foreach (var mi2 in bmi.Items)
                    DisposeMenu(mi2);
   
            mi.Dispose();
        }
    }

    public static CheckMenuItem CreateCheckItem(this ISubmenu parent, string text, bool isChecked = false, EventHandler<EventArgs>? changeHandler = null)
    {
        var itm = new CheckMenuItem
        {
            Text = text,
            Checked = isChecked,
        };
        if (changeHandler != null)
            itm.CheckedChanged += changeHandler;
        parent.Items.Add(itm);
        return itm;
    }

    public static void WordWrap(this Label label, int maxWidth)
    {
        var origLines = label.Text.Split('\n');
        var lines = new List<string>();
        foreach (var origLine in origLines)
        {
            var words = origLine.Split(' ');
            var line = "";
            for (int i = 0; i < words.Length; i++)
            {
                var nline = line + words[i] + " ";
                if (label.Font.MeasureString(nline.Substring(0, nline.Length - 1)).Width > maxWidth)
                {
                    lines.Add(line);
                    line = words[i] + " ";
                }
                else
                    line = nline;
            }

            lines.Add(line);
        }

        label.Text = string.Join(Environment.NewLine, lines);
    }
}