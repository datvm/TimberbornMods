namespace BuffDebuff;

public class IdsBuffTarget(IEnumerable<long> targets, IBuffableService buffables, EventBus eventBus) : GlobalBuffTarget(buffables, eventBus)
{

    readonly ImmutableHashSet<long> ids = [.. targets];

    protected override bool Filter(BuffableComponent buffable)
    {
        return ids.Contains(buffable.Id);
    }

}
