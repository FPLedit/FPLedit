using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.UI
{
    internal sealed class CloseHandler
    {
        private static readonly List<CloseHandler> activeHandlers = new List<CloseHandler>();

        public Window Dialog { get; }

        public Button Accept { get; }

        public Button Deny { get; }

        private bool isClosing;

        public CloseHandler(Window dialog, Button accept, Button deny)
        {
            Dialog = dialog;
            Accept = accept;
            Deny = deny;

            Dialog.Closing += Dialog_Closing;
            activeHandlers.Add(this);
        }

        private void Dialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            DetachHandler();

            if ((Accept != null && Accept.HasFocus) || (Deny != null && Deny.HasFocus))
                return;

            Deny?.PerformClick();
        }

        public void DetachHandler()
        {
            activeHandlers.Remove(this);
            Dialog.Closing -= Dialog_Closing;
        }

        /// <summary>
        /// NClose removes CloseHandlers.
        /// </summary>
        /// <param name="dialog"></param>
        public static void NClose(Window dialog)
        {
            var ch = activeHandlers.FirstOrDefault(c => c.Dialog == dialog);
            if (ch != null && !ch.isClosing)
            {
                ch.DetachHandler();
                dialog.Close();
            }
        }
    }

    public static class CloseHandlerExtensions
    {
        public static void AddCloseHandler(this Dialog dialog)
            => AddCloseHandler(dialog, dialog.DefaultButton, dialog.AbortButton);

        public static void AddCloseHandler(this Window dialog, Button accept, Button cancel)
            => new CloseHandler(dialog, accept, cancel);

        public static void NClose(this Window dialog) => CloseHandler.NClose(dialog);
    }
}
