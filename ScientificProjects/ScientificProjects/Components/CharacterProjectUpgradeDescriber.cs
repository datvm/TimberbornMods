namespace ScientificProjects.Components;

public class CharacterProjectUpgradeDescriber : BaseComponent, IEntityMultiEffectsDescriber, IAwakableComponent
{
#nullable disable
    EntityUpgradeDescriber characterUpgradeService;
    CharacterTrackerComponent characterTrackerComponent;
#nullable enable

    public Worker? Worker { get; private set; }
    public CharacterType CharacterType { get; private set; }

    [Inject]
    public void Inject(EntityUpgradeDescriber characterUpgradeService)
    {
        this.characterUpgradeService = characterUpgradeService;
    }

    public void Awake()
    {
        characterTrackerComponent = GetComponent<CharacterTrackerComponent>();
    }

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
        => characterUpgradeService.DescribeEffects(characterTrackerComponent);

}
