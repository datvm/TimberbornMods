namespace TimberUi.Extensions;

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

}
