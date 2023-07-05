using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.UI
{
    internal sealed class CloseHandler
    {
        private static readonly List<CloseHandler> activeHandlers = new ();

        public Window Dialog { get; }

        public Button? Accept { get; }

        public Button? Deny { get; }

        private bool isClosing;

        public CloseHandler(Window dialog, Button? accept, Button? deny)
        {
            Dialog = dialog;
            Accept = accept;
            Deny = deny;

            Dialog.Closing += Dialog_Closing;
            activeHandlers.Add(this);
        }

        private void Dialog_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isClosing)
                return;
            isClosing = true;
            DetachHandler();

            if (Accept is { HasFocus: true } || Deny is { HasFocus: true })
                return;

            Deny?.PerformClick();
        }

        private void DetachHandler()
        {
            activeHandlers.Remove(this);
            Dialog.Closing -= Dialog_Closing;
        }

        /// <summary>
        /// NClose removes CloseHandlers and then closes the window, i.e. it closes the window without triggering the
        /// Deny button action.
        /// </summary>
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
        /// <summary>
        /// Add a <see cref="CloseHandler"/> to the durrent window. This handler will call the deny button action if
        /// the window is closed manually (with the close button proivided by the OS).
        /// </summary>
        public static void AddCloseHandler(this Dialog dialog)
            => new CloseHandler(dialog, dialog.DefaultButton, dialog.AbortButton);

        /// <summary>
        /// NClose closes the curretn window and detaches any <see cref="CloseHandler"/> beforehand.
        /// </summary>
        public static void NClose(this Window dialog) => CloseHandler.NClose(dialog);
    }
}
