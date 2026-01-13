namespace BlueprintRelics.Services;

public class BlueprintRelicsRegistry
{
    readonly HashSet<BlueprintRelicComponent> relics = [];
    public IReadOnlyCollection<BlueprintRelicComponent> Relics => relics;

    public int Count => relics.Count;

    public void Register(BlueprintRelicComponent comp)
    {
        relics.Add(comp);
    }

    public void Unregister(BlueprintRelicComponent comp)
    {
        relics.Remove(comp);
    }

}
