namespace TImprove4Ui.UI;

[BindSingleton]
public class CustomizableIlluminatorColorPickerAdder(
    CustomizableIlluminatorFragment fragment,
    ILoc t,
    ColorPickerShower colorPickerShower,
#pragma warning disable CS9113 // Just for DI
    IEntityPanel _
#pragma warning restore CS9113 // Parameter is unread.
) : ILoadableSingleton
{

    TextField txtRgb = null!;

    public void Load()
    {
        txtRgb = fragment._rgbTextField;

        var btn = fragment._root.AddGameButtonPadded(t.T("LV.T4UI.ColorPicker"), PickColor, paddingY: 0).SetMargin(left: 10);
        btn.InsertSelfAfter(txtRgb);
    }

    void PickColor()
    {
        if (!ColorUtility.TryParseHtmlString("#" + txtRgb.text, out var color))
        {
            color = fragment._customizableIlluminator.CustomColor;
        }

        colorPickerShower.ShowColorPicker(color, false, c =>
        {
            txtRgb.value = ColorUtility.ToHtmlStringRGB(c);
        });
    }

}
