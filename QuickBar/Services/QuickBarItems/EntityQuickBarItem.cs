namespace QuickBar.Services.QuickBarItems;

public class EntityQuickBarItem : IQuickBarItem
{
    public string Title { get; } = "";
    public Sprite? Sprite { get; }
    public Texture2D? Texture { get; }

    public EntityComponent Entity { get; }

    readonly EntitySelectionService entitySelectionService;

    public EntityQuickBarItem(
        EntityComponent entity,
        EntitySelectionService entitySelectionService,
        EntityBadgeService entityBadgeService
    )
    {
        Entity = entity;
        this.entitySelectionService = entitySelectionService;

        var badge = entityBadgeService.GetHighestPriorityEntityBadge(entity);
        Title = badge.GetEntityName() ?? "";
        Sprite = badge.GetEntityAvatar();
    }

    public void Activate()
    {
        entitySelectionService.Select(Entity);
    }

    public bool IsStillValid()
    {
        return Entity && !Entity.Deleted;
    }
}
