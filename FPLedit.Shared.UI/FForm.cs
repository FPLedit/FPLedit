using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public static class FFormHandler
    {
        public static event EventHandler Init;

        internal static void InvokeInit(Window w) => Init?.Invoke(w, new EventArgs());
    }

    public class FDialog<T> : Dialog<T>
    {
        public bool WindowShown { get; private set; }
        public FDialog() => FFormHandler.InvokeInit(this);
        protected override void OnShown(EventArgs e)
        {
            WindowShown = true;
            base.OnShown(e);
        }
    }

    public class FDialog : Dialog
    {
        public bool WindowShown { get; private set; }
        public FDialog() => FFormHandler.InvokeInit(this);
        protected override void OnShown(EventArgs e)
        {
            WindowShown = true;
            base.OnShown(e);
        }
    }

    public class FForm : Form
    {
        public bool WindowShown { get; private set; }
        public FForm() => FFormHandler.InvokeInit(this);
        protected override void OnShown(EventArgs e)
        {
            WindowShown = true;
            base.OnShown(e);
        }
    }
}
