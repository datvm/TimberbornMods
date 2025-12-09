namespace Bindito.Core;

public static class BinditoHelper
{
    static BindingBuilderRegistry GetRegistry(this IContainerDefinition containerDefinition)
    {
        if (containerDefinition is not ContainerDefinition def)
        {
            throw new InvalidOperationException($"{nameof(containerDefinition)} is no longer a {nameof(ContainerDefinition)}.");
        }
        if (def._bindingBuilderRegistry is not BindingBuilderRegistry registry)
        {
            throw new InvalidOperationException($"{nameof(def._bindingBuilderRegistry)} is no longer a {nameof(BindingBuilderRegistry)}.");
        }

        return registry;
    }

    public static IContainerDefinition Remove<T>(this IContainerDefinition containerDefinition)
    {
        var registry = containerDefinition.GetRegistry();

        if (!registry._boundBindingBuilders.Remove(typeof(T)))
        {
            throw new InvalidOperationException($"No binding found for {typeof(T)}.");
        }

        return containerDefinition;
    }

    public static Configurator Remove<T>(this Configurator configurator)
    {
        configurator._containerDefinition.Remove<T>();
        return configurator;
    }

    public static Configurator MultiBindAndBindSingleton<TMultiBind, T>(this Configurator configurator) 
        where T : class, TMultiBind
        where TMultiBind : class
    {
        configurator.Bind<T>().AsSingleton();
        configurator.MultiBind<TMultiBind>().ToProvider<SimpleProvider<T>>().AsSingleton();

        return configurator;
    }

    class SimpleProvider<T>(T input) : IProvider<T>
    {
        public T Get() => input;
    }

}
