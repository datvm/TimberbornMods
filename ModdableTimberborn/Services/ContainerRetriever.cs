namespace ModdableTimberborn.Services;

/// <summary>
/// Quickly retrieve a reference to the DI container.
/// </summary>
public class ContainerRetriever
{
    static ContainerRetriever? instance;
    public static ContainerRetriever? InstanceOrNull => instance?.Refresh()?.Retriever;
    public static ContainerRetriever Instance => InstanceOrNull ?? throw new InvalidOperationException("SingletonRetriever instance is not available.");

    public static IContainer Container => instance?.Refresh()?.Container ?? throw new InvalidOperationException("SingletonRetriever instance is not available.");

    public static T GetInstance<T>() => Container.GetInstance<T>();
    public static object GetInstance(Type t) => Container.GetInstance(t);
    public static IEnumerable<T> GetInstances<T>() => Container.GetInstances<T>();
    public static IEnumerable<object> GetInstances(Type t) => Container.GetInstances(t);

    readonly WeakReference<IContainer> container;
    public ContainerRetriever(IContainer container)
    {
        this.container = new(container);
        instance = this;
    }

    public (ContainerRetriever Retriever, IContainer Container)? Refresh()
    {
        if (container.TryGetTarget(out var c))
        {
            return (this, c);
        }
        else
        {
            if (Instance == this)
            {
                instance = null;
            }
            return null;
        }
    }

}
