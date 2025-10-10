namespace ModdableTimberborn.EntityTracker;

public class CharacterTrackerComponent : BaseComponent
{
    public Worker? Worker { get; private set; }
    public CharacterType CharacterType { get; private set; }

    public bool IsWorker => Worker;
    public bool IsBot => CharacterType == CharacterType.Bot;
    public bool IsBeaver => CharacterType.IsBeaver();
    public bool IsAdult => CharacterType == CharacterType.AdultBeaver;
    public bool IsChild => CharacterType == CharacterType.ChildBeaver;

    public BonusTrackerComponent? BonusTrackerComponent { get; private set; }
    public BonusTracker? BonusTracker => BonusTrackerComponent?.BonusTracker;

    public void Awake()
    {
        CharacterType = this.GetCharacterType();
        BonusTrackerComponent = this.GetBonusTracker();
        Worker = GetComponentFast<Worker>();
    }

    public bool HasBonus(string id) => BonusTracker?.CurrentBonuses.ContainsKey(id) == true;

}
