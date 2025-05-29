namespace Bindito.Core;

public static partial class UiBuilderExtensions
{

    /// <summary>
    /// Bind anEntityPanelFragments and all it needs.
    /// </summary>
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

    /// <summary>
    /// Bind an EntityPanelFragments and all it needs.
    /// </summary>
    public static Configurator BindFragments<T>(this Configurator configurator) where T : class, IProvider<EntityPanelModule>
    {
        configurator._containerDefinition.BindFragments<T>();
        return configurator;
    }

    /// <summary>
    /// Bind anEntityPanelFragments and all it needs.
    /// </summary>
    public static Configurator BindFragment<T>(this Configurator configurator) where T : IEntityPanelFragment
    {
        configurator._containerDefinition.BindFragments<EntityPanelFragmentProvider<T>>();

        return configurator;
    }

    /// <summary>
    /// Bind Template Module with a fluent API.
    /// </summary>
    public static TemplateModuleHelper BindTemplateModule(this Configurator configurator) => new(configurator);

    public static MassRebindingHelper MassRebind(this Configurator configurator) => new(configurator);

    /// <summary>
    /// Quick way to perform a multibind and then also bind the same type as a singleton.
    /// </summary>
    public static Configurator MultiBindAndBindSingleton<TMultiBind, T>(this Configurator configurator)
        where T : class, TMultiBind
        where TMultiBind : class
    {
        configurator.Bind<T>().AsSingleton();
        configurator.MultiBind<TMultiBind>().ToExisting<T>();

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

    public static Configurator BindSingleton(this Configurator configurator, Type src, Type dst)
        => configurator.BindSingleton(src, dst, false);

    public static Configurator BindSingleton(this Configurator configurator, Type src, Type dst, bool alsoBindDestByItself)
    {
        if (alsoBindDestByItself)
        {
            configurator.BindSingleton(dst);
        }

        var assignee = configurator.Bind(src);

        if (alsoBindDestByItself)
        {
            var toExistingMethod = assignee.GetType().GetMethod(nameof(IBindingBuilder<>.ToExisting))
                .MakeGenericMethod(dst);
            toExistingMethod.Invoke(assignee, []);
        }
        else
        {
            var toMethod = assignee.GetType().GetMethod(nameof(IBindingBuilder<>.To))
                .MakeGenericMethod(dst);

            var scope = (IScopeAssignee)toMethod.Invoke(assignee, []);
            scope.AsSingleton();
        }

        return configurator;
    }

    public static Configurator BindSingleton<T, TImpl>(this Configurator configurator)
        where T : class
        where TImpl : class, T
    {
        configurator.Bind<T>().To<TImpl>().AsSingleton();
        return configurator;
    }

    public static Configurator BindSingleton<T>(this Configurator configurator)
        where T : class
    {
        configurator.Bind<T>().AsSingleton();
        return configurator;
    }

    public static Configurator BindSingleton(this Configurator configurator, Type type)
    {
        configurator.Bind(type).AsSingleton();
        return configurator;
    }

    public static Configurator MassRemoveBindings(this Configurator configurator, params IEnumerable<Type> types)
    {
        var registry = GetRegistry(configurator);

        foreach (var t in types)
        {
            registry._boundBindingBuilders.Remove(t);
        }

        return configurator;
    }

    public static Configurator RemoveBinding<T>(this Configurator configurator)
        where T : class
        => configurator.MassRemoveBindings(typeof(T));

    public static Configurator RemoveMultibinding(this Configurator configurator, Type type)
    {
        var registry = GetRegistry(configurator);
        registry._boundMultiBindingBuilders.Remove(type);
        return configurator;
    }

    public static Configurator RemoveMultibindings(this Configurator configurator, Type type, HashSet<Type> types)
    {
        var registry = GetRegistry(configurator);

        if (!registry._boundMultiBindingBuilders.TryGetValue(type, out var multiBindingBuilder)) { return configurator; }
        for (int i = 0; i < multiBindingBuilder.Count; i++)
        {
            var binding = multiBindingBuilder[i];

            var provisionBindingField = binding.GetType().GetField("_provisionBinding", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var provisionBinding = (ProvisionBinding)provisionBindingField.GetValue(binding);

            if (types.Contains(provisionBinding.Type))
            {
                multiBindingBuilder.RemoveAt(i);
                i--;
            }
        }

        return configurator;
    }

    public static Configurator RemoveMultibinding<T>(this Configurator configurator)
        where T : class
        => configurator.RemoveMultibinding(typeof(T));

    public static Configurator RemoveMultibindings<T, TImpl>(this Configurator configurator)
        where T : class
        where TImpl : class, T
        => configurator.RemoveMultibindings(typeof(T), [typeof(TImpl)]);

    /// <summary>
    /// Bind a type if it has not been bound yet.
    /// </summary>
    public static ISingleBindingBuilder<T>? TryBind<T>(this Configurator configurator) where T : class
    {
        var cd = (ContainerDefinition)configurator._containerDefinition;
        var registry = (BindingBuilderRegistry)cd._bindingBuilderRegistry;

        if (registry._boundBindingBuilders.ContainsKey(typeof(T))) { return null; }

        return configurator.Bind<T>();
    }

    /// <summary>
    /// Bind services needed for camera shake (can be called multiple times in case it's called from multiple mods).
    /// </summary>
    public static Configurator TryAddingCameraShake(this Configurator configurator, bool isMenuContext)
    {
        if (!isMenuContext)
        {
            configurator.TryBind<CameraShakeService>()?.AsSingleton();
        }
        configurator.TryBind<CameraShakeSettingService>()?.AsSingleton();

        return configurator;
    }

    /// <summary>
    /// Get the BindingBuilderRegistry from the configurator.
    /// </summary>
    public static BindingBuilderRegistry GetRegistry(Configurator configurator)
    {
        var def = (ContainerDefinition)configurator._containerDefinition;
        var registry = (BindingBuilderRegistry)def._bindingBuilderRegistry;

        return registry;
    }

}
