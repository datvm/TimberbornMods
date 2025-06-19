namespace MacroManagement.Components.DummyComponents;

public class DummyEntityBadge : BaseComponent, IEntityBadge
{
    public int EntityBadgePriority { get; private set; } = 0;
    IEntityBadge? mainBadge;

    public Sprite? GetEntityAvatar() => mainBadge?.GetEntityAvatar();
    public ClickableSubtitle GetEntityClickableSubtitle() => mainBadge?.GetEntityClickableSubtitle() ?? default;
    public string? GetEntityName() => mainBadge?.GetEntityName();
    public string? GetEntitySubtitle() => mainBadge?.GetEntitySubtitle();

    public void Init(BaseComponent original, EntityBadgeService entityBadgeService)
    {
        mainBadge = entityBadgeService.GetHighestPriorityEntityBadge(original);
        EntityBadgePriority = mainBadge?.EntityBadgePriority ?? 0;
    }
}
