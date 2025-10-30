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
    int Order => 0;

    Type Type { get; }
    IEnumerable<EditableBlueprint> Modify(IEnumerable<EditableBlueprint> blueprints);
}

/// <summary>
/// Modify the blueprints of a type
/// </summary>
public abstract class BaseBlueprintModifier<T> : ISpecModifier where T : ComponentSpec
{
    public Type Type { get; } = typeof(T);
    public abstract IEnumerable<EditableBlueprint> Modify(IEnumerable<EditableBlueprint> blueprints);
}

/// <summary>
/// Modify the blueprints of a type, working with named specs
/// </summary>
public abstract class BaseSpecModifier<T> : BaseBlueprintModifier<T> where T : ComponentSpec
{

    public override IEnumerable<EditableBlueprint> Modify(IEnumerable<EditableBlueprint> blueprints)
    {
        var modified = Modify(blueprints.Select(q => new NamedSpec<T>(q.Name, q.GetSpec<T>())));
        foreach (var blueprint in modified)
        {
            yield return new(blueprint.Name, blueprint.Spec);
        }
    }

    protected abstract IEnumerable<NamedSpec<T>> Modify(IEnumerable<NamedSpec<T>> specs);

}
public readonly record struct NamedSpec<T>(string Name, T Spec) where T : ComponentSpec;