using Eto;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI.PlatformControls
{
    /// <summary>
	/// Cell to present a combo box with data store binding in a <see cref="Grid"/>.
	/// </summary>
	[Handler(typeof(BDComboBoxCell.IHandler))]
    public class BDComboBoxCell : SingleValueCell<object>
    {
        /// <summary>
        /// Binding to get the text value for the items in the combo box.
        /// </summary>
        /// <value>The combo text binding.</value>
        public IIndirectBinding<string> ComboTextBinding { get; set; }

        /// <summary>
        /// Binding to get the key value for the items in the combo box.
        /// </summary>
        /// <value>The combo key binding.</value>
        public IIndirectBinding<string> ComboKeyBinding { get; set; }

        public IIndirectBinding<IEnumerable<object>> DataStoreBinding { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDComboBoxCell"/> class with the column index to bind.
        /// </summary>
        /// <param name="column">Column index to bind to.</param>
        public BDComboBoxCell(int column)
            : this()
        {
            Binding = new ColumnBinding<object>(column);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDComboBoxCell"/> class with the specified property to bind to.
        /// </summary>
        /// <param name="property">Property to bind the value of the combo box to.</param>
        public BDComboBoxCell(string property)
            : this()
        {
            Binding = Eto.Forms.Binding.Property<object>(property);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BDComboBoxCell"/> class.
        /// </summary>
        public BDComboBoxCell()
        {
            ComboTextBinding = new ListItemTextBinding();
            ComboKeyBinding = new ListItemKeyBinding();
        }

        /// <summary>
        /// Handler interface for the <see cref="BDComboBoxCell"/>.
        /// </summary>
        public new interface IHandler : SingleValueCell<object>.IHandler
        {
        }
    }
}
