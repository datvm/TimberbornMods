namespace PowerCopy.UI;

public class PowerCopyFragment(
    ILoc t,
    DuplicableEntryFactory entryFac,
    ObjectListingService objectListingService,
    DialogService diag,
    InputService inputService,
    IContainer container,
    PowerCopyService powerCopyService,
    PowerCopyAreaTool areaTool
) : IEntityPanelFragment, IEntityFragmentOrder
{
    public int Order { get; } = -1;

#nullable disable
    public VisualElement Fragment => panel;

    EntityPanelFragmentElement panel;
    VisualElement entryList;
    Toggle chkTargetAll, chkTargetSameType;

    Button btnLocEverywhere, btnLocDistrict;
#nullable enable

    Toggle[] allTargets = [];

    EntityComponent? curr;
    DistrictBuilding? currDb;

    string selectingBuildingName = "";
    string selectingTemplateName = "";
    readonly List<IDuplicable> duplicables = [];
    readonly List<KeyValuePair<IDuplicable, DuplicableEntry>> currEntries = [];
    readonly Dictionary<Type, DuplicableEntry> allEntries = [];

    public bool TargetingAll => chkTargetAll.value;
    public IEnumerable<Type> SelectedTypes => currEntries
        .Where(e => e.Value.Selected)
        .Select(e => e.Value.Type);

    public void ClearFragment()
    {
        panel.Visible = false;
        curr = null;
        currDb = null;
        duplicables.Clear();

        foreach (var e in currEntries)
        {
            e.Value.Visible = false;
        }
        currEntries.Clear();
    }

    public VisualElement InitializeFragment()
    {
        panel = new()
        {
            Visible = false,
            Background = EntityPanelFragmentBackground.PalePurple,
        };

        var options = panel.AddChild().SetMarginBottom();
        options.AddLabel(t.T("LV.PC.CopyOptions"));
        entryList = options.AddChild();

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
    }

    public void ShowFragment(BaseComponent entity)
    {
        curr = entity.GetComponent<EntityComponent>();
        if (!curr)
        {
            ClearFragment();
            return;
        }

        var template = entity.GetComponent<TemplateSpec>();
        if (template is null)
        {
            ClearFragment();
            return;
        }
        selectingTemplateName = template.TemplateName;

        var label = entity.GetComponent<LabeledEntity>();
        if (label is null)
        {
            ClearFragment();
            return;
        }
        selectingBuildingName = label.DisplayName;

        powerCopyService.GetDuplicables(entity, duplicables);
        if (duplicables.Count == 0)
        {
            ClearFragment();
            return;
        }

        chkTargetSameType.text = t.T("LV.PC.CopyTargets_SameType", selectingBuildingName);

        AddAndShowTypes();

        currDb = entity.GetComponent<DistrictBuilding>();
        if (!currDb || !currDb.District)
        {
            currDb = null;
        }

        UpdateFragment(); // To adjust visibility so statistics will be correct
        UpdateStatistics();

        panel.Visible = true;
    }

    void AddAndShowTypes()
    {
        var added = false;

        foreach (var duplicable in duplicables)
        {
            var t = duplicable.GetType();

            var entry = allEntries.GetOrAdd(t, () =>
            {
                added = true;
                return entryFac.CreateFor(t);
            });

            entry.OnSelectedChanged += OnEntryChanged;

            currEntries.Add(new(duplicable, entry));
        }

        if (added)
        {
            entryList.Clear();

            var sorted = allEntries.Values
                .OrderBy(q => q.Order)
                .ThenBy(q => q.Name);
            foreach (var entry in sorted)
            {
                entryList.Add(entry);
            }
        }
    }

    void OnEntryChanged(bool obj)
    {
        UpdateStatistics();
    }

    public void UpdateFragment()
    {
        if (curr is null || currEntries.Count == 0) { return; }

        foreach (var (comp, entry) in currEntries)
        {
            var visible = entry.Visible = comp.IsDuplicable;

            if (visible && entry is IDuplicableUpdatableEntry u)
            {
                u.UpdateFor(comp);
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

        powerCopyService.Duplicate(curr!, [.. SelectedTypes], entities);

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
            targetingAll ? null : selectingTemplateName,
            [.. SelectedTypes],
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
