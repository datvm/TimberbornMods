namespace Bindito.Core;

public static partial class UiBuilderExtensions
{
    /// <summary>
    /// Represents a combination of <see cref="BindingFlags"/> used to specify instance-level binding, including both public and non-public members.
    /// </summary>
    public static readonly BindingFlags InstanceBindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    /// <summary>
    /// Represents the <see cref="MethodInfo"/> for the <see cref="IContainerDefinition.Bind"/> method.
    /// Note: it is early bound and a call to <see cref="MethodInfo.MakeGenericMethod(Type[])"/> is required to use it with a specific type.
    /// </summary>
    public static readonly MethodInfo IContainerDefinitionBind = typeof(IContainerDefinition).GetMethod(nameof(IContainerDefinition.Bind), InstanceBindingFlags);

    /// <summary>
    /// Represents the <see cref="MethodInfo"/> for the <see cref="IContainerDefinition.MultiBind"/> method.
    /// /// Note: it is early bound and a call to <see cref="MethodInfo.MakeGenericMethod(Type[])"/> is required to use it with a specific type.
    /// </summary>
    public static readonly MethodInfo IContainerDefinitionMultiBind = typeof(IContainerDefinition).GetMethod(nameof(IContainerDefinition.MultiBind), InstanceBindingFlags);

    #region Fragments

    /// <summary>
    /// Binds all fragments required by the specified provider type to the container definition.
    /// </summary>
    /// <remarks>This method inspects the public constructor of the specified provider type <typeparamref
    /// name="T"/> to identify its dependencies. Any constructor parameters that implement <see
    /// cref="IEntityPanelFragment"/> are automatically bound to the container as singletons. The provider itself is
    /// also bound to the container as a singleton.</remarks>
    /// <typeparam name="T">The type of the provider that implements <see cref="IProvider{T}"/> for <see cref="EntityPanelModule"/>. The
    /// provider must have exactly one public constructor with parameters.</typeparam>
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
    /// Binds fragments of type <typeparamref name="T"/> to the current configurator.
    /// </summary>
    /// <remarks>This method is an extension method that facilitates the binding of fragments to a
    /// configurator. It is typically used to register fragment providers for dependency injection or modular
    /// configuration.</remarks>
    /// <typeparam name="T">The type of the fragment provider to bind. Must implement <see cref="IProvider{T}"/> with <see
    /// cref="EntityPanelModule"/> as the generic type parameter.</typeparam>
    public static Configurator BindFragments<T>(this Configurator configurator) where T : class, IProvider<EntityPanelModule>
    {
        configurator._containerDefinition.BindFragments<T>();
        return configurator;
    }

    /// <summary>
    /// Binds a fragment of type <typeparamref name="T"/> to the configurator.
    /// </summary>
    /// <typeparam name="T">The type of the fragment to bind. Must implement <see cref="IEntityPanelFragment"/>.</typeparam>
    public static Configurator BindFragment<T>(this Configurator configurator) where T : IEntityPanelFragment
    {
        configurator.BindFragments<EntityPanelFragmentProvider<T>>();

        return configurator;
    }

    #endregion

    #region Bindings

    /// <summary>
    /// Binds a type to the configurator.
    /// </summary>
    /// <remarks>
    /// Use this method to bind a type unknown at compile type.
    /// </remarks>
    /// <param name="type">The type to bind.</param>
    public static IScopeAssignee Bind(this Configurator configurator, Type type)
    {
        var binding = (IScopeAssignee)IContainerDefinitionBind
            .MakeGenericMethod(type)
            .Invoke(configurator._containerDefinition, []);
        return binding;
    }

    /// <summary>
    /// Multibind a type when the types is unknown at compile time.
    /// </summary>
    public static IScopeAssignee MultiBind(this Configurator configurator, Type src, Type impl, bool toExisting = false)
    {
        var multiBindingBuilder = IContainerDefinitionMultiBind
            .MakeGenericMethod(src)
            .Invoke(configurator._containerDefinition, []);

        var toMethod = multiBindingBuilder.GetType().GetMethod(
            toExisting 
                ? nameof(BindingBuilder<>.ToExisting)
                : nameof(BindingBuilder<>.To),
            InstanceBindingFlags)
            .MakeGenericMethod(impl);

        var scope = (IScopeAssignee)toMethod.Invoke(multiBindingBuilder, []);
        return scope;
    }

    /// <summary>
    /// See <see cref="BindSingleton(Configurator, Type, Type, bool)"/>, this method set <paramref name="alsoBindDestByItself"/> to <see langword="false"/>.
    /// </summary>
    public static Configurator BindSingleton(this Configurator configurator, Type src, Type dst)
        => configurator.BindSingleton(src, dst, false);

    /// <summary>
    /// Bind a source type to a destination type as a singleton.
    /// Similar to <see cref="BindSingleton{T, TImpl}(Configurator)"/> but when you do not know the types at compile time.
    /// </summary>
    /// <param name="src">The source <see cref="Type"/> to bind from.</param>
    /// <param name="dst">The destination <see cref="Type"/> to bind to.</param>
    /// <param name="alsoBindDestByItself">A value indicating whether the destination type should also be bound to itself as a singleton. If <see
    /// langword="true"/>, the destination type will be independently bound as a singleton.</param>
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

    /// <summary>
    /// A convenience and fluent method to bind a type to its implementation as a singleton.
    /// </summary>
    public static Configurator BindSingleton<T, TImpl>(this Configurator configurator)
        where T : class
        where TImpl : class, T
    {
        configurator.Bind<T>().To<TImpl>().AsSingleton();
        return configurator;
    }

    /// <summary>
    /// A convenience and fluent method to bind a type to itself as a singleton.
    /// </summary>
    public static Configurator BindSingleton<T>(this Configurator configurator)
        where T : class
    {
        configurator.Bind<T>().AsSingleton();
        return configurator;
    }

    /// <summary>
    /// Binds a type to itself as a singleton when the type is unknown at compile time.
    /// </summary>
    public static Configurator BindSingleton(this Configurator configurator, Type type)
    {
        configurator.Bind(type).AsSingleton();
        return configurator;
    }

    #endregion

    #region Remove Bindings

    /// <summary>
    /// Removes the bindings for the specified types from the configurator's registry.
    /// </summary>
    /// <remarks>
    /// Use this method to clean up or reset specific type bindings in bulk with better performance.
    /// Does not produce error if the type is not bound.
    /// </remarks>
    /// <param name="types">The types whose bindings should be removed.</param>
    public static Configurator MassRemoveBindings(this Configurator configurator, params IEnumerable<Type> types)
    {
        var registry = GetRegistry(configurator);

        foreach (var t in types)
        {
            registry._boundBindingBuilders.Remove(t);
        }

        return configurator;
    }

    /// <summary>
    /// Removes the binding of the specified type from the configurator.
    /// </summary>
    /// <remarks>
    /// Use <see cref="MassRemoveBindings(Configurator, IEnumerable{Type})"/> to remove multiple bindings at once for better performance.
    /// Does not produce error if the type is not bound.
    /// </remarks>
    public static Configurator RemoveBinding<T>(this Configurator configurator)
        where T : class
        => configurator.MassRemoveBindings(typeof(T));

    /// <summary>
    /// Removes the multibinding sources (and all its implementations) for the specified types from the configurator's registry.
    /// </summary>
    /// <remarks>
    /// Use this method to clean up or reset specific multibinding sources in bulk with better performance.
    /// Does not produce error if the type is not bound.
    /// </remarks>
    /// <param name="types">The types whose multibinding sources should be removed.</param>
    public static Configurator MassRemoveMultibindings(this Configurator configurator, params IEnumerable<Type> types)
    {
        var registry = GetRegistry(configurator);
        foreach (var t in types)
        {
            registry._boundMultiBindingBuilders.Remove(t);
        }
        return configurator;
    }

    /// <summary>
    /// Removes the multibinding source for the specified type from the configurator's registry.
    /// </summary>
    /// <remarks>
    /// Use <see cref="MassRemoveMultibindings(Configurator, IEnumerable{Type})"/> to remove multiple multibinding sources at once for better performance.
    /// Does not produce error if the type is not bound.
    /// </remarks>
    public static Configurator RemoveMultibinding(this Configurator configurator, Type type)
        => configurator.MassRemoveMultibindings(type);

    /// <summary>
    /// Remove the specific multibindings implementations for a given source from the configurator's registry.
    /// </summary>
    /// <remarks>
    /// Does not produce error if the <paramref name="type"/> or any of the <paramref name="types"/> are not bound.
    /// </remarks>
    /// <param name="type">The type of the multibinding source to remove implementations from.</param>
    /// <param name="types">The set of types representing the multibinding implementations to remove from the specified source type.</param>
    public static Configurator RemoveMultibindings(this Configurator configurator, Type type, HashSet<Type> types)
    {
        var registry = GetRegistry(configurator);

        if (!registry._boundMultiBindingBuilders.TryGetValue(type, out var multiBindingBuilder)) { return configurator; }
        for (int i = 0; i < multiBindingBuilder.Count; i++)
        {
            var binding = multiBindingBuilder[i];

            var provisionBindingField = binding.GetType().GetField("_provisionBinding", InstanceBindingFlags);
            var provisionBinding = (ProvisionBinding)provisionBindingField.GetValue(binding);

            if (types.Contains(provisionBinding.Type))
            {
                multiBindingBuilder.RemoveAt(i);
                i--;
            }
        }

        return configurator;
    }

    /// <summary>
    /// See <see cref="RemoveMultibinding(Configurator, Type)"/>.
    /// </summary>
    public static Configurator RemoveMultibinding<T>(this Configurator configurator)
        where T : class
        => configurator.RemoveMultibinding(typeof(T));

    /// <summary>
    /// See <see cref="RemoveMultibindings(Configurator, Type, HashSet{Type})"/>
    /// </summary>
    public static Configurator RemoveMultibindings<T, TImpl>(this Configurator configurator)
        where T : class
        where TImpl : class, T
        => configurator.RemoveMultibindings(typeof(T), [typeof(TImpl)]);

    #endregion

    #region Try Bindings & Checks

    /// <summary>
    /// Checks if a type is already bound in the configurator's registry.
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns><see langword="true"/> if the type is bound, <see langword="false"/> otherwise.</returns>
    public static bool IsBound(this Configurator configurator, Type type)
    {
        var registry = GetRegistry(configurator);
        return registry._boundBindingBuilders.ContainsKey(type);
    }

    /// <inheritdoc cref="IsBound(Configurator, Type)" />
    public static bool IsBound<T>(this Configurator configurator) where T : class
        => configurator.IsBound(typeof(T));

    /// <summary>
    /// Checks if a type is multi-bound in the configurator's registry.
    /// </summary>
    /// <param name="src">
    /// The source type to check for multi-binding.
    /// </param>
    /// <param name="impl">
    /// The implementation type to check against the multi-bound source.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the source type is multi-bound to the implementation type, <see langword="false"/> otherwise.
    /// </returns>
    public static bool IsMultiBound(this Configurator configurator, Type src, Type impl)
    {
        var registry = GetRegistry(configurator);
        if (!registry._boundMultiBindingBuilders.TryGetValue(src, out var bindings)) { return false; }

        return bindings.Any(q =>
            q.GetType().GetField("_provisionBinding", InstanceBindingFlags)
                .GetValue(q) is ProvisionBinding pb
                && pb.Type == impl);
    }

    /// <inheritdoc cref="IsMultiBound(Configurator, Type, Type)"/>
    public static bool IsMultiBound<TSrc, TImpl>(this Configurator configurator)
        where TSrc : class
        where TImpl : class, TSrc 
        => configurator.IsMultiBound(typeof(TSrc), typeof(TImpl));

    /// <summary>
    /// Tries to bind a type to the configurator if it is not already bound.
    /// Unlike <see cref="Configurator.Bind{T}"/>, this method does not throw an exception if the type is already bound.
    /// </summary>
    /// <returns>
    /// An <see cref="IScopeAssignee"/> if the type was successfully bound, or <see langword="null"/> if it was already bound.
    /// </returns>
    public static IScopeAssignee? TryBind(this Configurator configurator, Type type)
    {
        if (configurator.IsBound(type)) { return null; }
        return configurator.Bind(type);
    }

    /// <inheritdoc cref="TryBind(Configurator, Type)"/>
    public static ISingleBindingBuilder<T>? TryBind<T>(this Configurator configurator) where T : class
        => (ISingleBindingBuilder<T>?)configurator.TryBind(typeof(T));

    /// <summary>
    /// Tries to multi-bind a source type to an implementation type if it is not already bound.
    /// </summary>
    /// <remarks>
    /// <paramref name="toExisting"/> is only used when binding, the check does not consider it.
    /// </remarks>
    /// <returns>
    /// An <see cref="IScopeAssignee"/> if the source type was successfully multi-bound to the implementation type, or <see langword="null"/> if it was already bound.
    /// </returns>
    public static IScopeAssignee? TryMultiBind(this Configurator configurator, Type src, Type impl, bool toExisting = false)
    {
        if (configurator.IsMultiBound(src, impl)) { return null; }

        return configurator.MultiBind(src, impl, toExisting);
    }

    /// <inheritdoc cref="TryMultiBind(Configurator, Type, Type, bool)" />
    public static IScopeAssignee? TryMultiBind<TSrc, TImpl>(this Configurator configurator, bool toExisting = false)
        where TSrc : class
        where TImpl : class, TSrc
    {
        return configurator.TryMultiBind(typeof(TSrc), typeof(TImpl), toExisting);
    }

    #endregion

    /// <summary>
    /// Gets the <see cref="BindingBuilderRegistry"/> from the configurator.
    /// </summary>
    public static BindingBuilderRegistry GetRegistry(Configurator configurator)
    {
        var def = (ContainerDefinition)configurator._containerDefinition;
        var registry = (BindingBuilderRegistry)def._bindingBuilderRegistry;

        return registry;
    }

}
