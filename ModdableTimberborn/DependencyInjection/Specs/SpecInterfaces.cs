namespace ModdableTimberborn.DependencyInjection;

/// <summary>
/// Register your service with this interface to have its Load run before the <see cref="SpecService.Load"/> method is called.
/// </summary>
public interface ISpecServiceFrontRunner : ILoadableSingleton { }

/// <summary>
/// Register your service with this interface to have its Run method called after the <see cref="SpecService.Load"/> method is called.
/// If it is also an <see cref="ILoadableSingleton"/>, its <see cref="ILoadableSingleton.Load"/> will be executed before <see cref="SpecService.Load"/>.
/// </summary>
public interface ISpecServiceTailRunner
{
    /// <summary>
    /// Runs after the <see cref="SpecService.Load"/> method is called.
    /// </summary>
    void Run(SpecService specService);
}

/// <summary>
/// Modify the blueprints of a type
/// </summary>
public interface ISpecModifier
{
    int Order { get; }

    Type Type { get; }
    IEnumerable<Blueprint> Modify(IEnumerable<Blueprint> blueprints);
}

/// <summary>
/// Modify the blueprints of a type
/// </summary>
public abstract class BaseBlueprintModifier<T> : ISpecModifier where T : ComponentSpec
{
    public virtual int Order => 0;
    public Type Type { get; } = typeof(T);
    public abstract IEnumerable<Blueprint> Modify(IEnumerable<Blueprint> blueprints);
}

/// <summary>
/// <inheritdoc />
/// </summary>
public abstract class BaseSpecModifier<T> : BaseBlueprintModifier<T> where T : ComponentSpec
{

    public override IEnumerable<Blueprint> Modify(IEnumerable<Blueprint> blueprints)
    {
        var specs = blueprints.Select(q => q.GetSpec<T>());
        var modifiedSpecs = Modify(specs);

        foreach (var spec in modifiedSpecs)
        {
            yield return new([spec], []);
        }
    }

    protected abstract IEnumerable<T> Modify(IEnumerable<T> specs);

}