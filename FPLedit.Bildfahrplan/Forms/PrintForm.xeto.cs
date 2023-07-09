using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI.Validators;
using System;
using FPLedit.Shared.UI;
using PdfSharp;

namespace FPLedit.Bildfahrplan.Forms;

internal sealed class PrintForm : FDialog<(int routeIdx, PageSize pageSize, bool landscape, float marginCm)?>
{
#pragma warning disable CS0649,CA2213
    private readonly RoutesDropDown routesDropDown = default!;
    private readonly DropDown paperDropDown = default!;
    private readonly TextBox marginTextBox = default!;
    private readonly CheckBox landscapeChk = default!;
#pragma warning restore CS0649,CA2213
    private readonly NumberValidator marginValidator;

    public PrintForm(IPluginInterface pluginInterface)
    {
        Eto.Serialization.Xaml.XamlReader.Load(this);
            
        routesDropDown.Initialize(pluginInterface);
        routesDropDown.EnableVirtualRoutes = true;
            
        paperDropDown.DataContext = new PageSize?(); // hacky way to use DropDownBind without a global datacontext.
        DropDownBind.Enum<PageSize?, PageSize>(paperDropDown, "Value", null);
        paperDropDown.SelectedValue = PageSize.A4;

        marginTextBox.Text = "1";
        marginValidator = new NumberValidator(marginTextBox, false, false, false);
    }

    private void PrintButton_Click(object sender, EventArgs e)
    {
        if (!marginValidator.Valid)
            return;

        Result = (routesDropDown.SelectedRoute, (PageSize) paperDropDown.SelectedValue, landscapeChk.Checked!.Value, float.Parse(marginTextBox.Text));
        Close();
    }

    private static class L
    {
        public static readonly string SelectRoute = T._("Strecke auswählen");
        public static readonly string PageSettings = T._("Seiteneinstellungen");
        public static readonly string Landscape = T._("Querformat");
        public static readonly string Margin = T._("Seitenrand [cm]:");
        public static readonly string Print = T._("PDF speichern");
        public static readonly string Title = T._("Bildfahrplan als PDF drucken");
    }
}