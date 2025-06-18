namespace QuickBar.Services.QuickBarItems;

public class EntityQuickBarItemProvider(
    EntitySelectionService entitySelectionService,
    EntityBadgeService entityBadgeService,
    EntityRegistry entities
) : IQuickBarItemProvider
{
    public ImmutableHashSet<Type> SupportedType { get; } = [typeof(EntityQuickBarItem)];

    public IQuickBarItem? Deserialize(string data)
    {
        if (!Guid.TryParse(data, out var guid)) { return null; }

        var entity = entities.GetEntity(guid);
        if (!entity)
        {
            Debug.LogWarning($"Entity with ID {guid} not found. It may no longer exist.");
            return null;
        }

        return new EntityQuickBarItem(entity, entitySelectionService, entityBadgeService);
    }

    public string? Serialize(IQuickBarItem item)
    {
        var entity = ((EntityQuickBarItem)item).Entity;
        if (!entity) { return null; }

        var id = entity.EntityId.ToString();
        return id;
    }

    public bool TryCreateItem(IOmnibarItem omnibarItem, [NotNullWhen(true)] out IQuickBarItem? quickbarItem)
    {
        quickbarItem = default;
        if (omnibarItem is not OmnibarEntityNameItem nameItem) { return false; }

        var entity = nameItem.Entity;
        if (!entity) { return false; }

        quickbarItem = CreateFromEntity(entity);
        return true;
    }

    public IQuickBarItem CreateFromEntity(EntityComponent entity) 
        => new EntityQuickBarItem(entity, entitySelectionService, entityBadgeService);

}
