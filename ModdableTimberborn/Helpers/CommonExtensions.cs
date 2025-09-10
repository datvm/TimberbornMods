namespace ModdableTimberborn.Helpers;

public static class CommonExtensions
{

    public static TComp? GetComponentOrNullFast<TComp>(this BaseComponent component)
        where TComp : BaseComponent
    {
        var result = component.GetComponentFast<TComp>();
        return result ? result : null;
    }

    public static BonusManager GetBonusManager<T>(this T component)
        where T : BaseComponent
        => component.GetComponentFast<BonusManager>();

}
