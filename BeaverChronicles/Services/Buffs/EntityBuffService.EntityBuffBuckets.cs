namespace BeaverChronicles.Services.Buffs;

public class EntityBuffBuckets<TBuff>(Dictionary<string, TBuff> limited, Dictionary<string, TBuff> permanent) where TBuff : EntityStatus
{
    public EntityBuffBuckets() : this([], []) { }

    public Dictionary<string, TBuff> Limited { get; } = limited;
    public Dictionary<string, TBuff> Permanent { get; } = permanent;
    public IEnumerable<TBuff> Values => Limited.Values.Concat(Permanent.Values);

    public Dictionary<string, TBuff> this[EntityBuffCategory category]
        => category == EntityBuffCategory.Permanent ? Permanent : Limited;
}
