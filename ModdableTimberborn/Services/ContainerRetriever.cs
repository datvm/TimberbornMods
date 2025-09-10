namespace ModdableTimberborn.Services;

public class ContainerRetriever
{
    static WeakReference<ContainerRetriever>? instance;
    public static ContainerRetriever? InstanceOrNull => instance?.TryGetTarget(out var ins) == true ? ins : null;
    public static ContainerRetriever Instance => InstanceOrNull ?? throw new InvalidOperationException("SingletonRetriever instance is not available.");
    
    public static IContainer Container => Instance.container;

    public static T GetInstance<T>() => Container.GetInstance<T>();
    public static object GetInstance(Type t) => Container.GetInstance(t);
    public static IEnumerable<T> GetInstances<T>() => Container.GetInstances<T>();
    public static IEnumerable<object> GetInstances(Type t) => Container.GetInstances(t);

    readonly IContainer container;
    public ContainerRetriever(IContainer container)
    {
        this.container = container;
        instance = new(this);
    }

}
