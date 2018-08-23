using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    internal class CloseHandler
    {
        internal static List<CloseHandler> ActiveHandlers = new List<CloseHandler>();

        public Window Dialog { get; private set; }

        public Button Accept { get; private set; }

        public Button Deny { get; private set; }

        private bool isClosing;

        public CloseHandler(Window dialog, Button accept, Button deny)
        {
            Dialog = dialog;
            Accept = accept;
            Deny = deny;

            Dialog.Closing += Dialog_Closing;
            ActiveHandlers.Add(this);
        }

        private void Dialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            isClosing = true;
            Remove();

            if ((Accept != null && Accept.HasFocus) || (Deny != null && Deny.HasFocus))
                return;

            Deny?.PerformClick();
        }

        public void Remove() => ActiveHandlers.Remove(this);

        public static void NClose(Window dialog)
        {
            var ch = ActiveHandlers.FirstOrDefault(c => c.Dialog == dialog);
            if (ch == null || !ch.isClosing)
                dialog.Close();
        }
    }
}
