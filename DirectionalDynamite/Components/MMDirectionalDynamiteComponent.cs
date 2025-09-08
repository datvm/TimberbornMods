namespace DirectionalDynamite.Components;

public class MMDirectionalDynamiteComponent(BaseComponent mm) : IDirectionalDynamiteComponent
{
    [NotNull]
    public DirectionalDynamiteComponent? Original { get; } = ((MMComponent)mm).Original.Prefab.GetComponentFast<DirectionalDynamiteComponent>();

    public static MMDirectionalDynamiteComponent? TryGetMM(BaseComponent comp)
    {
        var mm = comp.GetComponentFast<MMComponent>();
        if (!mm) { return null; }

        var result = new MMDirectionalDynamiteComponent(mm);
        return result.Original ? result : null;
    }

    public Vector3Int Coordinates => Original.Coordinates;
    public Direction3D Direction
    {
        get => Original.Direction;
        set => ApplyToAll(dd => dd.Direction = value);
    }
    public bool DoNotTriggerNeighbor
    {
        get => Original.DoNotTriggerNeighbor;
        set => ApplyToAll(dd => dd.DoNotTriggerNeighbor = value);
    }
    public int MaxDepth => Original.MaxDepth;

    public void HideIndicator()
    {
        var dds = GetAll();
        foreach (var d in dds)
        {
            d.HideIndicator();
        }
    }

    public void ShowIndicator(Sprite arrow)
    {
        var dds = GetAll();
        foreach (var d in dds)
        {
            d.ShowIndicator(arrow);
        }

        dds.FirstOrDefault()?.service?.HighlightDestroyingEntities(dds);
    }

    List<DirectionalDynamiteComponent> GetAll()
    {
        List<DirectionalDynamiteComponent> result = [];
        foreach (var b in ((MMComponent)mm).SelectedBuildings)
        {
            var dd = b.Prefab.GetComponentFast<DirectionalDynamiteComponent>();
            if (dd)
            {
                result.Add(dd);
            }
        }

        return result;
    }

    void ApplyToAll(Action<DirectionalDynamiteComponent> action)
    {
        foreach (var dd in GetAll())
        {
            action(dd);
        }
    }

}
