namespace ModdableTimberborn.EntityTracker;

public class CharacterTracker : IEntityTracker<CharacterTrackerComponent>
{
    readonly HashSet<CharacterTrackerComponent> adults = [];
    public IReadOnlyCollection<CharacterTrackerComponent> Adults => adults;

    readonly HashSet<CharacterTrackerComponent> children = [];
    public IReadOnlyCollection<CharacterTrackerComponent> Children => children;

    readonly HashSet<CharacterTrackerComponent> bots = [];
    public IReadOnlyCollection<CharacterTrackerComponent> Bots => bots;

    public IEnumerable<CharacterTrackerComponent> Beavers => adults.Concat(children);
    public IEnumerable<CharacterTrackerComponent> Workers => adults.Concat(bots);

    readonly HashSet<CharacterTrackerComponent> entities = [];
    public IReadOnlyCollection<CharacterTrackerComponent> Entities => entities;

    public event Action<CharacterTrackerComponent>? OnEntityRegistered;
    public event Action<CharacterTrackerComponent>? OnEntityUnregistered;

    public void Track(EntityComponent entity)
    {
        var character = entity.GetComponentFast<CharacterTrackerComponent>();
        if (!character) { return; }

        GetSet(character.CharacterType).Add(character);
        entities.Add(character);        
        
        OnEntityRegistered?.Invoke(character);
    }

    public void Untrack(EntityComponent entity)
    {
        var character = entity.GetComponentFast<CharacterTrackerComponent>();
        if (!character) { return; }

        GetSet(character.CharacterType).Remove(character);
        entities.Remove(character);

        OnEntityUnregistered?.Invoke(character);
    }

    HashSet<CharacterTrackerComponent> GetSet(CharacterType type) => type switch
    {
        CharacterType.AdultBeaver => adults,
        CharacterType.ChildBeaver => children,
        CharacterType.Bot => bots,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}
