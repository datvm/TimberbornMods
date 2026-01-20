namespace BlueprintRelics.UI.UpgradesDialog;

[BindTransient]
public class RelicUpgradesDialog : DialogBoxElement
{

    readonly PanelStack panelStack;

    public RelicUpgradesDialog(ILoc t, PanelStack panelStack, VisualElementInitializer veInit, IContainer container)
    {
        this.panelStack = panelStack;

        SetTitle(t.T("LV.BRe.OpenUpgradeDialogTitle"));
        AddCloseButton();
        SetDialogPercentSize(height: .8f);

        var parent = Content;
        parent.AddChild(container.GetInstance<UnlockedRecipesPanel>).SetMarginBottom();
        parent.AddChild(container.GetInstance<UpgradedRecipesPanel>);

        this.Initialize(veInit);
    }

    public async Task ShowAsync() => await ShowAsync(null, panelStack);

}
