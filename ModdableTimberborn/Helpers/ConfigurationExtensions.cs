namespace ModdableTimberborn.Helpers;

partial class CommonExtensions
{

    public static bool IsGameContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.Game);
    public static bool IsMenuContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.MainMenu);
    public static bool IsBootstrapperContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.Bootstrapper);
    public static bool IsMapEditorContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.MapEditor);
    public static bool IsGameplayContext(this ConfigurationContext context) => context.IsGameContext() || context.IsMapEditorContext();

    public static Configurator BindPrefabModifier<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, IPrefabModifier
    {
        return configurator.MultiBindSingleton<IPrefabModifier, T>(alsoBindSelf);
    }

    public static Configurator BindSpecModifier<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, ISpecModifier
    {
        return configurator.MultiBindSingleton<ISpecModifier, T>(alsoBindSelf);
    }

}
