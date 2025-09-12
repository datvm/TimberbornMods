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

    public static bool IsGameContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.Game);
    public static bool IsMenuContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.MainMenu);
    public static bool IsBootstrapperContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.Bootstrapper);
    public static bool IsMapEditorContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.MapEditor);

}