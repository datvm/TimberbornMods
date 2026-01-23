namespace MoreNaming.Components;

public class EntityNamingComponent : BaseComponent, IPersistentEntity, IModifiableEntityBadge, IEntityBadge, IAwakableComponent
{
    static readonly ComponentKey SaveKey = new("BuildingNamingComponent");
    static readonly PropertyKey<string> NameKey = new("Name");

    public new string? Name { get; set; }

    public int EntityBadgePriority { get; } = 5;
    public IEntityBadge? UnderlyingEntityBadge { get; private set; }
    public IModifiableEntityBadge? ModifiableEntityBadge { get; private set; }

    public void Awake()
    {
        UnderlyingEntityBadge = GetUnderlyingEntityBadge();
        ModifiableEntityBadge = UnderlyingEntityBadge as IModifiableEntityBadge;
    }

    IEntityBadge? GetUnderlyingEntityBadge()
    {
        List<IEntityBadge> badges = [];
        GetComponents(badges);

        var currMax = int.MinValue;
        IEntityBadge? result = null;

        for (int i = 0; i < badges.Count; i++)
        {
            var b = badges[i];
            if (b is null || ReferenceEquals(b, this) || b.EntityBadgePriority <= currMax) { continue; }

            currMax = b.EntityBadgePriority;
            result = b;
        }

        return result;
    }

    public Sprite? GetEntityAvatar() => UnderlyingEntityBadge?.GetEntityAvatar();

    public ClickableSubtitle GetEntityClickableSubtitle() => UnderlyingEntityBadge?.GetEntityClickableSubtitle() ?? default;

    public string? GetEntityName() =>
        ModifiableEntityBadge?.GetEntityName()
        ?? Name
        ?? UnderlyingEntityBadge?.GetEntityName();

    public string? GetEntitySubtitle() => UnderlyingEntityBadge?.GetEntitySubtitle();

    public void Load(IEntityLoader entityLoader)
    {
        if (!entityLoader.TryGetComponent(SaveKey, out var s)) { return; }

        if (s.Has(NameKey))
        {
            Name = s.Get(NameKey);
        }
    }

    public void Save(IEntitySaver entitySaver)
    {
        if (Name is null) { return; }

        var s = entitySaver.GetComponent(SaveKey);
        s.Set(NameKey, Name);
    }

    public void SetEntityName(string entityName)
    {
        if (ModifiableEntityBadge is null)
        {
            Name = entityName;
        }
        else
        {
            ModifiableEntityBadge.SetEntityName(entityName);
        }
    }

}
