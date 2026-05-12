namespace BeaverChronicles.Models;

public interface IEntityStatus
{
    string Id { get; }
    string Title { get; }
    string Description { get; }
    float UntilDay { get; }
}

public readonly record struct LimitedTimeCharacterStatus(
    string Id,
    CharacterType CharacterType,
    BonusStat[] Bonuses,
    string Title,
    string Description,
    float UntilDay = 0
) : IEntityStatus;

public readonly record struct WorkplaceLimitedTimeStatus(
    string Id,
    WorkplaceHelper.WorkplaceFilter WorkplaceFilter,
    BonusStat[] Bonuses,
    string Title,
    string Description,
    float UntilDay = 0
) : IEntityStatus;

public readonly record struct BonusStat(string Bonus, float Amount)
{
    public BonusStat(BonusType Bonus, float Amount) : this(Bonus.ToString(), Amount) { }

    public BonusSpec ToBonusSpec() => new()
    {
        Id = Bonus,
        MultiplierDelta = Amount,
    };

}