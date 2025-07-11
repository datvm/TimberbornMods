using MacroManagement.Components;

namespace HueAndTurn.Components;

public class MMHueAndTurnComponent(BaseComponent mm) : IHueAndTurnComponent
{

    public static MMHueAndTurnComponent? TryGetMM(BaseComponent comp)
    {
        var mm = comp.GetComponentFast<MMComponent>();
        if (!mm) { return null; }

        var result = new MMHueAndTurnComponent(mm);
        return result.Original ? result : null;
    }

    [NotNull]
    public HueAndTurnComponent? Original { get; } = ((MMComponent)mm).Original.Prefab.GetComponentFast<HueAndTurnComponent>();

    public bool CanHaveTransparency => Original.CanHaveTransparency;
    public string DisplayName => Original.DisplayName;
    public bool HasFluid => Original.HasFluid;
    public string PrefabName => Original.PrefabName;
    public PrefabSpec? PrefabSpec => Original.PrefabSpec;
    public ReadOnlyHueAndTurnProperties Properties => Original.Properties;
    public bool RotationPivotSupported => Original.RotationPivotSupported;

    public void ApplyProperties(ReadOnlyHueAndTurnProperties properties)
        => ApplyAll(h => h.ApplyProperties(properties));

    public void Reset()
        => ApplyAll(h => h.Reset());

    public void SetColor(Color? color)
        => ApplyAll(h => h.SetColor(color));

    public void SetFluidColor(Color? color)
        => ApplyAll(h => h.SetFluidColor(color));

    public void SetRotation(int? rotation)
        => ApplyAll(h => h.SetRotation(rotation));

    public void SetRotationPivot(Vector2Int? rotationPivot)
        => ApplyAll(h => h.SetRotationPivot(rotationPivot));

    public void SetRotationWithPivot(int? rotation, Vector2Int? rotationPivot)
        => ApplyAll(h => h.SetRotationWithPivot(rotation, rotationPivot));

    public void SetRotationXZ(Vector2Int? rotationXZ)
        => ApplyAll(h => h.SetRotationXZ(rotationXZ));

    public void SetScale(Vector3Int? scale)
        => ApplyAll(h => h.SetScale(scale));

    public void SetTranslation(Vector3Int? translation)
        => ApplyAll(h => h.SetTranslation(translation));

    public void SetTransparency(int? transparency)
        => ApplyAll(h => h.SetTransparency(transparency));

    void ApplyAll(Action<HueAndTurnComponent> action)
    {
        foreach (var e in ((MMComponent)mm).SelectedBuildings)
        {
            var hat = e.Prefab.GetComponentFast<HueAndTurnComponent>();
            if (!hat) { continue; }

            action(hat);
        }
    }

}
