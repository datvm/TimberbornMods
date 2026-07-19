namespace TechTree.UI;

[BindSingleton]
public class TechTreeDialog(
    ILoc t,
    VisualElementInitializer veInit,
    PanelStack panelStack
) : DialogBoxElement, ILoadableSingleton
{

    public void Load()
    {
        SetTitle(t.T("LV.TT.Title"));
        AddCloseButton();

        SetDialogPercentSize(90, 90);

        this.Initialize(veInit);
    }

    public async Task ShowAsync() => await ShowAsync(null, panelStack);

}
