using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI.PlatformControls
{
    class ListItemTextBinding : PropertyBinding<string>
    {
        public ListItemTextBinding()
            : base("Text")
        {
        }

        protected override string InternalGetValue(object dataItem)
        {
            if (dataItem is IListItem item)
                return item.Text;
            if (HasProperty(dataItem))
                return base.InternalGetValue(dataItem);
            return dataItem != null ? System.Convert.ToString(dataItem) : null;
        }
        protected override void InternalSetValue(object dataItem, string value)
        {
            if (dataItem is IListItem item)
                item.Text = System.Convert.ToString(value);
            else
                base.InternalSetValue(dataItem, value);
        }
    }

    class ListItemKeyBinding : PropertyBinding<string>
    {
        public ListItemKeyBinding()
            : base("Key")
        {
        }

        protected override string InternalGetValue(object dataItem)
        {
            if (dataItem is IListItem item)
                return item.Key;
            if (HasProperty(dataItem))
                return base.InternalGetValue(dataItem);
            return dataItem != null ? System.Convert.ToString(dataItem) : null;
        }
    }
}
