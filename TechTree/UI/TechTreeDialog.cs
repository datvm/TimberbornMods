namespace TechTree.UI;

[BindSingleton]
public class TechTreeDialog(
    ILoc t,
    VisualElementInitializer veInit,
    PanelStack panelStack,
    IContainer container
) : DialogBoxElement, ILoadableSingleton
{

    public void Load()
    {
        SetTitle(t.T("LV.TT.Title"));
        AddCloseButton();

        SetDialogPercentSize(1f, .9f);

        this.Initialize(veInit);
    }

    public async Task ShowAsync() => await ShowAsync(null, panelStack);

}
