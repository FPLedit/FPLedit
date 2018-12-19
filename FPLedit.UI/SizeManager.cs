using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public class SizeManager
    {
        private ISettings settings;
        private List<SizeEntry> sizes;

        private const string SETTINGS_KEY = "ui.sizes";

        private static bool reset = false;

        public SizeManager(ISettings settings)
        {
            this.settings = settings;
            sizes = ParseSettings().ToList();
        }

        public static void Reset()
        {
            reset = true;
        }

        private IEnumerable<SizeEntry> ParseSettings()
        {
            var sizes = settings.Get(SETTINGS_KEY, "");
            var forms = sizes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var form in forms)
            {
                var parts = form.Split(':');
                yield return new SizeEntry(parts[0], int.Parse(parts[1]), int.Parse(parts[2]));
            }
        }

        private void WriteSettings()
        {
            var set = string.Join(";", sizes.Where(s => s.Resized).Select(s => $"{s.TypeName}:{s.Width}:{s.Height}"));
            if (reset)
                set = "";
            settings.Set(SETTINGS_KEY, set);
        }

        public void Apply(Window w)
        {
            w.Closing += CommitWindowState;
            w.Shown += WindowShown;
        }

        private void WindowShown(object sender, EventArgs e)
        {
            var type = sender.GetType().FullName;
            if (!(sender is Window w))
                return;

            var entry = sizes.FirstOrDefault(s => s.TypeName == type);
            if (entry == null)
            {
                entry = new SizeEntry(type, w.Width, w.Height);
                sizes.Add(entry);
            }
            else
            {
                entry.Resized = true; // Wir haben einen Eintrag, also ist es schon resized.
                w.Width = entry.Width;
                w.Height = entry.Height;
            }
        }

        private void CommitWindowState(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var type = sender.GetType().FullName;
            if (!(sender is Window w))
                return;

            var entry = sizes.FirstOrDefault(s => s.TypeName == type);
            if (entry == null)
                return;

            entry.Resized = entry.Resized || w.Width != entry.Width || w.Height != entry.Height;
            entry.Height = w.Height;
            entry.Width = w.Width;

            WriteSettings();
        }

        private class SizeEntry
        {
            public string TypeName;
            public int Width;
            public int Height;
            public bool Resized;

            public SizeEntry(string type, int w, int h)
            {
                TypeName = type;
                Width = w;
                Height = h;
            }
        }
    }
}
