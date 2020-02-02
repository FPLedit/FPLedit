using Eto.GtkSharp.Forms.Cells;
using FPLedit.Shared.UI.PlatformControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cairo;
using Gdk;
using Gtk;

namespace FPLedit.Shared.UI.Gtk
{
    public class BDComboBoxCellHandler : SingleCellHandler<CellRendererCombo, BDComboBoxCell, BDComboBoxCell.ICallback>, BDComboBoxCell.IHandler
    {
        readonly ListStore listStore;
        private IEnumerable<object> currentDataStore = null;

        class Renderer : CellRendererCombo, IEtoCellRenderer
        {
            WeakReference handler;
            public BDComboBoxCellHandler Handler { get { return (BDComboBoxCellHandler)handler.Target; } set { handler = new WeakReference(value); } }

#if GTK2
			public bool Editing => (bool)GetProperty("editing").Val;
#endif

            int row;
            [GLib.Property("row")]
            public int Row
            {
                get { return row; }
                set
                {
                    row = value;
                    if (Handler.FormattingEnabled)
                        Handler.Format(new GtkTextCellFormatEventArgs<Renderer>(this, Handler.Column.Widget, Handler.Source.GetItem(Row), Row));
                }
            }

#if GTK2
			public override void GetSize(Gtk.Widget widget, ref Gdk.Rectangle cell_area, out int x_offset, out int y_offset, out int width, out int height)
			{
				base.GetSize(widget, ref cell_area, out x_offset, out y_offset, out width, out height);
				height = Math.Max(height, Handler.Source.RowHeight);
			}
#else
            protected override void OnGetPreferredHeight(Widget widget, out int minimum_size, out int natural_size)
            {
                base.OnGetPreferredHeight(widget, out minimum_size, out _);
                natural_size = Handler.Source.RowHeight;
            }

            protected override void OnRender(Context cr, Widget widget, Gdk.Rectangle background_area, Gdk.Rectangle cell_area, CellRendererState flags)
            {
                if (flags.HasFlag(CellRendererState.Focused))
                {
                    var dataItem = Handler.Source.GetItem(row);
                    var dataStore = Handler.Widget.DataStoreBinding.GetValue(dataItem);
                    if (Handler.currentDataStore == null || !dataStore.Cast<string>().SequenceEqual(Handler.currentDataStore.Cast<string>()))
                    {
                        Handler.listStore.Clear();
                        foreach (var item in dataStore)
                            Handler.listStore.AppendValues(Handler.Widget.ComboTextBinding.GetValue(item), Handler.Widget.ComboKeyBinding.GetValue(item));
                        Handler.currentDataStore = dataStore.ToArray();
                    }
                }

                base.OnRender(cr, widget, background_area, cell_area, flags);
            }
#endif
        }

        public BDComboBoxCellHandler()
        {
            listStore = new ListStore(typeof(string), typeof(string));
            Control = new Renderer
            {
                Handler = this,
                Model = listStore,
                TextColumn = 0,
                HasEntry = false
            };
        }

        protected override void Initialize()
        {
            base.Initialize();
            this.Control.Edited += Connector.HandleEdited;
        }

        protected new ComboBoxCellEventConnector Connector { get { return (ComboBoxCellEventConnector)base.Connector; } }

        protected override WeakConnector CreateConnector()
        {
            return new ComboBoxCellEventConnector();
        }

        protected class ComboBoxCellEventConnector : CellConnector
        {
            public new BDComboBoxCellHandler Handler { get { return (BDComboBoxCellHandler)base.Handler; } }

            public void HandleEdited(object o, EditedArgs args)
            {
                Handler.SetValue(args.Path, args.NewText);
            }

            public void HandleEndEditing(object o, EditedArgs args)
            {
                Handler.Source.EndCellEditing(new TreePath(args.Path), Handler.ColumnIndex);
            }
        }

        protected override void BindCell(ref int dataIndex)
        {
            Column.Control.ClearAttributes(Control);
            SetColumnMap(dataIndex);
            Column.Control.AddAttribute(Control, "text", dataIndex++);
            base.BindCell(ref dataIndex);
        }

        public override void SetEditable(TreeViewColumn column, bool editable)
        {
            Control.Editable = editable;
        }

        public override void SetValue(object dataItem, object value)
        {
            if (Widget.Binding != null)
            {
                Widget.Binding.SetValue(dataItem, value);
            }
        }

        protected override GLib.Value GetValueInternal(object dataItem, int dataColumn, int row)
        {

            if (Widget.Binding != null)
            {
                var ret = Widget.Binding.GetValue(dataItem);
                if (ret != null)
                    return new GLib.Value(ret);
            }

            return new GLib.Value((string)null);
        }

        public override void AttachEvent(string id)
        {
            switch (id)
            {
                case Eto.Forms.Grid.CellEditedEvent:
                    Control.Edited += Connector.HandleEndEditing;
                    break;
                default:
                    base.AttachEvent(id);
                    break;
            }
        }
    }
}
