namespace ModdableTimberborn.Helpers;

partial class CommonExtensions
{

    public static bool IsGameContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.Game);
    public static bool IsMenuContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.MainMenu);
    public static bool IsBootstrapperContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.Bootstrapper);
    public static bool IsMapEditorContext(this ConfigurationContext context) => context.HasFlag(ConfigurationContext.MapEditor);
    public static bool IsGameplayContext(this ConfigurationContext context) => context.IsGameContext() || context.IsMapEditorContext();

    public static Configurator BindTemplateTailRunner<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, ITemplateCollectionServiceTailRunner 
        => configurator.MultiBindSingleton<ITemplateCollectionServiceTailRunner, T>(alsoBindSelf);

    public static Configurator BindTemplateModifier<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, ITemplateModifier 
        => configurator.MultiBindSingleton<ITemplateModifier, T>(alsoBindSelf);

    public static Configurator BindSpecTailRunner<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, ISpecServiceTailRunner 
        => configurator.MultiBindSingleton<ISpecServiceTailRunner, T>(alsoBindSelf);

    public static Configurator BindSpecModifier<T>(this Configurator configurator, bool alsoBindSelf = false)
        where T : class, ISpecModifier 
        => configurator.MultiBindSingleton<ISpecModifier, T>(alsoBindSelf);

}
