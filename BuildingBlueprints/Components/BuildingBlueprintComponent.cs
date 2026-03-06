namespace BuildingBlueprints.Components;

[AddTemplateModule2(typeof(BuildingSpec))]
public class BuildingBlueprintComponent(BlueprintGroupService groupService) : BaseComponent, IPersistentEntity, IDeletableEntity
{
    static readonly ComponentKey SaveKey = new(nameof(BuildingBlueprintComponent));
    static readonly PropertyKey<int> GroupKey = new("Group");
    static readonly PropertyKey<Guid> OriginalGuidKey = new("OriginalGuid");

    public int BlueprintGroup { get; private set; }
    public bool HasGroup => BlueprintGroup > 0;
    public Guid? OriginalGuid { get; private set; }

    public void DeleteEntity() => groupService.Unregister(this);

    public void AssignToGroup(int group, Guid? originalId)
    {
        BlueprintGroup = group;
        OriginalGuid = originalId;
        groupService.Register(this);
    }

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }
        
        if (s.Has(GroupKey))
        {
            BlueprintGroup = s.Get(GroupKey);
        }
        
        if (s.Has(OriginalGuidKey))
        {
            OriginalGuid = s.Get(OriginalGuidKey);
        }

        groupService.Register(this);
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (!HasGroup) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(GroupKey, BlueprintGroup);

        if (OriginalGuid.HasValue)
        {
            s.Set(OriginalGuidKey, OriginalGuid.Value);
        }
    }
}
