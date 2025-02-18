namespace SuperCursor;

public static class ModConfigHelper
{

    public static void CommonConfigure(IContainerDefinition containerDefinition)
    {
        Unbind(containerDefinition, typeof(CursorTool));
        containerDefinition.Bind<CursorTool>().To<SuperCursorTool>().AsSingleton();

        containerDefinition.Bind<SuperCursorText>().AsSingleton();
    }

    public static IContainerDefinition Unbind(this IContainerDefinition c, Type type)
    {
        if (c is not ContainerDefinition def)
        {
            throw new InvalidOperationException($"Container definition is not a {nameof(ContainerDefinition)}");
        }

        if (def._bindingBuilderRegistry is not BindingBuilderRegistry registry)
        {
            throw new InvalidOperationException($"Binding builder registry is not a {nameof(BindingBuilderRegistry)}");
        }

        registry._boundBindingBuilders.Remove(type);

        return c;
    }

    public static ImmutableArray<Type> GetImplementations<TInterface>(Assembly? assembly = default)
    {
        assembly ??= Assembly.GetCallingAssembly();

        return [.. assembly.GetTypes()
            .Where(t => typeof(TInterface).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .OrderBy(q => q.GetCustomAttribute<ServicePriorityAttribute>()?.Priority ?? int.MaxValue)];
    }

    static readonly MethodInfo MultibindToMethod = typeof(IBindingBuilder<>).GetMethod(nameof(IBindingBuilder<object>.To), BindingFlags.Public | BindingFlags.Instance)!;
    public static IContainerDefinition MultibindWithImplementations<TInterface>(this IContainerDefinition c, Assembly? assembly = default)
        where TInterface : class
    {
        assembly ??= Assembly.GetCallingAssembly();

        var types = GetImplementations<TInterface>(assembly);
        foreach (var type in types)
        {
            var multiBind = c.MultiBind<TInterface>();

            var method = multiBind.GetType().GetMethod(nameof(multiBind.To), BindingFlags.Public | BindingFlags.Instance)!.MakeGenericMethod(type);
            var bindResult = (IScopeAssignee) method.Invoke(multiBind, []);

            bindResult.AsSingleton();
        }
        return c;
    }

}

[Context("Game")]
public class GameConfig : IConfigurator
{

    public void Configure(IContainerDefinition c)
    {
        ModConfigHelper.CommonConfigure(c);

        c.MultibindWithImplementations<IObjectDescriber>();
        c.MultibindWithImplementations<ICoordDescriber>();
    }

}

[Context("MapEditor")]
public class MapEditorConfig : IConfigurator
{
    public void Configure(IContainerDefinition containerDefinition)
    {
        ModConfigHelper.CommonConfigure(containerDefinition);
    }
}