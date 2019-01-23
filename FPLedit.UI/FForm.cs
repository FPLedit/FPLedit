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
        public static Action<Window> Init { get; internal set; }
    }

    public class FDialog<T> : Dialog<T>
    {
        public FDialog() => FFormHandler.Init?.Invoke(this);
    }

    public class FDialog : Dialog
    {
        public FDialog() => FFormHandler.Init?.Invoke(this);
    }

    public class FForm : Form
    {
        public FForm() => FFormHandler.Init?.Invoke(this);
    }
}
