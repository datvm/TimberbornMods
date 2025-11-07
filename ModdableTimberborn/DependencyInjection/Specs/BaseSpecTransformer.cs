namespace ModdableTimberborn.DependencyInjection;

public abstract class BaseSpecTransformer<T> : BaseBlueprintModifier<T> where T : ComponentSpec
{
    public override IEnumerable<EditableBlueprint> Modify(IEnumerable<EditableBlueprint> blueprints)
    {
        foreach (var bp in blueprints)
        {
            bp.TransformSpec<T>(Transform);
            yield return bp;
        }
    }

    public abstract T? Transform(T spec);

}
