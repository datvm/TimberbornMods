namespace ScientificProjects.Components;

public class CharacterProjectUpgradeComponent : BaseComponent, IEntityMultiEffectsDescriber, IInitializableEntity, IDeletableEntity
{
#nullable disable
    public BonusTrackerComponent BonusTrackerComponent { get; private set; }
    public BonusTracker BonusTracker => BonusTrackerComponent.BonusTracker;
    CharacterTracker characterTracker;
    EntityUpgradeDescriber characterUpgradeService;
#nullable enable

    public Worker? Worker { get; private set; }
    public CharacterType CharacterType { get; private set; }

    public int Order { get; }

    public bool HasBonus(string id) => BonusTracker.CurrentBonuses.ContainsKey(id);

    [Inject]
    public void Inject(CharacterTracker characterTracker, EntityUpgradeDescriber characterUpgradeService)
    {
        this.characterTracker = characterTracker;
        this.characterUpgradeService = characterUpgradeService;
    }

    public void Awake()
    {
        CharacterType = this.GetCharacterType();
        BonusTrackerComponent = this.GetBonusTracker();
        Worker = GetComponentFast<Worker>();
    }

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
        => characterUpgradeService.DescribeEffects(this, t, dayNightCycle);

    public void DeleteEntity()
    {
        characterTracker.Unregister(this);
    }

    public void InitializeEntity()
    {
        characterTracker.Register(this);
    }
}
