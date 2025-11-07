namespace ModdableTimberborn.DependencyInjection;

/// <summary>
/// Modify the blueprints of a type, working with named specs.
/// Do not use this if the blueprint has multiple specs, use <see cref="BaseSpecTransformer{T}"/> or <see cref="BaseBlueprintModifier{T}"/> instead.
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
