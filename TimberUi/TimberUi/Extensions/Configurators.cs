namespace Bindito.Core;

public static partial class UiBuilderExtensions
{

    public static IContainerDefinition BindFragments<T>(this IContainerDefinition def) where T : class, IProvider<EntityPanelModule>
    {
        ConstructorInfo constructor;
        try
        {
            constructor = typeof(T)
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Single(q => q.GetParameters().Length > 0);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException($"Provider {typeof(T).FullName} must have exactly one public constructor with parameters");
        }

        var bindMethod = typeof(IContainerDefinition).GetMethod(nameof(IContainerDefinition.Bind), BindingFlags.Public | BindingFlags.Instance);
        foreach (var p in constructor.GetParameters())
        {
            if (typeof(IEntityPanelFragment).IsAssignableFrom(p.ParameterType))
            {
                var binding = (IScopeAssignee)bindMethod.MakeGenericMethod(p.ParameterType).Invoke(def, []);
                binding.AsSingleton();
            }
        }

        def.MultiBind<EntityPanelModule>().ToProvider<T>().AsSingleton();

        return def;
    }

    public static Configurator BindFragments<T>(this Configurator configurator) where T : class, IProvider<EntityPanelModule>
    {
        configurator._containerDefinition.BindFragments<T>();
        return configurator;
    }

    public static Configurator BindFragment<T>(this Configurator configurator) where T : IEntityPanelFragment
    {
        configurator._containerDefinition.BindFragments<EntityPanelFragmentProvider<T>>();

        return configurator;
    }

    /// <summary>
    /// Bind Template Module with a fluent API.
    /// </summary>
    public static TemplateModuleHelper BindTemplateModule(this Configurator configurator) => new(configurator);

    /// <summary>
    /// Quick way to perform a multibind and then also bind the same type as a singleton.
    /// </summary>
    public static Configurator MultiBindAndBindSingleton<TMultiBind, T>(this Configurator configurator)
        where T : class, TMultiBind
        where TMultiBind : class
    {
        configurator.Bind<T>().AsSingleton();
        configurator.MultiBind<TMultiBind>().ToProvider<SimpleProvider<T>>().AsSingleton();

        return configurator;
    }

    /// <summary>
    /// Binding a runtime type (not generic)
    /// </summary>
    public static IScopeAssignee Bind(this Configurator configurator, Type type)
    {
        var bindMethod = typeof(IContainerDefinition).GetMethod(nameof(IContainerDefinition.Bind), BindingFlags.Public | BindingFlags.Instance);
        var binding = (IScopeAssignee)bindMethod.MakeGenericMethod(type).Invoke(configurator._containerDefinition, []);

        return binding;
    }

}
