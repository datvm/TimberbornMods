namespace PowerCopy.UI;

[BindFragment]
public class PowerCopyFragment(
    ILoc t,
    ObjectListingService objectListingService,
    DialogService diag,
    InputService inputService,
    IContainer container,
    BuildingSettingsResolver buildingSettingsResolver,
    PowerCopyAreaTool areaTool,
    PowerCopyService powerCopyService
) : IEntityPanelFragment, IEntityFragmentOrder
{
    public int Order { get; } = -1;

#nullable disable
    public VisualElement Fragment => panel;

    EntityPanelFragmentElement panel;
    Toggle chkTargetAll, chkTargetSameType;

    Button btnLocEverywhere, btnLocDistrict;
#nullable enable

    Toggle[] allTargets = [];

    EntityComponent? curr;
    DistrictBuilding? currDb;

    string currName = "";
    string currTemplate = "";
    BuildingSettingsPair[] currPairs = [];
    readonly Dictionary<Type, BuildingSettingEntry> entriesByType = [];

    public bool TargetingAll => chkTargetAll.value;
    public IEnumerable<IBuildingSettings> SelectedSettings => entriesByType.Values
        .Where(e => e.Selected && e.Visible)
        .Select(e => e.BuildingSetting);

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Visible = false,
            Background = EntityPanelFragmentBackground.PalePurple,
        };

        var options = panel.AddChild().SetMarginBottom();
        options.AddLabel(t.T("LV.PC.CopyOptions"));
        var entryList = options.AddChild();
        InitializeEntries(entryList);

        var targets = panel.AddChild().SetMarginBottom();
        targets.AddLabel(t.T("LV.PC.CopyTargets"));
        chkTargetAll = targets.AddToggle(t.T("LV.PC.CopyTargets_All"), onValueChanged: _ => OnTargetSelected(true));
        chkTargetSameType = targets.AddToggle("", onValueChanged: _ => OnTargetSelected(false));
        allTargets = [chkTargetAll, chkTargetSameType];
        OnTargetSelected(false);

        var locations = panel.AddChild();
        btnLocEverywhere = AddLocationButton(CopyLocation.Everywhere);
        btnLocDistrict = AddLocationButton(CopyLocation.District);
        var tmp = AddLocationButton(CopyLocation.Area);
        tmp.text = t.T("LV.PC.CopyLocations_Area");
        tmp = AddLocationButton(CopyLocation.List);
        tmp.text = t.T("LV.PC.CopyLocations_List");

        return panel;

        NineSliceButton AddLocationButton(CopyLocation location)
            => locations.AddGameButtonPadded(onClick: () => OnLocationButtonClick(location), stretched: true).SetMarginBottom(5);

        void InitializeEntries(VisualElement parent)
        {
            foreach (var s in buildingSettingsResolver.AllBuildingSettings)
            {
                var entry = new BuildingSettingEntry(s);
                entry.OnCheckedChanged += UpdateStatistics;
                entriesByType[s.Type] = entry;
                parent.Add(entry);
            }
        }
    }

    public void ClearFragment()
    {
        panel.Visible = false;
        curr = null;
        currDb = null;
        currPairs = [];

        foreach (var e in entriesByType.Values)
        {
            e.Visible = false;
        }
    }

    public void ShowFragment(BaseComponent entity)
    {
        curr = entity.GetComponent<EntityComponent>();
        var template = entity.GetComponent<TemplateSpec>();
        if (!curr || template is null)
        {
            ClearFragment();
            return;
        }
        currTemplate = template.TemplateName;

        currPairs = buildingSettingsResolver.Get(entity);
        if (currPairs.Length == 0 || !ShowValidSettings())
        {
            ClearFragment();
            return;
        }

        currName = entity.GetLabeledName(t);
        chkTargetSameType.text = t.T("LV.PC.CopyTargets_SameType", currName);

        currDb = entity.GetComponent<DistrictBuilding>();
        if (!currDb || !currDb.District) { currDb = null; }

        UpdateFragment(); // To adjust visibility so statistics will be correct
        UpdateStatistics();

        panel.Visible = true;
    }

    bool ShowValidSettings()
    {
        var hasValid = false;

        foreach (var (d, s) in currPairs)
        {
            if (!d.IsDuplicable) { continue; }

            var entry = entriesByType[s.Type];
            entry.Set(d);
            entry.Visible = true;

            hasValid = true;
        }

        return hasValid;
    }

    public void UpdateFragment()
    {
        if (curr is null || currPairs.Length == 0) { return; }

        foreach (var (d, s) in currPairs)
        {
            var entry = entriesByType[s.Type];
            var visible = entry.Visible = d.IsDuplicable;

            if (visible)
            {
                entry.Set(d);
            }
        }
    }

    void OnTargetSelected(bool all)
    {
        var checkingTarget = all ? chkTargetAll : chkTargetSameType;

        foreach (var target in allTargets)
        {
            target.SetValueWithoutNotify(target == checkingTarget);
        }

        UpdateStatistics();
    }

    async void OnLocationButtonClick(CopyLocation location)
    {
        var entities = location switch
        {
            CopyLocation.Everywhere or CopyLocation.District => [.. objectListingService.QueryObjects(GenerateQuery(location))],
            CopyLocation.Area => await areaTool.Pick(GenerateQuery(CopyLocation.Everywhere)),
            CopyLocation.List => await GetEntitiesFromDialogAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(location), "Unknown location"),
        };

        if (entities.Length == 0 || !await ConfirmAsync(entities.Length)) { return; }

        powerCopyService.Copy(curr!, entities, [..SelectedSettings]);

        async Task<bool> ConfirmAsync(int count) =>
            inputService.IsKeyHeld(AlternateClickable.AlternateClickableActionKey)
            || await diag.ConfirmAsync(t.T("LV.PC.CopyConfirm", count));
    }

    async Task<EntityComponent[]> GetEntitiesFromDialogAsync()
    {
        var diag = container.GetInstance<ObjectSelectionDialog>();
        return await diag.ShowAsync(GenerateQuery(CopyLocation.Everywhere));
    }

    void UpdateStatistics()
    {
        if (!curr) { return; }

        var query = GenerateQuery(CopyLocation.Everywhere);
        btnLocEverywhere.text = t.T("LV.PC.CopyLocations_Anywhere", objectListingService.Count(query));

        if (currDb)
        {
            query = GenerateQuery(CopyLocation.District);
            btnLocDistrict.text = t.T("LV.PC.CopyLocations_District", objectListingService.Count(query), currDb!.District.DistrictName);
            btnLocDistrict.enabledSelf = true;
        }
        else
        {
            btnLocDistrict.text = t.T("LV.PC.CopyLocations_NoDistrict");
            btnLocDistrict.enabledSelf = false;
        }
    }

    ObjectListingQuery GenerateQuery(CopyLocation location)
    {
        var targetingAll = TargetingAll;

        return new(
            curr!,
            targetingAll ? null : currTemplate,
            [.. SelectedSettings],
            (location == CopyLocation.District && currDb && currDb!.District) ? currDb.District : null
        );
    }

    public enum CopyLocation
    {
        Everywhere,
        District,
        Area,
        List
    }

}
