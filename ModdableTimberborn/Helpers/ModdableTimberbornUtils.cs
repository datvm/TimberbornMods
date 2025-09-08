namespace ModdableTimberborn.Helpers;

public static class ModdableTimberbornUtils
{

    public static void PatchAwakePostfix<TComp, TModdedComponent>(this TComp comp)
        where TComp : BaseComponent
        where TModdedComponent : BaseModdableComponent<TComp>
    {
        var modded = comp.GetComponentFast<TModdedComponent>();
        modded.OriginalComponent = comp;

        if (modded is IModdableComponentAwake a)
        {
            a.AwakeAfter();
        }
    }

    public static void PatchStartPostfix<TComp, TModdedComponent>(this TComp comp)
        where TComp : BaseComponent
        where TModdedComponent : BaseModdableComponent<TComp>, IModdableComponentStart
    {
        var modded = comp.GetComponentFast<TModdedComponent>();
        modded.StartAfter();
    }

}
