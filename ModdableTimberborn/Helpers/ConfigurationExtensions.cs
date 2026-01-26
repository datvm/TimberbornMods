namespace ModdableTimberborn.Helpers;

partial class CommonExtensions
{

    extension(ConfigurationContext context)
    {

        public bool IsGameContext() => context.HasFlag(ConfigurationContext.Game);
        public bool IsMenuContext() => context.HasFlag(ConfigurationContext.MainMenu);
        public bool IsBootstrapperContext() => context.HasFlag(ConfigurationContext.Bootstrapper);
        public bool IsMapEditorContext() => context.HasFlag(ConfigurationContext.MapEditor);
        public bool IsGameplayContext() => context.IsGameContext() || context.IsMapEditorContext();

        public BindAttributeContext ToBindAttributeContext() => context switch
        {
            ConfigurationContext.Bootstrapper => BindAttributeContext.Bootstrapper,
            ConfigurationContext.MainMenu => BindAttributeContext.MainMenu,
            ConfigurationContext.Game => BindAttributeContext.Game,
            ConfigurationContext.MapEditor => BindAttributeContext.MapEditor,
            _ => throw new ArgumentOutOfRangeException(nameof(context), $"Cannot convert context value '{context}' to {nameof(BindAttributeContext)}."),
        };

    }

    extension(Configurator configurator)
    {
        public Configurator BindAttributes(ConfigurationContext context, Assembly? assembly = default, Scope defaultScope = Scope.Singleton)
        {
            assembly ??= Assembly.GetCallingAssembly();

            return configurator.BindAttributes(context.ToBindAttributeContext(), assembly, defaultScope);
        }

        public Configurator BindTemplateTailRunner<T>(bool alsoBindSelf = false)
            where T : class, ITemplateCollectionServiceTailRunner
            => configurator.MultiBindSingleton<ITemplateCollectionServiceTailRunner, T>(alsoBindSelf);

        public Configurator BindTemplateModifier<T>(bool alsoBindSelf = false)
            where T : class, ITemplateModifier
            => configurator.MultiBindSingleton<ITemplateModifier, T>(alsoBindSelf);

        public Configurator BindSpecTailRunner<T>(bool alsoBindSelf = false)
            where T : class, ISpecServiceTailRunner
            => configurator.MultiBindSingleton<ISpecServiceTailRunner, T>(alsoBindSelf);

        public Configurator BindSpecModifier<T>(bool alsoBindSelf = false)
            where T : class, ISpecModifier
            => configurator.MultiBindSingleton<ISpecModifier, T>(alsoBindSelf);

    }

}
