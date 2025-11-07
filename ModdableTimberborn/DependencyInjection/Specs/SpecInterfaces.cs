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
    bool ShouldRun => true;

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
public readonly record struct NamedSpec<T>(string Name, T Spec) where T : ComponentSpec;