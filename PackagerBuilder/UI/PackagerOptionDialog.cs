namespace PackagerBuilder.UI;

public class PackagerOptionDialog : DialogBoxElement
{
    readonly PanelStack panelStack;
    readonly Toggle chkPack10, chkCrate, chkBarrel;

    public PackagerOptionDialog(
        VisualElementInitializer veInit,
        PanelStack panelStack,
        ILoc t
    )
    {
        SetTitle(t.T("LV.Pkg.BuildPackager"));
        AddCloseButton();

        this.panelStack = panelStack;

        var pnlOptions = Content.AddChild().SetMarginBottom();

        chkPack10 = pnlOptions.AddToggle(t.T("LV.Pkg.Option10"));
        pnlOptions.AddLabel(t.T("LV.Pkg.Option10Desc")).SetMarginBottom(5);

        chkCrate = pnlOptions.AddToggle(t.T("LV.Pkg.OptionCrate"));
        pnlOptions.AddLabel(t.T("LV.Pkg.OptionCrateDesc")).SetMarginBottom(5);

        chkBarrel = pnlOptions.AddToggle(t.T("LV.Pkg.OptionBarrel"));
        pnlOptions.AddLabel(t.T("LV.Pkg.OptionBarrelDesc")).SetMarginBottom(5);

        Content.AddMenuButton(t.T("LV.Pkg.BuildPackager"), onClick: OnUIConfirmed, stretched: true);

        chkPack10.SetValueWithoutNotify(true);

        this.Initialize(veInit);
    }

    public async Task<PackagerBuildOptions?> ShowAsync()
    {
        var confirm = await ShowAsync(null, panelStack);
        if (!confirm) { return null; }

        var result = new PackagerBuildOptions(
            Pack10: chkPack10.value,
            Crate: chkCrate.value,
            Barrel: chkBarrel.value
        );
        return result == default ? null : result;
    }

}
public readonly record struct PackagerBuildOptions(bool Pack10, bool Crate, bool Barrel);