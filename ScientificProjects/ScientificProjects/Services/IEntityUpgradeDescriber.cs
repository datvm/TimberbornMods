namespace ScientificProjects.Services;

public interface IEntityUpgradeDescriber<TComponent> where TComponent : BaseComponent
{
    IEnumerable<EntityEffectDescription> DescribeEffects(TComponent comp, DescribeEffectsParameters parameters);
}

public readonly record struct DescribeEffectsParameters(
    IReadOnlyDictionary<string, ScientificProjectInfo> ActiveProjects,
    EntityUpgradeDescriber CharacterUpgradeDescriber
);
