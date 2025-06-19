namespace MacroManagement.Components;

public class MMComponent : BaseComponent, IDeletableEntity
{

#nullable disable
    EntityService entityService;
#nullable enable

    public event Action<MMBuildingItem, bool>? OnSelectionChanged;

    [Inject]
    public void Inject(EntityService entityService) => this.entityService = entityService;

    public bool IsUnselecting { get; set; }

    public ImmutableArray<MMBuildingItem> Buildings { get; private set; }
    public MMBuildingItem Original { get; private set; } = null!;

    public MMBuildingItem[] SelectedBuildings => [.. Buildings.Where(q => q.Select)];

    public void DeleteEntity()
    {
        if (IsUnselecting) { return; }

        foreach (var item in SelectedBuildings)
        {
            entityService.Delete(item.Prefab);
        }
    }

    public void Init(ImmutableArray<PrefabSpec> buildings, PrefabSpec original, EntityBadgeService entityBadgeService)
    {
        Buildings = [.. buildings.Select(q => CreateBuildingItem(q, entityBadgeService))];
        Original = CreateBuildingItem(original, entityBadgeService);

        foreach (var building in Buildings)
        {
            building.OnSelectChanged += (item, select) =>
            {
                OnSelectionChanged?.Invoke(item, select);
            };
        }
    }

    MMBuildingItem CreateBuildingItem(PrefabSpec prefab, EntityBadgeService entityBadgeService)
    {
        var pausable = prefab.GetComponentFast<PausableBuilding>();

        var badge = entityBadgeService.GetHighestPriorityEntityBadge(prefab);
        var name = badge?.GetEntityName();

        if (name is null)
        {
            var label = prefab.GetComponentFast<LabeledEntity>();
            name = label?.DisplayName ?? prefab.PrefabName;
        }

        string? districtName = null;
        var district = prefab.GetComponentFast<DistrictBuilding>();
        if (district && district.District)
        {
            var districtBadge = entityBadgeService.GetHighestPriorityEntityBadge(district.District);
            districtName = districtBadge?.GetEntityName();
        }

        return new(prefab, pausable, badge, name, districtName,
            prefab.GetComponentFast<SingleGoodAllower>(),
            prefab.GetComponentFast<Dwelling>()
        );
    }

}

public record MMBuildingItem(
    PrefabSpec Prefab,
    PausableBuilding? Pausable,
    IEntityBadge? Badge,
    string Name,
    string? DistrictName,
    SingleGoodAllower? SingleGoodAllower,
    Dwelling? Dwelling
)
{
    public event Action<MMBuildingItem, bool>? OnSelectChanged;

    public bool Select { get; private set; } = true;

    public bool IsPaused => Pausable && Pausable.Paused;

    public void ToggleSelect(bool select)
    {
        Select = select;
        OnSelectChanged?.Invoke(this, select);
    }

}