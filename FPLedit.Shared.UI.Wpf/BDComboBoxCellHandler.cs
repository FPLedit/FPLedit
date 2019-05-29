using Eto;
using Eto.Wpf;
using Eto.Wpf.Forms.Cells;
using FPLedit.Shared.UI.PlatformDependant;
using System;
using swc = System.Windows.Controls;
using sw = System.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FPLedit.Shared.UI.Wpf
{
    public class BDComboBoxCellHandler : CellHandler<swc.DataGridComboBoxColumn, BDComboBoxCell, BDComboBoxCell.ICallback>, BDComboBoxCell.IHandler
    {
        string GetValue(object dataItem)
        {
            if (Widget.Binding != null)
            {
                var val = Widget.Binding.GetValue(dataItem);
                if (val != null)
                    return Convert.ToString(val);
            }
            return null;
        }

        void SetValue(object dataItem, object value)
        {
            if (Widget.Binding != null)
            {
                Widget.Binding.SetValue(dataItem, Convert.ToString(value));
            }
        }

        class Column : swc.DataGridComboBoxColumn
        {
            public BDComboBoxCellHandler Handler { get; set; }

            protected override sw.FrameworkElement GenerateElement(swc.DataGridCell cell, object dataItem)
            {
                var element = (swc.ComboBox)base.GenerateElement(cell, dataItem);

                Initialize(cell, element, dataItem);
                return Handler.SetupCell(element);
            }

            void Initialize(swc.DataGridCell cell, swc.ComboBox control, object dataItem)
            {
                var collection = new CollectionHandler { Handler = Handler };
                collection.Register(Handler.Widget.DataStoreBinding.GetValue(dataItem));
                control.ItemsSource = collection.Items;

                if (!IsControlInitialized(control))
                {
                    control.DataContextChanged += (sender, e) => SetValue(control.GetParent<swc.DataGridCell>(), (swc.ComboBox)sender, e.NewValue);
                    SetControlInitialized(control, true);
                }
                SetValue(cell, control, dataItem);
            }

            void SetValue(swc.DataGridCell cell, swc.ComboBox control, object dataItem)
            {
                control.SelectedValue = Handler.GetValue(dataItem);
                Handler.FormatCell(control, cell, dataItem);
            }

            protected override sw.FrameworkElement GenerateEditingElement(swc.DataGridCell cell, object dataItem)
            {
                var element = (swc.ComboBox)base.GenerateEditingElement(cell, dataItem);
                Initialize(cell, element, dataItem);
                if (!IsControlEditInitialized(element))
                {
                    element.SelectionChanged += (sender, e) =>
                    {
                        var control = (swc.ComboBox)sender;
                        Handler.SetValue(control.DataContext, control.SelectedValue);
                    };
                    SetControlEditInitialized(element, true);
                }
                return Handler.SetupCell(element);
            }

            protected override bool CommitCellEdit(sw.FrameworkElement editingElement)
            {
                Handler.ContainerHandler.CellEdited(Handler, editingElement);
                return true;
            }
        }

        public BDComboBoxCellHandler()
        {
            Control = new Column
            {
                Handler = this,
                SelectedValuePath = "Key",
                DisplayMemberPath = "Text",
            };
        }

        struct Item
        {
            readonly BDComboBoxCellHandler handler;
            readonly object value;
            public string Text { get { return handler.Widget.ComboTextBinding.GetValue(value); } }
            public string Key { get { return handler.Widget.ComboKeyBinding.GetValue(value); } }
            public Item(BDComboBoxCellHandler handler, object value)
            {
                this.handler = handler;
                this.value = value;
            }
        }

        class CollectionHandler : EnumerableChangedHandler<object>
        {
            public BDComboBoxCellHandler Handler { get; set; }

            public ObservableCollection<Item> Items { get; set; }

            public CollectionHandler()
            {
                Items = new ObservableCollection<Item>();
            }

            public override void AddItem(object item)
            {
                Items.Add(new Item(Handler, item));
            }

            public override void InsertItem(int index, object item)
            {
                Items.Insert(index, new Item(Handler, item));
            }

            public override void RemoveItem(int index)
            {
                Items.RemoveAt(index);
            }

            public override void RemoveAllItems()
            {
                Items.Clear();
            }
        }
    }
}
