namespace Omnibar.Services.Omnibar.Providers;

public class OmnibarBeaverProvider(
    IAssetLoader assets, 
    ILoc t,
    EntityRegistry entities,
    EntityBadgeService entityBadgeService,
    EntitySelectionService entitySelectionService
) : IOmnibarCommandProvider
{
    public string Command { get; } = "/char ";
    public string CommandDesc { get; } = "LV.OB.CommandDescFind".T(t);
    public Texture2D Icon { get; } = assets.Load<Texture2D>("Sprites/BatchControl/Characters");

    public IReadOnlyList<OmnibarFilteredItem> ProvideItems(string filter)
    {
        if (!filter.StartsWith(Command)) { return []; }

        var nameKw = filter.Substring(Command.Length);
        if (nameKw.Length == 0) { return []; }

        List<OmnibarFilteredItem> result = [];

        foreach (var entity in entities.Entities)
        {
            var character = entity.GetComponentFast<Character>();
            if (!character || !character.Alive) { continue; }

            var citizen = entity.GetComponentFast<Citizen>();
            if (!citizen) { continue; }

            var match = OmnibarUtils.MatchText(nameKw, character.FirstName);
            if (!match.HasValue) { continue; }

            result.Add(new(
                new OmnibarBeaverItem(character, citizen, t, entityBadgeService, entitySelectionService),
                match.Value));
        }

        return result;
    }

}

public class OmnibarBeaverItem(
    Character character,
    Citizen citizen,
    ILoc t,
    EntityBadgeService entityBadgeService,
    EntitySelectionService entitySelectionService
) : IOmnibarItem
{
    public string Title { get; } = character.FirstName;
    public IOmnibarDescriptor? Description { get; } = new SimpleLabelDescriptor(citizen.AssignedDistrict 
        ? citizen.AssignedDistrict.DistrictName : 
        "BatchControl.NoDistrict".T(t));

    public void Execute()
    {
        entitySelectionService.SelectAndFocusOn(character);
    }

    public bool SetIcon(Image image)
    {
        var sprite = entityBadgeService.GetEntityAvatar(character);
        if (sprite is null) { return false; }

        image.sprite = sprite;
        return true;
    }
}
