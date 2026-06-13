namespace BeaverChronicles.Models;

public enum EntityBuffCategory
{
    LimitedTime,
    Permanent
}

public interface IEntityStatus
{
    string Id { get; }
    string Title { get; }
    string Description { get; }
    EntityBuffCategory Category { get; }
    float UntilDay { get; }
}

public interface IEntityStatus<out TEffect> : IEntityStatus
{
    TEffect Effects { get; }
}

public abstract record EntityStatus : IEntityStatus
{
    public string Id { get; init; } = "";
    public string Title { get; init; } = "";
    public string Description { get; init; } = "";
    public EntityBuffCategory Category { get; init; } = EntityBuffCategory.LimitedTime;
    public float UntilDay { get; init; }

    public EntityStatus WithUntilDay(float untilDay) => this with { UntilDay = untilDay };
}

public abstract record EntityStatus<TEffect> : EntityStatus, IEntityStatus<TEffect>
{
    public TEffect Effects { get; init; } = default!;
}

public record CharacterBuffStatus : EntityStatus<BonusStat[]>
{
    public CharacterType CharacterType { get; init; }
}

public record WorkplaceBuffStatus : EntityStatus<BonusStat[]>
{
    public WorkplaceBuffTarget Target { get; init; } = new();
}

public record WorkplaceBuffTarget
{
    public string[] TemplateNames { get; init; } = [];
    public string[] TemplateNamePrefixes { get; init; } = [];
}

public record WaterSourceBuffStatus : EntityStatus<WaterSourceBuffEffects>
{
    public Guid[]? EntityIds { get; init; }
}

public readonly record struct WaterSourceBuffEffects(
    bool? ImmuneToDrought,
    bool? ImmuneToBadtide,
    float? StrengthMultiplier,
    float? ContaminationDelta
);

public readonly record struct BonusStat(string Bonus, float Amount)
{
    public BonusStat(BonusType Bonus, float Amount) : this(Bonus.ToString(), Amount) { }

    public BonusSpec ToBonusSpec() => new()
    {
        Id = Bonus,
        MultiplierDelta = Amount,
    };

}
