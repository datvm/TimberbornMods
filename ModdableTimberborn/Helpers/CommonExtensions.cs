namespace ModdableTimberborn.Helpers;

public static partial class CommonExtensions
{
    public static void RemoveComponent<T>(this GameObject obj, bool immediately = true) where T : Object
    {
        var comp = obj.GetComponent<T>();
        if (!comp)
        {
            Debug.LogWarning($"Trying to remove component {typeof(T).Name} but it does not exist on {obj.name}");
            return;
        }

        if (immediately)
        {
            Object.DestroyImmediate(comp);
        }
        else
        {
            Object.Destroy(comp);
        }
    }

    public static TComp? GetComponentOrNullFast<TComp>(this BaseComponent component)
        where TComp : BaseComponent
    {
        var result = component.GetComponentFast<TComp>();
        return result ? result : null;
    }

    public static Lazy<Blueprint> ToLazyBlueprint<T>(this T spec) where T : ComponentSpec => new(() => spec.ToBlueprint());
    public static Blueprint ToBlueprint<T>(this T spec) where T : ComponentSpec => new([spec], []);
    public static T[] GetLazySpecs<T>(this SpecService specService) where T : ComponentSpec
        => [.. specService._cachedBlueprints[typeof(T)].Select(q => q.Value.GetSpec<T>())];
    
    public static BonusSpec ToBonusSpec(this BonusType t, float multiplierDelta) => new(t.ToString(), multiplierDelta);


}