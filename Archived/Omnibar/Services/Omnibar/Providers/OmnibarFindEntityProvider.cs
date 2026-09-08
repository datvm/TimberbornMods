namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarFindEntityProvider(
    IAssetLoader assets,
    ILoc t,
    EntityRegistry entities,
    EntityBadgeService entityBadgeService,
    EntitySelectionService entitySelectionService
) : IOmnibarCommandProvider
{
    public string Command { get; } = "/find ";
    public string CommandDesc { get; } = "LV.OB.CommandDescFind".T(t);
    public Texture2D Icon { get; } = assets.Load<Texture2D>("Sprites/Omnibar/tag");

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        if (!filter.StartsWith(Command)) { return []; }

        var nameKw = filter.Substring(Command.Length);
        if (nameKw.Length == 0) { return []; }

        List<OmnibarFilteredItem> result = [];

        foreach (var entity in entities.Entities)
        {
            if (entity.Deleted) { continue; }

            var badge = entityBadgeService.GetHighestPriorityEntityBadge(entity);
            var name = badge?.GetEntityName();
            if (string.IsNullOrEmpty(name)) { continue; }

            var match = OmnibarUtils.MatchText(nameKw, name!);
            if (!match.HasValue) { continue; }

            result.Add(new(
                new OmnibarEntityNameItem(entity, badge!, name!, entitySelectionService, Icon),
                match.Value));
        }

        return result;
    }

}

public class OmnibarEntityNameItem(
    EntityComponent entity,
    IEntityBadge badge,
    string name,
    EntitySelectionService entitySelectionService,
    Texture2D defaultIcon
) : IOmnibarItem
{
    public string Title { get; } = name;
    public IOmnibarDescriptor? Description { get; } = GetDescription(badge);
    public EntityComponent Entity { get; } = entity;

    public void Execute()
    {
        if (Entity)
        {
            entitySelectionService.SelectAndFocusOn(Entity);
        }
    }

    public bool SetIcon(Image image)
    {
        var sprite = badge.GetEntityAvatar();
        if (sprite is null)
        {
            image.image = defaultIcon;
        }
        else
        {
            image.sprite = sprite;
        }

        return true;
    }

    static SimpleLabelDescriptor? GetDescription(IEntityBadge badge)
    {
        var desc = badge.GetEntitySubtitle();
        if (string.IsNullOrEmpty(desc)) { return null; }

        return new SimpleLabelDescriptor(desc);
    }

}
