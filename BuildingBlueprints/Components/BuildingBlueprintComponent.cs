namespace BuildingBlueprints.Components;

[AddTemplateModule2(typeof(BuildingSpec))]
public class BuildingBlueprintComponent(BlueprintGroupService groupService) : BaseComponent, IPersistentEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BuildingBlueprintComponent));
    static readonly PropertyKey<int> GroupKey = new("Group");

    public int BlueprintGroup { get; private set; }
    public bool HasGroup => BlueprintGroup > 0;

    public void DeleteEntity() => groupService.Unregister(this);

    public void AssignToGroup(int group)
    {
        BlueprintGroup = group;
        groupService.Register(this);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        BlueprintGroup = s.Get(GroupKey);
        groupService.Register(this);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!HasGroup) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(GroupKey, BlueprintGroup);
    }
}
