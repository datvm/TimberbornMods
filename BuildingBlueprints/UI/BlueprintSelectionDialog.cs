namespace BuildingBlueprints.UI;

[BindTransient]
public class BlueprintSelectionDialog(
    BuildingBlueprintsService blueprintService,
    BuildingBlueprintPersistentService persistentService,
    IGoodService goods,
    NamedIconProvider namedIconProvider,
    DialogService diag,
    BlueprintWorkshopServiceResolver workshopServiceResolver,
    IContainer container,

    ILoc t,
    VisualElementInitializer veInit,
    PanelStack panelStack
) : DialogBoxElement
{

#nullable disable
    VisualElement lstBlueprints;
    Button btnBuild;
#nullable enable

    ParsedBlueprintInfo? SelectingBlueprint => bpEls.FirstOrDefault(el => el.Selected)?.Blueprint;
    readonly List<BuildingBlueprintElement> bpEls = [];

    public async Task<ParsedBlueprintInfo?> PickAsync()
    {
        SetTitle(t.T("LV.BB.SelectTitle"));
        AddCloseButton();
        SetDialogPercentSize(height: .75f);

        var parent = Content;

        var buttons = parent.AddRow().SetMarginBottom(10);
        buttons.AddChild().SetMarginLeftAuto();

        if (workshopServiceResolver.IsSupported)
        {
            buttons.AddGameButtonPadded(t.T("LV.BB.UploadWorkshop"), onClick: ShowUploadDialog).SetMarginRight(5);

            var url = workshopServiceResolver.Provider.WorkshopBrowsingUrl;
            if (url is not null)
            {
                buttons.AddGameButtonPadded(t.T("LV.BB.BrowseWorkshop"), onClick: () => OpenUrl(url)).SetMarginRight(5);
            }
        }
        buttons.AddGameButtonPadded(t.T("LV.BB.Browse"), onClick: persistentService.ShowFolder).SetMarginRight(5);
        buttons.AddGameButtonPadded(t.T("LV.BB.Reload"), onClick: RefreshCache);

        parent.AddLabel(t.T("LV.BB.SelectDesc")).SetMarginBottom(10);

        btnBuild = parent.AddMenuButton(t.T("LV.BB.Build"), onClick: OnUIConfirmed, stretched: true);
        btnBuild.enabledSelf = false;

        lstBlueprints = parent.AddChild();
        ReloadContent();

        var confirmed = await ShowAsync(veInit, panelStack);
        return confirmed ? SelectingBlueprint : null;
    }

    void OpenUrl(string url) => Application.OpenURL(url);

    void ShowUploadDialog()
    {
        container.GetInstance<BlueprintWorkshopSelectionDialog>().Show();
    }

    void RefreshCache()
    {
        blueprintService.GetParsedBlueprints(forceReload: true);
        ReloadContent();
    }

    void ReloadContent()
    {
        lstBlueprints.Clear();
        bpEls.Clear();

        foreach (var bp in blueprintService.GetParsedBlueprintsWithValidation())
        {
            BuildingBlueprintElement el = new(bp, t, goods, namedIconProvider);
            bpEls.Add(el);
            el.BlueprintSelected += () => OnBlueprintSelected(el);
            el.UnlockRequested += () => ProcessUnlockRequest(bp);

            lstBlueprints.Add(el.SetMarginBottom());
        }

        if (bpEls.Count == 0)
        {
            lstBlueprints.AddLabel(t.T("LV.BB.NoBlueprints"));
        }
        else
        {
            var el = bpEls.FirstOrDefault(e => !e.Invalid);
            if (el is not null)
            {
                el.Selected = true;
                OnBlueprintSelected(el);
            }
        }
    }

    void OnBlueprintSelected(BuildingBlueprintElement el)
    {
        foreach (var other in bpEls)
        {
            if (other != el && other.Selected)
            {
                other.Selected = false;
            }
        }

        btnBuild.enabledSelf = true;
    }

    async void ProcessUnlockRequest(BlueprintWithValidation bp)
    {
        var cost = bp.ScienceCost;
        if (cost > 0)
        {
            if (blueprintService.HasEnoughScience(cost))
            {
                if (!await diag.ConfirmAsync(t.T("LV.BB.UnlockConfirm", cost)))
                {
                    return;
                }
            }
            else
            {
                diag.Alert("LV.BB.NotEnoughScience", true);
                return;
            }
        }

        blueprintService.UnlockToolsForBlueprint(bp);
        ReloadContent();
    }

}
