using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Linq;

namespace FPLedit;

internal sealed class LogControl : RichTextArea, ILog
{
    private readonly Color systemText;
    private readonly ContextMenu? menu;
    private bool showDebug;

    public LogControl()
    {
        ReadOnly = true;

        menu = new ContextMenu();
        menu.CreateItem(T._("Alles löschen"), clickHandler: (_, _) => Buffer.Clear());
        menu.CreateItem(T._("Meldungen kopieren"), clickHandler: (_, _) => Clipboard.Instance.Text = Text.Trim());
        menu.CreateCheckItem(T._("Debug-Informationen anzeigen"), changeHandler: (s, _) => showDebug = ((CheckMenuItem)s!).Checked);

        systemText = SystemColors.ControlText;
    }

    #region Log
    public bool CanAttach => false;

    public void AttachLogger(ILog other)
    {
    }
        
    public void Error(string message)
        => WriteMl("[FEHLER] " + message, Colors.Red);

    public void Warning(string message)
        => WriteMl("[WARNUNG] " + message, Colors.Orange);

    public void Info(string message)
        => WriteMl("[INFO] " + message, systemText);

    public void LogException(Exception e)
        => WriteMl("[EXCEPTION] " + e.GetExceptionDetails(), Colors.Red);

    public void Debug(string message)
    {
        if (showDebug) WriteMl("[DEBUG] " + message, Colors.Blue);
    }

    private void WriteMl(string message, Color c)
    {
        if (IsDisposed)
            return;
        var lines = message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var l in lines)
            Write(l, c);
    }

    private void Write(string message, Color c)
    {
        var start = Text.Length;
        Buffer.Insert(start, message + Environment.NewLine);
        var range = new Range<int>(start, Text.Length - Environment.NewLine.Length);
        Buffer.SetForeground(range, c);
        Selection = new Range<int>(range.End + 1); // Scroll to end.
    }
    #endregion

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Buttons == MouseButtons.Alternate)
        {
            menu!.Show(this);
            e.Handled = true;
        }
        base.OnMouseDown(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        // We do not want to capture most key events.
        if (e.Control && new[] { Keys.A, Keys.C }.Contains(e.Key))
            return;
        base.OnKeyDown(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (menu != null && !menu.IsDisposed)
        {
            foreach (var topLevelItem in menu.Items)
                topLevelItem.DisposeMenu();
            menu.Dispose();
        }

        base.Dispose(disposing);
    }
}