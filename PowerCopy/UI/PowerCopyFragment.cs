namespace PowerCopy.UI;

public class PowerCopyFragment(
    ILoc t,
    DuplicableEntryFactory entryFac
) : IEntityPanelFragment, IEntityFragmentOrder
{
    public int Order { get; } = -1;

#nullable disable
    public VisualElement Fragment => panel;

    EntityPanelFragmentElement panel;
    VisualElement entryList;
    Toggle chkTargetAll, chkTargetSameType;
#nullable enable

    Toggle[] allTargets = [];

    TemplateSpec? curr;
    string selectingBuildingName = "";
    readonly List<IDuplicable> duplicables = [];
    readonly List<KeyValuePair<IDuplicable, DuplicableEntry>> currEntries = [];
    readonly Dictionary<Type, DuplicableEntry> allEntries = [];

    public bool TargetingAll => chkTargetAll.value;

    public void ClearFragment()
    {
        panel.Visible = false;
        curr = null;
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
        OnTargetSelected(true);

        return panel;
    }

    public void ShowFragment(BaseComponent entity)
    {
        curr = entity.GetComponent<TemplateSpec>();
        if (curr is null) { return; }

        var label = entity.GetComponent<LabeledEntity>();
        if (label is null)
        {
            ClearFragment();
            return;
        }
        selectingBuildingName = label.DisplayName;

        chkTargetSameType.text = t.T("LV.PC.CopyTargets_SameType", selectingBuildingName);

        entity.GetComponents(duplicables);
        if (duplicables.Count == 0)
        {
            ClearFragment();
            return;
        }

        AddAndShowTypes();
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
    }

}
