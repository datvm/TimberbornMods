namespace WoodYouLookAtTheLog.Services;

[BindTransient]
public class TimePickerDialog(PanelStack panelStack, VisualElementInitializer veInit, ILoc t) : DialogBoxElement
{

    IntegerField txtMinutes = null!;

    void Initialize()
    {
        var parent = Content;

        SetTitle(t.T("LV.WYLATL.Title"));
        AddCloseButton();

        parent.AddLabel(t.T("LV.WYLATL.PickMinutes"));
        txtMinutes = parent.AddIntField().SetMarginBottom();
        txtMinutes.SetValueWithoutNotify(90);

        parent.AddMenuButton(t.TOK(), OnUIConfirmed, size: UiBuilder.GameButtonSize.Large, stretched: true).SetMarginBottom();
        parent.AddGameButtonPadded(t.T("LV.WYLATL.ImGood"), OnUICancelled).SetMarginLeftAuto();
    }

    public async Task<int?> ShowAsync()
    {
        Initialize();

        if (!await ShowAsync(veInit, panelStack)) { return null; }
        return txtMinutes.value;
    }

}
