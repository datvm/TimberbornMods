namespace TechTree.UI;

[BindSingleton]
public class TechTreeDialog(
    ILoc t,
    VisualElementInitializer veInit,
    PanelStack panelStack,
    IContainer container
) : DialogBoxElement, ILoadableSingleton
{
    const float HeightPerc = .9f;
#nullable disable
    VisualElement actualContent;
#nullable enable

    public void Load()
    {
        SetTitle(t.T("LV.TT.Title"));
        AddCloseButton();

        SetDialogPercentSize(1f, HeightPerc);

        Content.RemoveFromHierarchy();

        actualContent = Container.AddChild();

        var row = actualContent.AddRow().AlignItems(Align.Stretch).SetFlexGrow();
        row.AddChild(container.GetInstance<TechTreePanel>).SetFlexGrow();

        var rightPanel = row.AddChild().SetWidth(300)
            .SetFlexShrink(0)
            .AlignItems(Align.Stretch)
            .SetBorder(TimberUiUtils.NeutralColor, 1);
        rightPanel.style.backgroundColor = Color.red;
        rightPanel.AddChild(container.GetInstance<CurrentResearchPanel>).SetFlexShrink(0);
        rightPanel.AddChild(container.GetInstance<TechDescriptionPanel>).SetFlexGrow();

        this.Initialize(veInit);
    }

    public async Task ShowAsync()
    {
        var task = ShowAsync(null, panelStack);

        SetDialogHeight();

        await task;
    }

    void SetDialogHeight()
    {
        var panel = actualContent.panel;
        var scale = panel?.scaledPixelsPerPoint ?? 1;

        var h = Screen.height * HeightPerc / scale;
        actualContent.style.height = h;
    }

}
