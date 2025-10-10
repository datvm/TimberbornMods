namespace ScientificProjects.Services;

public class CharacterTracker
{
    public event Action<CharacterProjectUpgradeComponent>? OnRegistered;
    public event Action<CharacterProjectUpgradeComponent>? OnUnregistered;

    readonly HashSet<CharacterProjectUpgradeComponent> bots = [];
    readonly HashSet<CharacterProjectUpgradeComponent> adults = [];
    readonly HashSet<CharacterProjectUpgradeComponent> children = [];

    public IReadOnlyCollection<CharacterProjectUpgradeComponent> Bots => bots;
    public IReadOnlyCollection<CharacterProjectUpgradeComponent> Adults => adults;
    public IReadOnlyCollection<CharacterProjectUpgradeComponent> Children => children;
    public IEnumerable<CharacterProjectUpgradeComponent> Workers => adults.Concat(bots);
    public IEnumerable<CharacterProjectUpgradeComponent> AllBeavers => adults.Concat(children);
    public IEnumerable<CharacterProjectUpgradeComponent> AllCharacters => AllBeavers.Concat(bots);

    public void Register(CharacterProjectUpgradeComponent comp)
    {
        switch (comp.CharacterType)
        {
            case CharacterType.Bot:
                bots.Add(comp);
                break;
            case CharacterType.AdultBeaver:
                adults.Add(comp);
                break;
            case CharacterType.ChildBeaver:
                children.Add(comp);
                break;
            default:
                Debug.LogWarning("Unknown character is getting registered: " + comp);
                return;
        }

        OnRegistered?.Invoke(comp);
    }

    public void Unregister(CharacterProjectUpgradeComponent comp)
    {
        bots.Remove(comp);
        adults.Remove(comp);
        children.Remove(comp);

        OnUnregistered?.Invoke(comp);
    }

}
