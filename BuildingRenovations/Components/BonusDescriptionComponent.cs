namespace BuildingRenovations.Components;

[AddTemplateModule2(typeof(BuildingSpec))]
public class BonusDescriptionComponent : IEntityMultiEffectsDescriber
{
    readonly Dictionary<string, BonusDescription> bonuses = [];

    public void AddBonus(BonusDescription bonus)
    {
        bonuses[bonus.Id] = bonus;
    }

    public void RemoveBonus(string bonusId)
    {
        bonuses.Remove(bonusId);
    }

    public IEnumerable<EntityEffectDescription> DescribeAll(ILoc t, IDayNightCycle dayNightCycle)
    {
        if (bonuses.Count == 0) { yield break; }

        foreach (var bonus in bonuses.Values)
        {
            var remainingDays = bonus.RemainingHours?.Invoke(dayNightCycle);
            var description = bonus.Description(t, dayNightCycle);

            yield return new EntityEffectDescription(
                bonus.Title,
                description,
                remainingDays
            );
        }
    }

}

public record BonusDescription(
    string Id,
    string Title,
    Func<ILoc, IDayNightCycle, string> Description,
    Func<IDayNightCycle, float?>? RemainingHours = null
)
{
    public BonusDescription(
        string Id,
        string Title,
        string Description,
        Func<IDayNightCycle, float?>? RemainingHours = null
    ) : this(Id, Title, (_, _) => Description, RemainingHours) { }
}
