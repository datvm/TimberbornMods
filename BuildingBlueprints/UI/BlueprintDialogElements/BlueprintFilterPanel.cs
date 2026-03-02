namespace BuildingBlueprints.UI.BlueprintDialogElements;

[BindTransient]
public class BlueprintFilterPanel : CollapsiblePanel
{
    readonly BlueprintListingSettings settings;
    readonly BuildingBlueprintTagService tagService;
    readonly ILoc t;
    readonly VisualElement tagList;

    public event Action FilterChanged = null!;

    public BlueprintFilterPanel(
        BlueprintListingSettings settings,
        BuildingBlueprintTagService tagService,
        ILoc t,
        NamedIconProvider namedIconProvider
    )
    {
        this.settings = settings;
        this.tagService = tagService;
        this.t = t;

        SetTitle(t.T("LV.BB.Filter"));
        SetExpand(settings.ExpandFilter);
        ExpandChanged += OnExpandChanged;
        this.SetBorder();

        var parent = Container;

        var filterRow = parent.AddRow().AlignItems().SetMarginBottom(5);
        filterRow.AddLabel(t.T("LV.BB.Keyword")).SetMarginRight(5);
        filterRow.AddTextField(changeCallback: OnKeywordChanged).SetFlexGrow().text = settings.FilterKeyword;

        var sourceRow = parent.AddRow().SetMarginBottom(10).AlignItems();
        sourceRow.AddToggle(t.T("LV.BB.FilterLocal"), onValueChanged: v => OnSourceChanged(true, v))
            .SetValueWithoutNotify(settings.ShowLocal);
        sourceRow.AddImage(BuildingBlueprintSourceInfo.GetIcon(true, namedIconProvider))
            .SetSize(20).SetMarginRight(10);

        sourceRow.AddToggle(t.T("LV.BB.FilterRemote"), onValueChanged: v => OnSourceChanged(false, v))
            .SetValueWithoutNotify(settings.ShowNonLocal);
        sourceRow.AddImage(BuildingBlueprintSourceInfo.GetIcon(false, namedIconProvider))
            .SetSize(20).SetMarginRight(10);

        sourceRow.AddToggle(t.T("LV.BB.HideInvalids"), onValueChanged: OnHideInvalidsChanged)
            .SetValueWithoutNotify(settings.HideInvalids);

        var tagPanel = parent.AddChild();
        tagPanel.AddLabel(t.T("LV.BB.FilterTagDesc"));
        tagList = tagPanel.AddRow().SetWrap();
    }

    public void ReloadContent()
    {
        tagList.Clear();

        if (tagService.Tags.Length <= 1)
        {
            tagList.AddLabel(t.T("LV.BB.NoTag"));
            return;
        }

        foreach (var t in tagService.TagNamesWithoutUntagged)
        {
            tagList.AddToggle(t, onValueChanged: v => OnHiddenTagChanged(t, v))
                .SetMarginRight(5)
                .SetValueWithoutNotify(settings.FilteredTags.Contains(t));
        }
    }

    void OnHiddenTagChanged(string name, bool hidden)
    {
        if (hidden)
        {
            settings.FilteredTags.Add(name);
        }
        else
        {
            settings.FilteredTags.Remove(name);
        }

        OnFilterChanged();
    }

    void OnHideInvalidsChanged(bool hide)
    {
        settings.HideInvalids = hide;
        OnFilterChanged();
    }

    void OnKeywordChanged(string keyword)
    {
        settings.FilterKeyword = keyword;
        OnFilterChanged();
    }

    void OnSourceChanged(bool local, bool enabled)
    {
        if (local)
        {
            settings.ShowLocal = enabled;
        }
        else
        {
            settings.ShowNonLocal = enabled;
        }
        OnFilterChanged();
    }

    void OnFilterChanged() => FilterChanged();

    void OnExpandChanged(bool e) => settings.ExpandFilter = e;

}
