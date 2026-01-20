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

    static readonly MethodInfo BindFragmentMethod;
    static readonly MethodInfo BindOrderedFragmentMethod;

    static UiBuilderExtensions()
    {
        var methods = typeof(UiBuilderExtensions).GetMethods();

        foreach (var m in methods)
        {
            switch (m.Name)
            {
                case nameof(BindFragment) when m.IsGenericMethod:
                    BindFragmentMethod = m;
                    break;
                case nameof(BindOrderedFragment) when m.IsGenericMethod:
                    BindOrderedFragmentMethod = m;
                    break;
            }
        }

        if (BindFragmentMethod is null || BindOrderedFragmentMethod is null)
        {
            throw new InvalidOperationException("Failed to initialize Configurator extension methods.");
        }
    }

    extension(Type type)
    {
        public bool IsEntityPanelFragment()
            => typeof(IEntityPanelFragment).IsAssignableFrom(type);
        public bool IsEntityFragmentOrder()
            => typeof(IEntityFragmentOrder).IsAssignableFrom(type);
    }

    extension(IScopeAssignee assignee)
    {
        public IExportAssignee AsScope(Scope scope) => scope switch
        {
            Scope.Transient => assignee.AsTransient(),
            Scope.Singleton => assignee.AsSingleton(),
            _ => throw new NotSupportedException("Unknown scope: " + scope),
        };
    }

    extension(IContainerDefinition definition)
    {
        /// <summary>
        /// Binds all fragments required by the specified provider type to the container definition.
        /// </summary>
        /// <remarks>This method inspects the public constructor of the specified provider type <typeparamref
        /// name="T"/> to identify its dependencies. Any constructor parameters that implement <see
        /// cref="IEntityPanelFragment"/> are automatically bound to the container as singletons. The provider itself is
        /// also bound to the container as a singleton.</remarks>
        /// <typeparam name="T">The type of the provider that implements <see cref="IProvider{T}"/> for <see cref="EntityPanelModule"/>. The
        /// provider must have exactly one public constructor with parameters.</typeparam>
        public IContainerDefinition BindFragments<T>() where T : class, IProvider<EntityPanelModule>
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
                    var binding = (IScopeAssignee)bindMethod.MakeGenericMethod(p.ParameterType).Invoke(definition, []);
                    binding.AsSingleton();
                }
            }

            definition.MultiBind<EntityPanelModule>().ToProvider<T>().AsSingleton();

            return definition;
        }
    }

    extension(Configurator configurator)
    {
        public Configurator BindAttributes(BindAttributeContext context, Assembly? assembly = null, Scope defaultScope = Scope.Singleton)
        {
            assembly ??= Assembly.GetCallingAssembly();

            AttributeBinder.BindAttributes(assembly, configurator, defaultScope, context);

            return configurator;
        }

        #region Fragments

        /// <summary>
        /// Binds fragments of type <typeparamref name="T"/> to the current configurator.
        /// </summary>
        /// <remarks>This method is an extension method that facilitates the binding of fragments to a
        /// configurator. It is typically used to register fragment providers for dependency injection or modular
        /// configuration.</remarks>
        /// <typeparam name="T">The type of the fragment provider to bind. Must implement <see cref="IProvider{T}"/> with <see
        /// cref="EntityPanelModule"/> as the generic type parameter.</typeparam>
        public Configurator BindFragments<T>() where T : class, IProvider<EntityPanelModule>
        {
            configurator._containerDefinition.BindFragments<T>();
            return configurator;
        }

        /// <summary>
        /// Binds a fragment of type <typeparamref name="T"/> to the configurator.
        /// </summary>
        /// <typeparam name="T">The type of the fragment to bind. Must implement <see cref="IEntityPanelFragment"/>.</typeparam>
        public Configurator BindFragment<T>() where T : IEntityPanelFragment
        {
            configurator.BindFragments<EntityPanelFragmentProvider<T>>();

            return configurator;
        }

        /// <summary>
        /// Binds a fragment of the specified type to the configurator.
        /// </summary>
        /// <param name="t">The type of the fragment to bind. Must implement <see cref="IEntityPanelFragment"/>.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the specified type <paramref name="t"/> does not implement <see cref="IEntityPanelFragment"/>.
        /// </exception>
        public Configurator BindFragment(Type t)
        {
            if (!t.IsEntityPanelFragment())
            {
                throw new InvalidOperationException($"Type {t.FullName} must implement {nameof(IEntityPanelFragment)} to be bound as a fragment.");
            }

            return (Configurator)BindFragmentMethod
                .MakeGenericMethod(t)
                .Invoke(null, [configurator]);
        }

        public Configurator BindFragmentOrder<T>() where T : IEntityFragmentOrder
        {
            configurator.MultiBind<IEntityFragmentOrder>().ToExisting<T>();
            return configurator;
        }

        public Configurator BindOrderedFragment<T>() where T : IEntityPanelFragment, IEntityFragmentOrder
            => configurator
                .BindFragment<T>()
                .BindFragmentOrder<T>();

        public Configurator BindOrderedFragment(Type t)
        {
            if (!t.IsEntityPanelFragment() || !t.IsEntityFragmentOrder())
            {
                throw new InvalidOperationException($"Type {t.FullName} must implement both {nameof(IEntityPanelFragment)} and {nameof(IEntityFragmentOrder)} to be bound as an ordered fragment.");
            }

            return (Configurator)BindOrderedFragmentMethod
                .MakeGenericMethod(t)
                .Invoke(null, [configurator]);
        }

        /// <summary>
        /// Binds an alert fragment of type <typeparamref name="T"/> to the configurator.
        /// </summary>
        public Configurator BindAlertFragment<T>() where T : class, IAlertFragmentWithOrder
        {
            configurator.BindSingleton<T>();
            configurator.MultiBind<AlertPanelModule>().ToProvider<AlertFragmentProvider<T>>().AsSingleton();

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
        public IScopeAssignee Bind(Type type)
        {
            var binding = (IScopeAssignee)IContainerDefinitionBind
                .MakeGenericMethod(type)
                .Invoke(configurator._containerDefinition, []);
            return binding;
        }

        /// <summary>
        /// Binds a source type to a destination type with the specified scope when the types are unknown at compile time.
        /// </summary>
        public Configurator Bind(Type src, Type dst, Scope scope, bool alsoBindDestByItself = false)
        {
            if (alsoBindDestByItself)
            {
                configurator.Bind(dst).AsScope(scope);
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
                var scopeAssignee = (IScopeAssignee)toMethod.Invoke(assignee, []);
                scopeAssignee.AsScope(scope);
            }

            return configurator;
        }

        /// <summary>
        /// Multibind a type when the types is unknown at compile time.
        /// </summary>
        public IScopeAssignee MultiBind(Type src, Type impl, bool toExisting = false)
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
        public Configurator BindSingleton(Type src, Type dst)
            => configurator.BindSingleton(src, dst, false);

        /// <summary>
        /// Bind a source type to a destination type as a singleton.
        /// Similar to <see cref="BindSingleton{T, TImpl}(Configurator)"/> but when you do not know the types at compile time.
        /// </summary>
        /// <param name="src">The source <see cref="Type"/> to bind from.</param>
        /// <param name="dst">The destination <see cref="Type"/> to bind to.</param>
        /// <param name="alsoBindDestByItself">A value indicating whether the destination type should also be bound to itself as a singleton. If <see
        /// langword="true"/>, the destination type will be independently bound as a singleton.</param>
        public Configurator BindSingleton(Type src, Type dst, bool alsoBindDestByItself)
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
        public Configurator BindSingleton<T, TImpl>()
            where T : class
            where TImpl : class, T
        {
            configurator.Bind<T>().To<TImpl>().AsSingleton();
            return configurator;
        }

        /// <summary>
        /// A convenience and fluent method to bind a type to itself as a singleton.
        /// </summary>
        public Configurator BindSingleton<T>()
            where T : class
        {
            configurator.Bind<T>().AsSingleton();
            return configurator;
        }

        /// <summary>
        /// Binds a type to itself as a singleton when the type is unknown at compile time.
        /// </summary>
        public Configurator BindSingleton(Type type)
        {
            configurator.Bind(type).AsSingleton();
            return configurator;
        }

        /// <summary>
        /// A convenience method to multi-bind a type as a multibinding source and also bind it as a singleton.
        /// </summary>
        public Configurator MultiBindAndBindSingleton<T, TImpl>()
            where TImpl : class, T
            where T : class
        {
            configurator.Bind<TImpl>().AsSingleton();
            configurator.MultiBind<T>().ToExisting<TImpl>();

            return configurator;
        }

        public Configurator MultiBindSingleton<T, TImpl>()
            where T : class
            where TImpl : class, T
        {
            configurator.MultiBind<T>().To<TImpl>().AsSingleton();
            return configurator;
        }

        public Configurator MultiBindSingleton<T, TImpl>(bool alsoBindSelf)
            where T : class
            where TImpl : class, T
        {
            return alsoBindSelf
                ? MultiBindAndBindSingleton<T, TImpl>(configurator)
                : MultiBindSingleton<T, TImpl>(configurator);
        }

        public Configurator BindTransient<T>()
            where T : class
        {
            configurator.Bind<T>().AsTransient();
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
        public Configurator MassRemoveBindings(params IEnumerable<Type> types)
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
        public Configurator RemoveBinding<T>()
            where T : class
            => configurator.MassRemoveBindings([typeof(T)]);

        /// <summary>
        /// Removes the multibinding sources (and all its implementations) for the specified types from the configurator's registry.
        /// </summary>
        /// <remarks>
        /// Use this method to clean up or reset specific multibinding sources in bulk with better performance.
        /// Does not produce error if the type is not bound.
        /// </remarks>
        /// <param name="types">The types whose multibinding sources should be removed.</param>
        public Configurator MassRemoveMultibindings(params IEnumerable<Type> types)
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
        public Configurator RemoveMultibinding(Type type)
            => configurator.MassRemoveMultibindings([type]);

        /// <summary>
        /// Remove the specific multibindings implementations for a given source from the configurator's registry.
        /// </summary>
        /// <remarks>
        /// Does not produce error if the <paramref name="type"/> or any of the <paramref name="types"/> are not bound.
        /// </remarks>
        /// <param name="type">The type of the multibinding source to remove implementations from.</param>
        /// <param name="types">The set of types representing the multibinding implementations to remove from the specified source type.</param>
        public Configurator RemoveMultibindings(Type type, HashSet<Type> types)
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
        public Configurator RemoveMultibinding<T>()
            where T : class
            => configurator.RemoveMultibinding(typeof(T));

        /// <summary>
        /// See <see cref="RemoveMultibindings(Configurator, Type, HashSet{Type})"/>
        /// </summary>
        public Configurator RemoveMultibindings<T, TImpl>()
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
        public bool IsBound(Type type)
        {
            var registry = GetRegistry(configurator);
            return registry._boundBindingBuilders.ContainsKey(type);
        }

        /// <inheritdoc cref="IsBound(Configurator, Type)" />
        public bool IsBound<T>() where T : class
            => configurator.IsBound(typeof(T));

        /// <summary>
        /// Tries to get the bound type for a given type in the configurator's registry and outputs it.
        /// </summary>
        /// <param name="configurator">The configurator to check for the bound type.</param>
        /// <param name="type">The type to check for a bound type.</param>
        /// <param name="boundType">The output parameter that will hold the bound type if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the bound type was found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetBound(Type type, [NotNullWhen(true)] out Type? boundType)
        {
            var registry = GetRegistry(configurator);

            if (!registry._boundBindingBuilders.TryGetValue(type, out var bindingBuilder))
            {
                boundType = null;
                return false;
            }

            var provisionBinding = GetProvisionBinding(bindingBuilder);
            boundType = provisionBinding.Type;
            return true;
        }

        /// <inheritdoc cref="TryGetBound(Configurator, Type, out Type?)"/>
        public bool TryGetBound<T>([NotNullWhen(true)] out Type? boundType) where T : class
            => configurator.TryGetBound(typeof(T), out boundType);

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
        public bool IsMultiBound(Type src, Type impl)
        {
            var registry = GetRegistry(configurator);
            if (!registry._boundMultiBindingBuilders.TryGetValue(src, out var bindings)) { return false; }

            return bindings.Any(q => GetProvisionBinding(q).Type == impl);
        }

        /// <inheritdoc cref="IsMultiBound(Configurator, Type, Type)"/>
        public bool IsMultiBound<TSrc, TImpl>()
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
        public IScopeAssignee? TryBind(Type type)
        {
            if (configurator.IsBound(type)) { return null; }
            return configurator.Bind(type);
        }

        /// <inheritdoc cref="TryBind(Configurator, Type)"/>
        public ISingleBindingBuilder<T>? TryBind<T>() where T : class
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
        public IScopeAssignee? TryMultiBind(Type src, Type impl, bool toExisting = false)
        {
            if (configurator.IsMultiBound(src, impl)) { return null; }

            return configurator.MultiBind(src, impl, toExisting);
        }

        /// <inheritdoc cref="TryMultiBind(Configurator, Type, Type, bool)" />
        public IScopeAssignee? TryMultiBind<TSrc, TImpl>(bool toExisting = false)
            where TSrc : class
            where TImpl : class, TSrc
        {
            return configurator.TryMultiBind(typeof(TSrc), typeof(TImpl), toExisting);
        }

        #endregion

    }

    static ProvisionBinding GetProvisionBinding(object bindingBuilder)
    {
        var provisionBindingField = bindingBuilder.GetType()
            .GetField(nameof(BindingBuilder<>._provisionBinding), InstanceBindingFlags);
        return (ProvisionBinding)provisionBindingField.GetValue(bindingBuilder);
    }

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
