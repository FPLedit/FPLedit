using Eto.Forms;
using System;

namespace FPLedit.Shared.UI
{
    public static class FFormHandler
    {
        public static event EventHandler? Init;

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
        
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled && e.Control && e.Key == Keys.S)
                DefaultButton?.PerformClick();
                
            base.OnKeyDown(e);
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (!e.Handled && e.Control && e.Key == Keys.S)
                DefaultButton?.PerformClick();
                
            base.OnKeyDown(e);
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
