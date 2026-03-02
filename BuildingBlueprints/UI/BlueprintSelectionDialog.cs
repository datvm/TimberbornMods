namespace BuildingBlueprints.UI;

[BindTransient]
public class BlueprintSelectionDialog(
    BuildingBlueprintsService blueprintService,
    BuildingBlueprintPersistentService persistentService,
    DialogService diag,
    BlueprintWorkshopServiceResolver workshopServiceResolver,
    BlueprintListingSettings listingSettings,
    IContainer container,
    ILoc t,
    VisualElementInitializer veInit,
    PanelStack panelStack
) : DialogBoxElement
{

#nullable disable
    VisualElement blueprintList;
    BlueprintFilterPanel filterPanel;
    Label lblFilter;
#nullable enable

    readonly List<BuildingBlueprintElement> blueprintEls = [];
    ParsedBlueprintInfo? selected;

    public async Task<ParsedBlueprintInfo?> PickAsync()
    {
        SetTitle(t.T("LV.BB.SelectTitle"));
        AddCloseButton();
        SetDialogPercentSize(height: .75f);


        var parent = Content;

        var buttons = parent.AddRow().SetMarginBottom(10);
        
        if (workshopServiceResolver.IsSupported)
        {
            buttons.AddGameButtonPadded(t.T("LV.BB.UploadWorkshop"), onClick: ShowUploadDialog).SetMarginRight(5);

            var url = workshopServiceResolver.Provider.WorkshopBrowsingUrl;
            if (url is not null)
            {
                buttons.AddGameButtonPadded(t.T("LV.BB.BrowseWorkshop"), onClick: () => OpenUrl(url)).SetMarginRight(5);
            }
        }

        buttons.AddChild().SetMarginLeftAuto();
        
        buttons.AddGameButtonPadded(t.T("LV.BB.Browse"), onClick: persistentService.ShowFolder).SetMarginRight(5);
        buttons.AddGameButtonPadded(t.T("LV.BB.Reload"), onClick: RefreshCache);

        filterPanel = container.GetInstance<BlueprintFilterPanel>().SetMarginBottom();
        filterPanel.FilterChanged += ApplyFilter;
        parent.Add(filterPanel);

        lblFilter = parent.AddLabel().SetMarginBottom(10).SetMarginLeftAuto();

        blueprintList = parent.AddChild();
        ReloadContent();

        var confirmed = await ShowAsync(veInit, panelStack);
        return confirmed ? selected : null;
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
        blueprintList.Clear();
        blueprintEls.Clear();

        foreach (var bp in blueprintService.GetParsedBlueprintsWithValidation())
        {
                var el = blueprintList.AddChild(container.GetInstance<BuildingBlueprintElement>);
                el.Init(bp);
                blueprintEls.Add(el);

                el.BlueprintSelected += () => OnBlueprintSelected(bp);
                el.UnlockRequested += () => ProcessUnlockRequest(bp);
                el.EditRequested += () => ShowEditAsync(bp);
        }

        if (blueprintEls.Count == 0)
        {
            blueprintList.AddLabel(t.T("LV.BB.NoBlueprints"));
        }

        filterPanel.ReloadContent();

        ApplyFilter();
    }

    void ApplyFilter()
    {
        var count = 0;

        foreach (var tag in blueprintEls)
        {
            if (tag.Filter(listingSettings))
            {
                count++;
            }
        }

        if (count == blueprintEls.Count)
        {
            lblFilter.SetDisplay(false);
        }
        else
        {
            lblFilter.text = t.T("LV.BB.FilterStat", count, blueprintEls.Count);
            lblFilter.SetDisplay(true);
        }
    }

    void OnBlueprintSelected(BlueprintWithValidation bp)
    {
        selected = bp.Blueprint;
        OnUIConfirmed();
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

    async void ShowEditAsync(BlueprintWithValidation bp)
    {
        var diag = container.GetInstance<BlueprintEditDialog>();
        await diag.ShowEditAsync(bp.Blueprint);

        RefreshCache();
    }

}
