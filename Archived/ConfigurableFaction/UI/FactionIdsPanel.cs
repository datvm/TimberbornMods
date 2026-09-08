namespace ConfigurableFaction.UI;

public abstract class FactionIdsPanel<T>(
    FactionOptionsService optionsService,
    ILoc t
) : GroupPanel, IFactionItemsPanel
{
    readonly List<FactionItemRow<string>> rows = [];

#nullable disable
    protected FactionOptions options;
#nullable enable

    protected ILoc t = t;
    protected FactionOptionsService optionsService = optionsService;

    public event Action? OnItemChanged;

    protected abstract string HeaderLoc { get; }
    protected abstract ImmutableArray<T> GetSpecs(FactionInfo faction);
    protected abstract string GetId(T spec);
    protected abstract string GetText(T spec);
    protected abstract HashSet<string> OptionsList { get; }
    protected abstract HashSet<string> LockedInList { get; }
    protected abstract HashSet<string> ExistingList { get; }

    public IFactionItemsPanel Init(
        FactionOptions options,
        ImmutableArray<FactionInfo> otherFactions
    )
    {
        this.options = options;

        SetHeader(t.T(HeaderLoc));
        InitList(otherFactions);

        return this;
    }

    void InitList(ImmutableArray<FactionInfo> otherFactions)
    {
        var parent = Content;
        rows.Clear();

        foreach (var faction in otherFactions)
        {
            var factionRow = parent.AddChild().SetMarginBottom(10);
            factionRow.AddLabel(t.T("LV.CFac.FromFaction", faction.Spec.DisplayName.Value).Bold().Color(TimberbornTextColor.Solid));

            foreach (var spec in GetSpecs(faction))
            {
                var row = factionRow.AddChild<FactionItemRow<string>>();
                var id = GetId(spec);
                var text = GetText(spec);
                row.SetItem(id, text);

                row.SetLockFunc((id, isChecked) => isChecked && LockedInList.Contains(id));
                row.Value = OptionsList.Contains(id);

                row.SetAdditionalFilter((id, isChecked, filter) => !ExistingList.Contains(id));

                row.OnValueChanged += OnInternalRowChanged;

                rows.Add(row);
            }
        }
    }

    public void SetFilter(SettingsFilter filter)
    {
        foreach (var row in rows)
        {
            row.Filter(filter);
        }
    }

    void OnInternalRowChanged(string id, bool add)
    {
        OnRowChanged(id, add);
        OnItemChanged?.Invoke();
    }

    protected abstract void OnRowChanged(string id, bool add);

    public void RefreshItems()
    {
        foreach (var row in rows)
        {
            row.Value = OptionsList.Contains(row.Data);
        }
    }

}
