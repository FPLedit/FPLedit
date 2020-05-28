using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;

namespace FPLedit.Shared.UI
{
    public sealed class SizeManager
    {
        private readonly ISettings settings;
        private readonly List<SizeEntry> sizes;

        private const string SETTINGS_KEY = "ui.sizes";

        private static bool reset = false;

        public SizeManager(ISettings settings)
        {
            this.settings = settings;
            try
            {
                sizes = ParseSettings().ToList();
            }
            catch
            {
                sizes = new List<SizeEntry>();
            }
        }

        public static void Reset()
        {
            reset = true;
        }

        private IEnumerable<SizeEntry> ParseSettings()
        {
            var loadedSizes = settings.Get(SETTINGS_KEY, "");
            var forms = loadedSizes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var form in forms)
            {
                var parts = form.Split(':');
                if (parts.Length == 3)
                    yield return new SizeEntry(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), false);
                else if (parts.Length == 4)
                    yield return new SizeEntry(parts[0], int.Parse(parts[1]), int.Parse(parts[2]), bool.Parse(parts[3]));
            }
        }

        private void WriteSettings()
        {
            var set = string.Join(";", sizes.Where(s => s.Resized).Select(s => $"{s.TypeName}:{s.Width}:{s.Height}:{s.Maximized}"));
            if (reset)
                set = "";
            settings.Set(SETTINGS_KEY, set);
        }

        public void Apply(Window w)
        {
            w.Closing += CommitWindowState;
            w.Shown += WindowShown;
        }

        private void WindowShown(object? sender, EventArgs e)
        {
            if (!(sender is Window w))
                return;
            var type = sender.GetType().FullName;

            var entry = sizes.FirstOrDefault(s => s.TypeName == type);
            if (entry == null)
            {
                entry = new SizeEntry(type!, w.ClientSize.Width, w.ClientSize.Height, false);
                sizes.Add(entry);
            }
            else
            {
                if (entry.Resized)
                {
                    if (!entry.Maximized || !w.Maximizable)
                        w.ClientSize = new Size(entry.Width, entry.Height);
                    else
                        w.Maximize();
                }
            }
        }

        private void CommitWindowState(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!(sender is Window w))
                return;
            var type = sender.GetType().FullName;

            var entry = sizes.FirstOrDefault(s => s.TypeName == type);
            if (entry == null)
                return;

            entry.Resized = entry.Resized || w.ClientSize.Width != entry.Width || w.ClientSize.Height != entry.Height;
            entry.Height = w.ClientSize.Height;
            entry.Width = w.ClientSize.Width;
            entry.Maximized = w.WindowState == WindowState.Maximized;

            WriteSettings();
        }

        private class SizeEntry
        {
            public readonly string TypeName;
            public int Width;
            public int Height;
            public bool Resized;
            public bool Maximized;

            public SizeEntry(string type, int w, int h, bool max)
            {
                TypeName = type;
                Width = w;
                Height = h;
                Maximized = max;
            }
        }
    }
}
