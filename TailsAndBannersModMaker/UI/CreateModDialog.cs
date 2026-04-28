namespace TailsAndBannersModMaker.UI;

[BindTransient(Contexts = BindAttributeContext.MainMenu)]
public class CreateModDialog(
    VisualElementInitializer veInit,
    PanelStack panelStack,
    ILoc t,
    IContainer container,
    ModBuilderService modBuilderService,
    DialogService diag,
    IExplorerOpener opener
) : DialogBoxElement
{

    readonly ModMakerInfo modInfo = new();

    void Initialize()
    {
        AddCloseButton();
        SetTitle(t.T("LV.TBMM.OpenDialog"));

        const int height = 800;
        SetDialogSize(height: height);

        var content = Content; //.AddScrollView().SetHeight(height);

        content.Add(container.GetInstance<ModManifestPanel>().SetMod(modInfo));

        content.AddCollapsiblePanel(t.T("LV.TBMM.GroupInfoHeader"))
            .SetMarginBottom(10)
            .Container.AddLabel(t.T("LV.TBMM.GroupInfo"));

        foreach (var type in ModMakerInfo.DecalTypes)
        {
            content.Add(container.GetInstance<DecalTypePanel>().Initialize(type, modInfo.GetDecalInfo(type)));
        }

        content.AddMenuButton(t.T("LV.TBMM.BuildMod"), BuildMod, stretched: true, size: UiBuilder.GameButtonSize.Large);
    }

    async void BuildMod()
    {
        var err = modBuilderService.Validate(modInfo);
        if (err is not null)
        {
            diag.Alert(err);
            return;
        }

        var warnings = string.Join(Environment.NewLine, modBuilderService.ValidateWarnings(modInfo));
        if (warnings.Length > 0 && !await diag.ConfirmAsync(t.T("LV.TBMM.WarningPrompt", warnings)))
        {
            return;
        }

        var folder = await modBuilderService.BuildAsync(modInfo);
        if (!await diag.ConfirmAsync("LV.TBMM.BuildDone", true, localizedOkText: "LV.TBMM.ShowFolder", localizedCancelText: "Core.OK"))
        {
            return;
        }

        opener.OpenDirectory(folder);
    }

    public void Show()
    {
        Initialize();
        Show(veInit, panelStack);
    }

}
